using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CameraBehavior
{
    public class CameraShake: MonoBehaviour
    {
        private CameraManager cameraManager;

        [Header("Pos")]
        public float shakeDuration;
        public float shakeMagnitude;
        public float shakeFrequency;
        public float power;
        
        [Header("Rot")]
        public Vector3 shakeRot;

        public Transform cameraShakePos;
        
        private void Start()    
        {
            cameraManager = GetComponent<CameraManager>();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                StartCoroutine(Shake(shakeDuration, shakeMagnitude, shakeFrequency, true, power));
            }
            cameraShakePos.position = Vector3.Lerp(cameraShakePos.position, cameraManager.transitionParent.position, Time.deltaTime * 100);
        }
        
        
        private IEnumerator Shake(float shakeDuration, float shakeMagnitude, float shakeFrequency, bool applyDamping, float power)
        {
            float elapsed = 0.0f;
            while (elapsed < shakeDuration)
            {
                float percentComplete = elapsed / shakeDuration;
                float damper = applyDamping ? Mathf.Exp(-power * percentComplete) : 1.0f;
                float x = Random.Range(-1f, 1f) * shakeMagnitude * damper ;
                float y = Random.Range(-1f, 1f) * shakeMagnitude * damper;

                cameraShakePos.position = Vector3.Lerp( cameraManager.camera.transform.position, cameraShakePos.position + new Vector3(x, 0, y), percentComplete);
                cameraManager.handSwing.transform.position = 
                    Vector3.Lerp( cameraManager.handSwing.transform.position, cameraManager.handSwing.transform.position + new Vector3(x/10, 0, y/10), percentComplete);
                
                /*cameraShakePos.localRotation *=
                    Quaternion.Slerp(cameraShakePos.localRotation, Quaternion.Euler(x, shakeRot.y, y),
                        percentComplete);*/

                elapsed += Time.deltaTime * shakeFrequency;

                yield return null;
            }
            /*cameraShakePos.localRotation *=
                Quaternion.Slerp(cameraShakePos.rotation, Quaternion.identity, 
                    Time.deltaTime * 1);*/
        }
        
    }
}