#ifndef SSGI_INCLUDE
#define SSGI_INCLUDE


#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

// This line must be included to ensure LoadSceneNormals automatically unpacks Accurate G-Buffer Normals
#pragma multi_compile _ _GBUFFER_NORMALS_OCT


Texture2D<float4> _GBuffer0;
Texture2D<float3> _ScreenTexture;

SamplerState my_point_clamp_sampler;
SamplerState my_linear_clamp_sampler;

uint os_FrameId;

#ifndef TAU
#define TAU 6.28318530718
#endif

#ifndef PI
#define PI 3.14159265359
#endif

float rand2dTo1d(float2 vec, float2 dotDir = float2(12.9898, 78.233))
{
    float random = dot(sin(vec.xy), dotDir);
    random = frac(sin(random) * 143758.5453);
    return random;
}

float2 rand2dTo2d(float2 vec, float seed = 4605)
{
    return float2(
		rand2dTo1d(vec + seed, float2(12.989, 78.233)),
		rand2dTo1d(vec + seed, float2(39.346, 11.135))
	);
}

// Source: https://blog.demofox.org/2022/01/01/interleaved-gradient-noise-a-different-kind-of-low-discrepancy-sequence/
float IGN(int pixelX, int pixelY, int frameId)
{
#define FRAME_REPEAT_RATE 64
#define OFFSET 5.588238
    float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
    
    float frame = frameId % FRAME_REPEAT_RATE;
    float x = float(pixelX) + OFFSET * frame;
    float y = float(pixelY) + OFFSET * frame;
    
    float f = magic.x * x + magic.y * y;
    return frac(magic.z * frac(f));
}


// Source: https://answers.unity.com/questions/133680/how-do-you-find-the-tangent-from-a-given-normal.html
float3 GetRandomTangent(float3 direction)
{
    float3 forward = float3(0, 0, 1);
    float3 up = float3(0, 1, 0);

    float3 t1 = cross(direction, forward);
    float3 t2 = cross(direction, up);
    float3 t = t2;
    
    if (length(t1) > length(t2))
        t = t1;
    
    return t;
}

float3 GetCosineWeightedDirection(float noise, float3 direction)
{
    direction = normalize(direction);
    float3 bitangent = GetRandomTangent(direction);
    float3 tangent = cross(-direction, bitangent);
    float rho = sqrt(noise);
    
    float phi = TAU * noise;
    
    float3 d = tangent * rho * cos(phi) + bitangent * rho * sin(phi) + direction * sqrt(1.0 - noise);
    
    return normalize(d);
}


float SampleSceneDepth01(float2 uv)
{
    float rawDepth = SampleSceneDepth(uv);
    return Linear01Depth(rawDepth, _ZBufferParams);
}

half Remap0N(half inStop, half outStart, half outStop, half v)
{
    half t = v / inStop;
    return lerp(outStart, outStop, saturate(t));
} 

// Returns % between start and stop
half InverseLerp(half start, half stop, half value)
{
    return (value - start) / (stop - start);
}

half Remap(half inStart, half inStop, half outStart, half outStop, half v)
{
    half t = InverseLerp(inStart, inStop, v);
    return lerp(outStart, outStop, saturate(t));
}

float2 translate(float2 samplePosition, float2 offset){
    //move samplepoint in the opposite direction that we want to move shapes in
    return samplePosition - offset;
}


float rectangle(float2 samplePosition){
    float2 halfSize = float2(0.5, 0.5);
    samplePosition = translate(samplePosition, float2(0.5, 0.5));
    float2 componentWiseEdgeDistance = abs(samplePosition) - halfSize;
    float outsideDistance = length(max(componentWiseEdgeDistance, 0));
    float insideDistance = min(max(componentWiseEdgeDistance.x, componentWiseEdgeDistance.y), 0);
    return abs(insideDistance);
}

float3 ComputeSSGI(float2 uv)
{
    // Parameters
    static const float _RAY_COUNT = 32.0;
    static const float _RAY_STEPS = 12.0;
    static const float _STRENGTH = 1.0;
    float _MAX_DISTANCE = 2.8;
    
    const float maxHits = _RAY_COUNT * _RAY_STEPS;
    const float invMaxHits = rcp(maxHits);
    
    //_MAX_DISTANCE = min(_MAX_DISTANCE, rectangle(uv) * 0.5);
    //const float stepSize = _MAX_DISTANCE / _RAY_STEPS;
    const float stepSize = 0.2;
    const float sceneDepth = SampleSceneDepth01(uv);
    const float3 sceneColor = _ScreenTexture.SampleLevel(my_point_clamp_sampler, uv, 0).rgb;
    const float3 sceneNormal = normalize(SampleSceneNormals(uv));
    
    if (sceneDepth >= 1.0)
    {
        return sceneColor;
    }
    
    
    
    const float3 rayOrigin = float3((uv * 2.0) - 1.0, sceneDepth);
    float3 accumulatedColor = float3(0, 0, 0);
    int hitMask = 0;
    float sum = 0.0;


    float t = (_Time.y * 60.0) % 60.0;
    float2 p = uv.xy * _ScreenParams.xy;
    [loop]
    for (int j = 1; j <= _RAY_COUNT; j++)
    {
        float r = rand2dTo1d(p * j + t.xx);
        
        r = saturate(r);
        const float3 direction = GetCosineWeightedDirection(r, sceneNormal);
        const float3 directionSS = normalize(mul(float4(direction, 0.0), UNITY_MATRIX_V).xyz);
        
        float3 rayPosition = rayOrigin;
        rayPosition += stepSize * rand2dTo1d(uv.xy + j.xx + t.xx);
        //rayPosition += stepSize * (r - 0.5);
        
        for (int i = 1; i < _RAY_STEPS; i++)
        {
            rayPosition += stepSize * directionSS;
            float weight = 1.0 - i / _RAY_STEPS;
            
            const float2 sampleCoord = (rayPosition.xy + 1.0) * 0.5;


            /*
            if (any(rayPosition.xy < -1.0) || any(rayPosition.xy > 1.0))
                break;

            const float d = SampleSceneDepth01(sampleCoord);
            if(d < sceneDepth)
                continue;

            if (d >= 1.0)
                continue;
            
            
            const float3 n = normalize(SampleSceneNormals(sampleCoord));
            const float nIntensity = saturate(dot(n, -sceneNormal));
            
            if(nIntensity <= 0)
                continue;
            */

            accumulatedColor += _ScreenTexture.SampleLevel(my_linear_clamp_sampler, sampleCoord, 0).rgb * weight;
            sum += weight;
            break;
        }
    }
    
    accumulatedColor /= (sum + 1e-4);

    float3 ambientCol = float3(0.1,0.1,0.1);
    //const float3 ambientCol = CalculateIrradianceFromReflectionProbes(SampleSceneNormals(uv), float3(0, 0, 0), 1.0); // see GlobalIllumination.hlsl)
    const float3 albedo = _GBuffer0.SampleLevel(my_point_clamp_sampler, uv, 0).rgb;
    float ambientAccumInterpolator = saturate(hitMask * invMaxHits);
    ambientAccumInterpolator = pow(t, 0.5);
    
    float3 col = lerp(ambientCol, accumulatedColor, ambientAccumInterpolator);
    col = accumulatedColor;
    
    return sceneColor + col * albedo * _STRENGTH;
}


#endif