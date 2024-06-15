using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameIntroduction : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject Weapon;
    
    
    IEnumerator Start()
    {
        Weapon.SetActive(false);
        Player.SetActive(false);
        for (int i = 0; i < 7; i++)
        {
            VoicelineManager.Instance.CallVoiceLineINtroduction(i);
            yield return new WaitForSecondsRealtime(VoicelineManager.Instance.scriptable.soundList[i].IASoundID.audioDuration);
        }
        Weapon.SetActive(true);
        Player.SetActive(true);
    }
    
    void Update()
    {
        
    }
}
