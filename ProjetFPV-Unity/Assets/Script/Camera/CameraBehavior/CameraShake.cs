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

        public float shakeDuration;
        public float shakeMagnitude;
        public float shakeFrequency;
        public AnimationCurve curve;
        public Vector3 shakeRot;

        public Transform cameraShakePos;

        private Vector3 originalPos;
        
        private void Start()
        {
            cameraManager = GetComponent<CameraManager>();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                originalPos = cameraManager.transitionParent.position;
                StartCoroutine(Shake(shakeDuration, shakeMagnitude, shakeFrequency, false));
            }
        }
        
        // Coroutine to shake the camera
        private IEnumerator Shake(float shakeDuration, float shakeMagnitude, float shakeFrequency, bool applyDamping)
        {
            float elapsed = 0.0f;

            while (elapsed < shakeDuration)
            {
                float percentComplete = elapsed / shakeDuration;
                float damper = applyDamping ? curve.Evaluate(percentComplete) : 1.0f;
                float x = Random.Range(-1f, 1f) * shakeMagnitude ;
                float y = Random.Range(-1f, 1f) * shakeMagnitude ;

                cameraShakePos.position += new Vector3(x, y, 0);
                
                
                cameraShakePos.rotation *=
                    Quaternion.Slerp(cameraShakePos.rotation, Quaternion.Euler(shakeRot.x, shakeRot.y,
                            shakeRot.z * (Mathf.Cos(elapsed) * (this.shakeFrequency))),
                        this.shakeFrequency * cameraManager.so_Camera.rotationOffSetSmooth);
                
                

                elapsed += Time.deltaTime * shakeFrequency;

                yield return null;
            }

            cameraShakePos.position = originalPos;
        }
        
    }
}