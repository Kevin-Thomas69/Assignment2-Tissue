using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using DG.Tweening;

public class LoadingManager : MonoBehaviour
{
    [Header("UI")]
    public Image loadingBar;           
    public TextMeshProUGUI tipsText;   

    [Header("Progress Bar Animation")]
    public float fillDuration = 0.5f;  
    public Ease easeType = Ease.OutBounce; 

    private AsyncOperation operation;
    private bool isReady = false;
    private bool finalFillTriggered = false; 

    private void Start()
    {
        PlayerPrefs.DeleteKey("NextScene");
        string nextScene = PlayerPrefs.GetString("NextScene", "Playground");

        StartCoroutine(LoadSceneAsync(nextScene));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        tipsText.text = "Loading...";
        finalFillTriggered = false;

        while (!operation.isDone)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (operation.progress < 0.9f)
            {
                loadingBar.DOKill();
                loadingBar.DOFillAmount(targetProgress, fillDuration).SetEase(easeType);
            }
            else if (!finalFillTriggered)
            {
                finalFillTriggered = true;
                loadingBar.DOKill();
                loadingBar.DOFillAmount(1f, fillDuration).SetEase(easeType)
                          .OnComplete(() =>
                          {
                              isReady = true;
                              tipsText.text = "Press any key to Enter"; 
                          });
            }

            yield return null;
        }
    }

    private void Update()
    {
        if (isReady && Input.anyKeyDown)
        {
            operation.allowSceneActivation = true;
        }
    }
}
