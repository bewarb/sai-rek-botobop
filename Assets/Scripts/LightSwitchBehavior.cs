using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitchBehavior : MonoBehaviour
{
    private Light[] lightSources;

    private bool switchOn;
    private bool playerDetected;
    
    void Start()
    {
        var sources = GameObject.FindGameObjectsWithTag("Light");
        lightSources = Array.ConvertAll(sources, source => source.GetComponent<Light>());
        switchOn = true;
        playerDetected = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerDetected) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            switchOn = !switchOn;
            LevelManager.isLightOn = switchOn;
            foreach (Light light in lightSources) {
                light.intensity = switchOn ? 1.0f : 0.2f;
            }
        }
    }

    public void SetPlayerDetected(bool newPlayerDetected)
    {
        playerDetected = newPlayerDetected;
    }
}
