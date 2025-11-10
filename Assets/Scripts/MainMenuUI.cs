using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        SceneLoader.Load("GameScene");  
    }

    public void QuitGame()
    {
        Debug.Log("�˳���Ϸ");  
        Application.Quit();    
    }
}
