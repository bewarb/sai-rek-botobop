using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertCounter : MonoBehaviour
{
    public static int alertedCounter;

    private bool detected = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            detected = true;
            alertedCounter++;
            Debug.Log("current count: " + alertedCounter);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            detected = false;
            alertedCounter--;
            Debug.Log("current count: " + alertedCounter);
        }
    }

    private void OnDestroy()
    {
        if (detected) alertedCounter--;
        Debug.Log("current count: " + alertedCounter);
    }
}
