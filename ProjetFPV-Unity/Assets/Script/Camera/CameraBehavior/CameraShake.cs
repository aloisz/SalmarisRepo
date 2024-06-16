using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace CameraBehavior
{
    public class CameraShake: GenericSingletonClass<CameraShake>
    {
        private CameraManager cameraManager;

        [Header("Global")]
        public float shakeDuration;
        public float shakeFrequency;
        public float power;
        
        [FormerlySerializedAs("shakeMagnitude")] [Header("Pos")]
        public float shakeMagnitudePos;

        [Header("Rot")]
        public float shakeMagnitudeRot;

        public Transform cameraShakePos;

        public override void Awake()
        {
            base.Awake();
        }

        private void Start()    
        {
            cameraManager = GetComponent<CameraManager>();
        }
        
        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ShakeCamera(shakeDuration, shakeMagnitudePos, shakeMagnitudeRot, shakeFrequency, true, power);
            }
            cameraShakePos.position = Vector3.Lerp(cameraShakePos.position, cameraManager.transitionParent.position, Time.deltaTime * 100);
        }

        /// <summary>
        /// Shake Camera Position or Rotation 
        /// </summary>
        /// <param name="isPos">is either Position or Rotation affected</param>
        /// <param name="shakeDuration"></param>
        /// <param name="shakeMagnitude"></param>
        /// <param name="shakeFrequency"></param>
        /// <param name="applyFadeOut"></param>
        /// <param name="power"></param>
        public void ShakeCamera(bool isPos, float shakeDuration, float shakeMagnitude, float shakeFrequency, bool applyFadeOut, float power)
        {
            StopAllCoroutines();
            StartCoroutine(isPos
                ? ShakePos(shakeDuration, shakeMagnitude, shakeFrequency, applyFadeOut, power)
                : ShakeRot(shakeDuration, shakeMagnitude, shakeFrequency, applyFadeOut, power));
        }
        
        /// <summary>
        /// Shake Camera Position and Rotation 
        /// </summary>
        /// <param name="shakeDuration"></param>
        /// <param name="shakeMagnitude"></param>
        /// <param name="shakeFrequency"></param>
        /// <param name="applyFadeOut"></param>
        /// <param name="power"></param>
        /// <param name="isPos"></param>
        public void ShakeCamera(float shakeDuration, float shakeMagnitudePos, float shakeMagnitudeRot, float shakeFrequency, bool applyFadeOut, float power)
        {
            StopAllCoroutines();
            StartCoroutine(ShakePosRot(shakeDuration, shakeMagnitudePos, shakeMagnitudeRot, shakeFrequency, applyFadeOut, power));
        }
        
        
        private IEnumerator ShakePos(float shakeDuration, float shakeMagnitude, float shakeFrequency, bool applyFadeOut, float power)
        {
            float elapsed = 0.0f;
            while (elapsed < shakeDuration)
            {
                float percentComplete = elapsed / shakeDuration;
                float damper = applyFadeOut ? Mathf.Exp(-power * percentComplete) : 1.0f;
                float x = Random.Range(-1f, 1f) * shakeMagnitude * damper ;
                float y = Random.Range(-1f, 1f) * shakeMagnitude * damper;
                float z = Random.Range(-1f, 1f) * shakeMagnitude * damper;

                //cameraShakePos.position = Vector3.Lerp( cameraManager.camera.transform.position, cameraShakePos.position + new Vector3(x, y, 0), percentComplete);
                cameraManager.handSwing.transform.position = 
                    Vector3.Lerp( cameraManager.handSwing.transform.position, cameraManager.handSwing.transform.position + new Vector3(x/10, y/10, z/10), percentComplete);

                elapsed += Time.deltaTime * shakeFrequency;

                yield return null;
            }
        }
        
        private IEnumerator ShakeRot(float shakeDuration, float shakeMagnitudeRot, float shakeFrequency, bool applyFadeOut, float power)
        {
            float elapsed = 0.0f;
            while (elapsed < shakeDuration)
            {
                float percentComplete = elapsed / shakeDuration;
                float damper = applyFadeOut ? Mathf.Exp(-power * percentComplete) : 1.0f;
                float rotX = Random.Range(-1f, 1f) * (shakeMagnitudeRot) * damper;
                float rotY = Random.Range(-1f, 1f) * (shakeMagnitudeRot) * damper;
                float rotZ = Random.Range(-1f, 1f) * (shakeMagnitudeRot) * damper;
                
                cameraManager.smoothOffset =
                    Quaternion.Slerp(cameraManager.smoothOffset, Quaternion.Euler(rotX, rotY,
                            rotZ * (Mathf.Cos(elapsed))),

                        Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);
                
                cameraShakePos.rotation = Quaternion.Slerp(cameraShakePos.rotation,
                    cameraManager.playerTransform.rotation * cameraManager.smoothOffset,
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);

                elapsed += Time.deltaTime * shakeFrequency;

                yield return null;
            }
        }
        
        private IEnumerator ShakePosRot(float shakeDuration, float shakeMagnitudePos, float shakeMagnitudeRot, float shakeFrequency, bool applyFadeOut, float power)
        {
            float elapsed = 0.0f;
            while (elapsed < shakeDuration)
            {
                float percentComplete = elapsed / shakeDuration;
                float damper = applyFadeOut ? Mathf.Exp(-power * percentComplete) : 1.0f;
                float x = Random.Range(-1f, 1f) * shakeMagnitudePos * damper ;
                float y = Random.Range(-1f, 1f) * shakeMagnitudePos * damper;
                float z = Random.Range(-1f, 1f) * shakeMagnitudePos * damper;

                //cameraShakePos.position = Vector3.Lerp( cameraManager.camera.transform.position, cameraShakePos.position + new Vector3(x, y, 0), percentComplete);
                cameraManager.handSwing.transform.position = 
                    Vector3.Lerp( cameraManager.handSwing.transform.position, cameraManager.handSwing.transform.position + new Vector3(x/10, y/10, z/10), percentComplete);
                
                
                float rotX = Random.Range(-1f, 1f) * (shakeMagnitudeRot) * damper ;
                float rotY = Random.Range(-1f, 1f) * (shakeMagnitudeRot) * damper;
                float rotZ = Random.Range(-1f, 1f) * (shakeMagnitudeRot) * damper;
                
                cameraManager.smoothOffset =
                    Quaternion.Slerp(cameraManager.smoothOffset, Quaternion.Euler(rotX, rotY,
                            rotZ * (Mathf.Cos(elapsed))),

                        Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);
                
                cameraShakePos.rotation = Quaternion.Slerp(cameraShakePos.rotation,
                    cameraManager.playerTransform.rotation * cameraManager.smoothOffset,
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);

                elapsed += Time.deltaTime * shakeFrequency;

                yield return null;
            }
        }
        
    }
}