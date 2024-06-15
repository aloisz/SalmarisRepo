using System.Collections;
using System.Collections.Generic;
using CameraBehavior;
using DG.Tweening;
using UnityEngine;

public class GameIntroduction : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject Weapon;
    [SerializeField] private GameObject Camera;

    [Header("WayPoint")] 
    [SerializeField] private Transform waitingDoorPos;
    [SerializeField] private Transform facingDir;
    
    
    IEnumerator Start()
    {
        Init(false);
        
        for (int i = 0; i < 7; i++)
        {
            VoicelineManager.Instance.CallVoiceLineINtroduction(i);
            yield return new WaitForSecondsRealtime(VoicelineManager.Instance.scriptable.soundList[i].IASoundID.audioDuration);
            State(i);
        }
        Player.SetActive(true);
    }

    private void Init(bool state)
    {
        Camera.GetComponent<CameraManager>().enabled = state;
        Camera.GetComponentInChildren<HandSwing>().enabled = state;

        Weapon.GetComponentInChildren<Barbatos>().enabled = state;
        
        Weapon.SetActive(state);
        Player.SetActive(state);
    }

    private void State(int id)
    {
        switch (id)
        {
            case 0:
                ShowWeapon();
                FaceCameraToDoor();
                break;
            case 1:
                Init(true);
                break;
        }
    }
    

    private void ShowWeapon()
    {
        Vector3 weaponTrasform = Weapon.transform.position;
        Weapon.transform.DOMove( Weapon.transform.position + Vector3.down * 2, 0);
        Weapon.SetActive(true);
        Weapon.transform.DOMove(weaponTrasform, .2f).SetDelay(1.25f);
    }

    private void FaceCameraToDoor()
    {
        Camera.transform.DORotate(facingDir.eulerAngles, 1.25f);
        Camera.transform.DOMove(waitingDoorPos.position, 2);
    }
    
    void Update()
    {
        
    }
}
