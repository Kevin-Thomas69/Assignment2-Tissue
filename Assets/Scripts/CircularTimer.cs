using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CircularTimer : MonoBehaviour
{
    [Header("UI")]
    public Image circleImage;          
    public TextMeshProUGUI text;      

    [Header("Countdown Settings")]
    public float totalTime = 10f;     
    public float circleDuration = 1f; 

    private float timeLeft;           
    private float circleTimer = 0f;   
    private bool isCounting = false;
    private bool hasEnded = false;

    public event Action OnCountdownEnd;

    void Start()
    {
        timeLeft = totalTime;
        circleTimer = 0f;
        hasEnded = false;

        if (circleImage != null)
            circleImage.fillAmount = 1f;

        UpdateText();
    }

    void Update()
    {
        if (!isCounting) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0f) timeLeft = 0f;

        circleTimer += Time.deltaTime;
        if (circleTimer >= circleDuration)
            circleTimer = 0f;

        if (circleImage != null)
            circleImage.fillAmount = 1f - (circleTimer / circleDuration);

        UpdateText();

        if (timeLeft <= 0f && !hasEnded)
        {
            hasEnded = true;
            StopCountdown();

            if (Tissue.Instance != null)
                Tissue.Instance.TimeUp();

            OnCountdownEnd?.Invoke();
        }
    }

    private void UpdateText()
    {
        if (text != null)
            text.text = Mathf.CeilToInt(timeLeft).ToString();
    }

    public void StartCountdown()
    {
        timeLeft = totalTime;
        circleTimer = 0f;
        hasEnded = false;
        isCounting = true;

        if (circleImage != null)
            circleImage.fillAmount = 1f;

        UpdateText();
    }

    public void StopCountdown()
    {
        isCounting = false;
    }
}
