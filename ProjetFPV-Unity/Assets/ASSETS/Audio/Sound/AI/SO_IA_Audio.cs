using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "IA_Audio/IA_Audio", fileName = "new IA_Audio")]
public class SO_IA_Audio : ScriptableObject
{
    [field: Header("-----Audio-----")]
    [field: SerializeField] public List<IASound> soundList;
}

[System.Serializable]
public class IASound
{
    [HorizontalLine(color: EColor.Blue)]
    public int ID;
    public IASoundID IASoundID;
}

[System.Serializable]
public class IASoundID
{
    public AudioClip clip;
    [ResizableTextArea]
    public string text;
    public float audioDuration;
}
