using System.Collections;
using System.Collections.Generic;
using CameraBehavior;
using DG.Tweening;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class GameIntroduction : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject Player; 
    [SerializeField] private GameObject WeaponPos;
    [SerializeField] private GameObject CameraParent;

    [Header("WayPoint")] 
    [SerializeField] private Transform waitingDoorPos;
    [SerializeField] private Transform doorDirection;
    [SerializeField] private Transform OutsidePos;

    [Header("WayPoint")] 
    [SerializeField] private AnimationCurve jumpingCurve;
    IEnumerator Start()
    {
        Init(false);
        
        for (int i = 0; i < 7; i++)
        {
            VoicelineManager.Instance.CallVoiceLineIntroduction(i);
            yield return new WaitForSecondsRealtime(VoicelineManager.Instance.scriptable.soundList[i].IASoundID.audioDuration);
            State(i);
        }
        Player.SetActive(true);
    }

    private void Init(bool state)
    {
        CameraParent.GetComponent<CameraManager>().enabled = state;
        CameraParent.GetComponentInChildren<HandSwing>().enabled = state;

        WeaponPos.GetComponentInChildren<Barbatos>().enabled = state;   
        if (!state)
        {
            baseWeaponPos = WeaponPos.transform.position;
            WeaponPos.transform.DOMove( WeaponPos.transform.position + Vector3.down * 2, 0);
        }

        Player.GetComponent<PlayerController>().enabled = state;
        Player.GetComponent<Rigidbody>().isKinematic = !state;
        
    }

    private void State(int id)
    {
        switch (id)
        {
            case 5:
                ShowWeapon();
                
                break;
            case 6:
                FaceCameraToDoor();
                break;
        }
    }

    private Vector3 baseWeaponPos;
    private void ShowWeapon()
    {
        WeaponPos.transform.DOMove(baseWeaponPos, .2f).SetDelay(0.25f).OnComplete((() => 
            CameraParent.GetComponentInChildren<HandSwing>().enabled = true));
    }

    private void FaceCameraToDoor()
    {
        CameraParent.transform.DORotate(waitingDoorPos.eulerAngles, 1.25f);
        CameraParent.transform.DOMove(waitingDoorPos.position, 2).SetDelay(1.25f).OnComplete(() => 
            JumpOutSide());
    }

    private void JumpOutSide()
    {
        CameraParent.transform.DORotate(doorDirection.eulerAngles, .5f);  
        StartCoroutine(WaitToLookBeforeJump());
        StartCoroutine(Jump());
    }

    private IEnumerator WaitToLookBeforeJump()
    {
        yield return new WaitForSeconds(2);
        CameraParent.transform.DORotate(OutsidePos.eulerAngles, .2f);
    }
    private IEnumerator Jump()
    {
        yield return new WaitForSeconds(.8f);
        CameraParent.transform.DOJump(OutsidePos.position, 10, 1, 1.25f).SetDelay(1).SetEase(jumpingCurve).OnComplete(() =>
            Init(true));
    }
    
}
