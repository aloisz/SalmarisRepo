using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Announcements : GenericSingletonClass<Announcements>
{
    [SerializeField] private TextMeshProUGUI textAnnouncement;
    
    [Tooltip("The screen display time of a character. So with 4 characters, word's display time = 4 * this variable")]
    [SerializeField] private float secondsPerCharacter;

    private void Start()
    {
        textAnnouncement.text = "";
    }

    public void GenerateAnnouncement(string announcementText)
    {
        StartCoroutine(Announcement(announcementText));
    }

    IEnumerator Announcement(string announcementText)
    {
        textAnnouncement.text = announcementText;
        textAnnouncement.transform.DOScaleY(0, 0f);
        
        textAnnouncement.enabled = true;
        textAnnouncement.transform.DOScaleY(1, .25f);

        yield return new WaitForSeconds(announcementText.Length * secondsPerCharacter);
        
        textAnnouncement.transform.DOScaleY(0, .25f).OnComplete(() =>
        {
            textAnnouncement.enabled = false;
        });
    }
}
