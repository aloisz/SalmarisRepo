using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedActivation : MonoBehaviour
{
    [SerializeField] private float delay;
    [SerializeField] private GameObject go;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(nameof(Enable));
    }

    IEnumerator Enable()
    {
        yield return new WaitForSeconds(delay);
        go.SetActive(true);
    }
}
