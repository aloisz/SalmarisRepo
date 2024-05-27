using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using GameObject = UnityEngine.GameObject;


namespace CameraBehavior
{
    //[ExecuteInEditMode]
    public class CameraFrustumCulling : MonoBehaviour
    {
        [Range(0, 20)] public float distancePoint;
        public GameObject[] targets;
        internal CameraManager cameraManager;
        
        public float left = -0.2F;
        public float right = 0.2F;
        public float top = 0.2F;
        public float bottom = -0.2F;
        private void Start()
        {
            cameraManager = GetComponent<CameraManager>();
            targets = GameObject.FindGameObjectsWithTag("Decors");
        }
        
        /*private GameObject[] FindGameObjectsInLayer(int layer)
        {
            var goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
            System.Collections.Generic.List<GameObject> goList = new System.Collections.Generic.List<GameObject>();
            foreach (var t in goArray)
            {
                if (t.layer == layer)
                {
                    goList.Add(t);
                }
            }
            if (goList.Count == 0)
            {
                return null;
            }
            return goList.ToArray();
        }*/

        private void LateUpdate()
        {
            CalculateFrustum();
        }
        private void CalculateFrustum()
        {
            if(targets.Length == 0) return;
            foreach (var target in targets)
            {
                target.SetActive(IsVisible(cameraManager.camera, target));
            }
        }
        
        private bool IsVisible(Camera c, GameObject target)
        {
            
            Matrix4x4 m = PerspectiveOffCenter(left, right, bottom, top, c.nearClipPlane, c.farClipPlane);
            c.projectionMatrix = m;
            
            var planes = GeometryUtility.CalculateFrustumPlanes(c);
            var point = target.transform.position;
            
            
            foreach (var plane in planes)
            {
                if (plane.GetDistanceToPoint(point) < distancePoint)
                {
                    return false;
                }
            }
            return true;
        }
        
        static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
        {
            float x = 2.0F * near / (right - left);
            float y = 2.0F * near / (top - bottom);
            float a = (right + left) / (right - left);
            float b = (top + bottom) / (top - bottom);
            float c = -(far + near) / (far - near);
            float d = -(2.0F * far * near) / (far - near);
            float e = -1.0F;
            Matrix4x4 m = new Matrix4x4();
            m[0, 0] = x;
            m[0, 1] = 0;
            m[0, 2] = a;
            m[0, 3] = 0;
            m[1, 0] = 0;
            m[1, 1] = y;
            m[1, 2] = b;
            m[1, 3] = 0;
            m[2, 0] = 0;
            m[2, 1] = 0;
            m[2, 2] = c;
            m[2, 3] = d;
            m[3, 0] = 0;
            m[3, 1] = 0;
            m[3, 2] = e;
            m[3, 3] = 0;
            return m;
        }
    }
}

