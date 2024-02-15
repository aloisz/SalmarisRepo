using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace CameraBehavior
{
    public class CameraManager : MonoBehaviour
    {
        
        [Header("Bobbing")]
        public float walkingBobbingSpeed = 14f;
        public float bobbingAmount = 0.05f;
        //public PlayerController controller;

        [SerializeField] private float defaultPosY = 0;
        [SerializeField] private float defaultPosX = 0;
        float timer = 0;
        private void LateUpdate()
        {
            HeadBobing();
        }
        
        private void HeadBobing()
        {
            /*if(Mathf.Abs(PlayerController.Instance.moveDirection.x) > 8 || Mathf.Abs(PlayerController.Instance.moveDirection.z) > 8 && PlayerController.Instance.characterController.isGrounded)
            {
                //Player is moving
                timer += Time.deltaTime * walkingBobbingSpeed;
                transform.localPosition = new Vector3(defaultPosX + Mathf.Cos(timer) * bobbingAmount, defaultPosY + Mathf.Sin(timer) * bobbingAmount, transform.localPosition.z);
            }
            else
            {
                //Idle
                timer = 0;
                transform.localPosition = new Vector3(
                    Mathf.Lerp(transform.localPosition.x, defaultPosX, Time.deltaTime * walkingBobbingSpeed), 
                    Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed),
                    transform.localPosition.z);
                
            }*/
        }
    }
}

