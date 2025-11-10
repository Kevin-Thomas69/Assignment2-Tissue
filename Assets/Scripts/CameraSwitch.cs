using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraSwitch : MonoBehaviour
{
    public static CameraSwitch Instance { get; private set; }

    [Header("Cameras")]
    public Camera[] cameras;

    [Header("Switch Shortcuts")]
    public KeyCode[] shortcuts;

    [Header("Switch AudioListener Too")]
    public bool changeAudioListener = true;

    [Header("Hide PulseEffect on these indices")]
    public int[] hidePulseOnIndices = new int[] { 1 }; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == null) continue;

            cameras[i].enabled = (i == 0);

            if (changeAudioListener)
            {
                var listener = cameras[i].GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = (i == 0);
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < shortcuts.Length; i++)
        {
            if (Input.GetKeyUp(shortcuts[i]))
            {
                if (!InputManager.CanControl) return;
                SwitchCamera(i);
            }
        }
    }

    
    
    
    
    public void SwitchCamera(int index)
    {
        if (index < 0 || index >= cameras.Length)
        {
            Debug.LogError("CameraSwitch: index out of range: " + index);
            return;
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == null) continue;

            bool enableCam = (i == index);
            cameras[i].enabled = enableCam;

            if (changeAudioListener)
            {
                var listener = cameras[i].GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = enableCam;
            }

            if (enableCam)
                Debug.Log("Switched camera: " + cameras[i].name);
        }

        
        bool hidePulse = false;
        if (hidePulseOnIndices != null)
        {
            for (int k = 0; k < hidePulseOnIndices.Length; k++)
            {
                if (hidePulseOnIndices[k] == index)
                {
                    hidePulse = true;
                    break;
                }
            }
        }

        try
        {
            PulseEffect.SetAllActive(!hidePulse);
        }
        catch (Exception)
        {
        }
    }

    
    
    
    public Camera GetCurrentCamera()
    {
        foreach (var cam in cameras)
        {
            if (cam != null && cam.enabled)
                return cam;
        }
        return null;
    }
}
