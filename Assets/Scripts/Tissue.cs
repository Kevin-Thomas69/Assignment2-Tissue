using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class Tissue : MonoBehaviour
{
    public static Tissue Instance;

    [Header("Countdown System")]
    public CircularTimer circularTimer; 

    [Header("Victory/Fail UI")]
    public Image fade;           
    public GameObject victoryUI; 
    public GameObject failUI;    
    public float fadeDuration = 1f; 
    public TextMeshProUGUI backToMenuText; 
    public float autoReturnDelay = 10f;    

    private bool hasGameEnded = false; 

    void Awake()
    {
        Instance = this;

        if (fade != null)
        {
            fade.gameObject.SetActive(true);
            fade.color = new Color(0, 0, 0, 0);
        }

        if (victoryUI != null) victoryUI.SetActive(false);
        if (failUI != null) failUI.SetActive(false);
        if (backToMenuText != null) backToMenuText.gameObject.SetActive(false);
    }

    public void PickedUpTissue()
    {
        if (hasGameEnded) return;
        hasGameEnded = true;

        if (circularTimer != null)
        {
            circularTimer.StopCountdown();
        }

        ShowResult(true); 

    }

    public void TimeUp()
    {
        if (hasGameEnded) return;
        hasGameEnded = true;

        ShowResult(false); 
    }

    public IEnumerator BackToMenu()
    {
        yield return new WaitForSeconds(autoReturnDelay);
        SceneManager.LoadScene("Menu");
    }

    private void ShowResult(bool isVictory)
    {
        InputManager.CanControl = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (fade != null)
        {
            fade.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                GameObject resultUI = isVictory ? victoryUI : failUI;

                if (resultUI != null)
                {
                    resultUI.SetActive(true);
                    resultUI.transform.localScale = Vector3.zero;
                    resultUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                }

                if (backToMenuText != null)
                {
                    backToMenuText.gameObject.SetActive(true);
                    backToMenuText.text = $"Back to menu in {Mathf.CeilToInt(autoReturnDelay)} sec";
                }
            });
        }
        else
        {
            if (isVictory && victoryUI != null) victoryUI.SetActive(true);
            if (!isVictory && failUI != null) failUI.SetActive(true);
            if (backToMenuText != null)
            {
                backToMenuText.gameObject.SetActive(true);
                backToMenuText.text = $"Back to menu in {Mathf.CeilToInt(autoReturnDelay)} sec";
            }
        }

        StartCoroutine(BackToMenu());
    }
}
