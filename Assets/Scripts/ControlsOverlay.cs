using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ControlsOverlay : MonoBehaviour
{
    [Header("Display Targets")]
    public TextMeshProUGUI hintText;         

    [Header("Panel Background")]
    public Image background;                 
    [Range(0f,1f)] public float backgroundAlpha = 0.7f; 
    public Color backgroundColor = Color.black;          

    [Header("Toggle & Initial State")]
    public KeyCode toggleKey = KeyCode.F1;   
    public bool startVisible = true;         

    [Header("Content Mode")]
    public bool useManualContent = true;     
    [TextArea]
    public string manualContent =
        "Keyboard instructions:\n" +
        "AdvanceDialogue:Mouse Left\n" +
        "Forward/Backward/Left/Right: W/S/A/D\n" +
        "Running: Shift\n" +
        "Interact /Pick: F\n" +
        "CameraSwitch: 1/2"+
        "Turn on or off prompts: F1";

    [Header("Position")]
    public bool placeBelowFps = true;        
    public float verticalSpacing = 6f;       

    private bool isVisible;                  

    void Awake() { }

    void Start()
    {
        isVisible = startVisible;
        BuildContent();
        ApplyBackground();
        if (placeBelowFps) PositionBelowFps();
        ApplyVisible();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            ApplyVisible();
        }
    }

    private void ApplyVisible()
    {
        if (hintText != null)
            hintText.enabled = isVisible;
        if (background != null)
            background.enabled = isVisible;
    }

    private void ApplyBackground()
    {
        if (background == null) return;
        var c = backgroundColor;
        c.a = backgroundAlpha;
        background.color = c;
    }

    private void PositionBelowFps()
    {
        var fps = FindObjectOfType<FPSDisplay>();
        if (fps == null || hintText == null || fps.fpsText == null) return;

        var fpsRect = fps.fpsText.rectTransform;
        var rect = hintText.rectTransform;

        rect.anchorMin = fpsRect.anchorMin;
        rect.anchorMax = fpsRect.anchorMax;
        rect.pivot = new Vector2(0f, 1f);

        float height = fpsRect.rect.height;
        if (height <= 0f) height = fpsRect.sizeDelta.y;

        rect.anchoredPosition = fpsRect.anchoredPosition + new Vector2(0f, -height - verticalSpacing);

        if (background != null)
        {
            var bRect = background.rectTransform;
            bRect.anchorMin = rect.anchorMin;
            bRect.anchorMax = rect.anchorMax;
            bRect.pivot = rect.pivot;
            bRect.anchoredPosition = rect.anchoredPosition;
        }
    }

    public void BuildContent()
    {
        if (hintText == null) return;

        if (useManualContent && !string.IsNullOrEmpty(manualContent))
        {
            hintText.text = manualContent;
            return;
        }

        var lines = new List<string>();

        lines.Add($"Camera Switch: {GetCameraShortcutDescription()}");

        lines.Add("Interact / Pick: F");               
        lines.Add("Advance Dialogue: Mouse Left");     
        lines.Add("Loading Screen: Press any key");    

        hintText.text = string.Join("\n", lines);
    }

    private string GetCameraShortcutDescription()
    {
        if (CameraSwitch.Instance == null || CameraSwitch.Instance.shortcuts == null)
            return "(not set)";

        var keys = new List<string>();
        for (int i = 0; i < CameraSwitch.Instance.shortcuts.Length; i++)
        {
            keys.Add(ToFriendlyKey(CameraSwitch.Instance.shortcuts[i]));
        }
        return string.Join(" / ", keys);
    }

    private string ToFriendlyKey(KeyCode code)
    {
        switch (code)
        {
            case KeyCode.Alpha0: return "0";
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            case KeyCode.Alpha7: return "7";
            case KeyCode.Alpha8: return "8";
            case KeyCode.Alpha9: return "9";
            default: return code.ToString();
        }
    }
}