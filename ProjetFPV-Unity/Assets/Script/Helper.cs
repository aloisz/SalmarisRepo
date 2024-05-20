using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Helper
{
    public static Vector2 ConvertTo4Dir(Vector2 inputVector)
    {
        //checks if the x or y axis is dominant
        bool isAxisVertical = Mathf.Abs(inputVector.y) >= Mathf.Abs(inputVector.x);
        bool isAxisPositive = isAxisVertical ? inputVector.y >= 0 : inputVector.x >= 0;
        if (isAxisVertical) return isAxisPositive ? Vector2.up : Vector2.down;
        else return isAxisPositive ? Vector2.right : Vector2.left;
    }
    
    public static void DecreaseTimerIfPositive(this ref float f)
    {
        if (f > 0) f -= Time.deltaTime;
        if (f < 0) f = 0;
    }
    
    public static void DecreaseTimerIfPositiveNoTimeScale(this ref float f)
    {
        if (f > 0) f -= Time.unscaledDeltaTime;
        if (f < 0) f = 0;
    }

    public static Vector3 ConvertToV3Int(Vector3 v)
    {
        var vector = new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        return vector;
    }

    public static Vector3 ReturnDirFromIndex(int index)
    {
        Vector3 value = Vector3.zero;
        switch (index)
        {
            case 0: value = Vector3.right; break;
            case 1: value = Vector3.left; break;
            case 2: value = Vector3.forward; break;
            case 3: value = Vector3.back; break;
        }
        return value;
    }

    public static Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time, float jumpPower)
    {
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0f;

        float Y = distance.y;
        float XZ = distanceXZ.magnitude;

        float velocityXZ = (XZ * time);
        float velocityY = (jumpPower * time) + (.5f * Mathf.Abs(Physics.gravity.y) * (time * time)); // ((Y ) / time) + 

        Vector3 result = distanceXZ.normalized;
        result *= velocityXZ ;
        result.y = velocityY;

        return result;
    }
    
    public static Vector3 ReturnDirFromTransform(int index, Transform t)
    {
        Vector3 value = Vector3.zero;
        switch (index)
        {
            case 0: value = t.right; break;
            case 1: value = -t.right; break;
            case 2: value = t.forward; break;
            case 3: value = -t.forward; break;
        }
        return value;
    }
    
    #if UNITY_EDITOR
    public static void SetupIconFromEnemyType(GameObject obj, EnemyToSpawn.EnemyKeys enemyType)
    {
        var iconContent = EditorGUIUtility.IconContent
            ($"Assets/Editor/Icons/{Enum.GetName(typeof(EnemyToSpawn.EnemyKeys), enemyType)}.png");
        EditorGUIUtility.SetIconForObject(obj, (Texture2D) iconContent.image);
    }
    #endif
    
    public static void GetInterfaces<T>(out List<T> resultList, GameObject objectToSearch) where T: class 
    {
        MonoBehaviour[] list = objectToSearch.GetComponents<MonoBehaviour>();
        
        resultList = new List<T>();
        
        foreach(MonoBehaviour mb in list)
        {
            if(mb is T)
            {
                //found one
                resultList.Add((T)((System.Object)mb));
            }
        }
    }

    public static Vector3 Vector3Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static void RandomInList<T>(out T randomObject, List<T> referenceList) where T : class
    {
        randomObject = referenceList[Random.Range(0, referenceList.Count)];
    }
    
    public static void DrawBoxCollider(Color gizmoColor, Transform t, BoxCollider boxCollider, float alphaForInsides = 0.3f)
    {
        //Save the color in a temporary variable to not overwrite changes in the inspector (if the sent-in color is a serialized variable).
        var color = gizmoColor;
 
        //Change the gizmo matrix to the relative space of the boxCollider.
        //This makes offsets with rotation work
        Gizmos.matrix = Matrix4x4.TRS(t.TransformPoint(boxCollider.center), t.rotation, t.lossyScale);
 
        //Draws the edges of the BoxCollider
        //Center is Vector3.zero, since we've transformed the calculation space in the previous step.
        Gizmos.color = color;
        Gizmos.DrawWireCube(Vector3.zero, boxCollider.size);
 
        //Draws the sides/insides of the BoxCollider, with a tint to the original color.
        color.a *= alphaForInsides;
        Gizmos.color = color;
        Gizmos.DrawCube(Vector3.zero, boxCollider.size);
    }
    
    
    #if UNITY_EDITOR
    public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color, float thickness)
    {
        if (_color != default(Color))
            Handles.color = _color;
        Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
        using (new Handles.DrawingScope(angleMatrix))
        {
            var pointOffset = (_height - (_radius * 2)) / 2;
 
            //draw sideways
            Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius, thickness);
            Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius), thickness);
            Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius), thickness);
            Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius, thickness);
            //draw frontways
            Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius, thickness);
            Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0), thickness);
            Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0), thickness);
            Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius, thickness);
            //draw center
            Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius, thickness);
            Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius, thickness);
 
        }
    }
    #endif
    
}



