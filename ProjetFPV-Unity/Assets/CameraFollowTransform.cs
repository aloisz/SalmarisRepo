using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class CameraFollowTransform : MonoBehaviour
{
    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = PlayerController.Instance.transform.position;
    }
}
