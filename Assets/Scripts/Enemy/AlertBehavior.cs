using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertBehavior : MonoBehaviour
{
    public AudioClip alertSound;
    public GameObject player;
    Boolean playedClip = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        transform.Find("AlertDanger").gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (AlertCounter.alertedCounter > 0 || ProxAlertCounter.alertedCounter > 0)
        {
            if (!playedClip)
            {
                AudioSource.PlayClipAtPoint(alertSound, player.transform.position);
                playedClip = true;
            }
            
            transform.Find("AlertDanger").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("AlertDanger").gameObject.SetActive(false);
            playedClip = false;
        }
    }
}
