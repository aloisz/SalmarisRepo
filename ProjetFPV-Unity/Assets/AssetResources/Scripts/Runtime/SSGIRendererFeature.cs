using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal sealed class SSGIRendererFeature : ScriptableRendererFeature
{
    internal sealed class SSGIRenderPass : ScriptableRenderPass
    {
        private const string ssgiShader = "OccaSoftware/SSGI/RenderSSGI";

        private RTHandle ssgi;
        private RTHandle source;

        private const string targetId = "_SSGITarget";

        private const string profilerTag = "OS_SSGI";
        private const string cmdBufferName = "OS_SSGI";

        private Material ssgiMaterial = null;

        internal SSGIRenderPass()
        {
            ssgi = RTHandles.Alloc(Shader.PropertyToID(targetId), name: targetId);
        }


        public void SetTarget(RTHandle colorHandle)
        {
            source = colorHandle;
        }

        internal void Setup()
        {
            ConfigureInput(ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Depth);
        }

        internal bool LoadShaders()
        {
            bool isLoaded = true;
            isLoaded &= LoadShader(ref ssgiMaterial, ssgiShader);
            return isLoaded;
        }

        private bool LoadShader(ref Material m, string path)
        {
            if (m != null)
                return true;

            Shader s = Shader.Find(path);
            m = CoreUtils.CreateEngineMaterial(s);
            return m != null;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.enableRandomWrite = true;
            descriptor.colorFormat = RenderTextureFormat.DefaultHDR;
            descriptor.sRGB = false;
            descriptor.width = Mathf.Max(descriptor.width, 1);
            descriptor.height = Mathf.Max(descriptor.height, 1);
            descriptor.msaaSamples = 1;

            RenderingUtils.ReAllocateIfNeeded(ref ssgi, descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: targetId);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Profiler.BeginSample(profilerTag);
            CommandBuffer cmd = CommandBufferPool.Get(cmdBufferName);

            cmd.SetGlobalTexture(ShaderParams._ScreenTexture, source);
            cmd.SetGlobalVector(
                ShaderParams._ScreenSizePx,
                new Vector2(renderingData.cameraData.cameraTargetDescriptor.width, renderingData.cameraData.cameraTargetDescriptor.height)
            );
            cmd.SetGlobalInt("os_FrameId", Time.frameCount);

            // Write SSGI
            cmd.SetGlobalTexture("_Source", source);
            Blitter.BlitCameraTexture(cmd, source, ssgi, ssgiMaterial, 0);

            Blitter.BlitCameraTexture(cmd, ssgi, source);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            Profiler.EndSample();
        }

        private static class ShaderParams
        {
            public static int _EnvironmentMap = Shader.PropertyToID("_EnvironmentMap");
            public static int _LocalGIProbePosition = Shader.PropertyToID("_LocalGIProbePosition");
            public static int _DiffuseIrradianceData = Shader.PropertyToID("_DiffuseIrradianceData");
            public static int _ScreenTexture = Shader.PropertyToID("_ScreenTexture");
            public static int _ScreenSizePx = Shader.PropertyToID("_ScreenSizePx");
            public static int _LocalGIMaxDistance = Shader.PropertyToID("_LocalGIMaxDistance");
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }
    }


    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    public Settings settings = new Settings();
    SSGIRenderPass ssgiPass;

    public override void Create()
    {
        ssgiPass = new SSGIRenderPass();
        ssgiPass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (IsExcludedCameraType(renderingData.cameraData.camera.cameraType))
            return;

        if (!ssgiPass.LoadShaders())
            return;

        ssgiPass.Setup();
        renderer.EnqueuePass(ssgiPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        ssgiPass.Setup();
        ssgiPass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
        ssgiPass.SetTarget(renderer.cameraColorTargetHandle);
    }

    private bool IsExcludedCameraType(CameraType type)
    {
        switch (type)
        {
            case CameraType.Preview:
                return true;
            case CameraType.Reflection:
                return true;
            default:
                return false;
        }
    }
}
