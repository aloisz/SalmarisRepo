using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalSpawnerManager : GenericSingletonClass<DecalSpawnerManager>
{
    [SerializeField] private GameObject[] decals;

    public enum possibleDecals
    {
        deathEnemy,
        explosionEnemy,
        grenade1,
        grenade2,
        shotgunImpact
    }

    public void SpawnDecal(Vector3 pos, Vector3 normal, possibleDecals index)
    {
        GameObject spawnedObject = Instantiate(decals[(int)index], pos, Quaternion.identity, transform);
        
        // Create the rotation to align the decal to the surface normal
        Quaternion lookRotation = Quaternion.LookRotation(-normal);

        // Generate a random angle for rotation around the normal
        float randomAngle = Random.Range(0f, 360f);

        // Create a quaternion for the random rotation around the surface normal
        Quaternion randomRotation = Quaternion.AngleAxis(randomAngle, spawnedObject.transform.forward);

        // Combine the rotations
        spawnedObject.transform.rotation = lookRotation * randomRotation;
    }
}
