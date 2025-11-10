using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI fpsText; 

    private float deltaTime = 0.0f;

    void Awake()
    {
        Application.targetFrameRate = -1; 
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        if (fpsText != null)
        {
            fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
        }
    }
}
