using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalSpawnerManager : GenericSingletonClass<DecalSpawnerManager>
{
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    
    public void SpawnDecal(Vector3 pos, Vector3 normal, string key)
    {
        GameObject spawnedObject = Pooling.instance.Pop(key);
        spawnedObject.transform.position = pos;

        // Create the rotation to align the decal to the surface normal
        Quaternion lookRotation = Quaternion.LookRotation(-normal);

        // Generate a random angle for rotation around the Z-axis of the decal
        float randomAngle = Random.Range(0f, 360f);

        // Create a quaternion for the random rotation around the Z axis
        Quaternion randomRotation = Quaternion.Euler(0, 0, randomAngle);

        // Combine the rotations: align to the normal first, then apply the random Z rotation
        spawnedObject.transform.rotation = lookRotation * randomRotation;

        spawnedObject.GetComponent<DecalParameters>().SpawnDecal(key);
    }
}
