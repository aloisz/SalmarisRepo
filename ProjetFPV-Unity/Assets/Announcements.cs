using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Announcements : GenericSingletonClass<Announcements>
{
    [SerializeField] private TextMeshProUGUI textAnnouncement;
    
    [Tooltip("The screen display time of a character. So with 4 characters, word's display time = 4 * this variable")]
    [SerializeField] private float secondsPerCharacter;
    
    public void GenerateAnnouncement(string announcementText)
    {
        StartCoroutine(Announcement(announcementText));
    }

    IEnumerator Announcement(string announcementText)
    {
        textAnnouncement.text = announcementText;
        
        textAnnouncement.enabled = true;

        yield return new WaitForSeconds(announcementText.Length * secondsPerCharacter);
        
        textAnnouncement.enabled = false;
    }
}
