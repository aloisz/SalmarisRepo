using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testIAAUdio : MonoBehaviour
{
    public SO_IA_Audio iaAudio;
    void Start()
    {
        foreach (var sound in iaAudio.soundList)
        {
            if (2 == sound.ID)
            {
                Debug.Log(sound.IASoundID.clip);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
