using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayBuildGameObjectName : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TextMeshPro>().text = gameObject.transform.parent.name;
    }
}
