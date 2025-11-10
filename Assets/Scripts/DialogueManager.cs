using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;  
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    [Header("Timer")]
    public CircularTimer circularTimer; 

    private Dialogue currentDialogue;
    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false); 
    }

    public void StartDialogue(Dialogue dialogueData)
    {
        currentDialogue = dialogueData;
        currentIndex = 0;

        dialoguePanel.SetActive(true);

        InputManager.CanControl = false;

        ShowLine();
    }

    void Update()
    {
        if (currentDialogue == null) return;

        if (Input.GetMouseButtonDown(0)) 
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentDialogue.dialogueLines[currentIndex];
                isTyping = false;
            }
            else
            {
                currentIndex++;
                if (currentIndex < currentDialogue.dialogueLines.Count)
                {
                    ShowLine();
                }
                else
                {
                    EndDialogue(); 
                }
            }
        }
    }

    void ShowLine()
    {
        nameText.text = currentDialogue.speakerName;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeWriter(currentDialogue.dialogueLines[currentIndex]));
    }

    IEnumerator TypeWriter(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f); 
        }
        isTyping = false;
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false); 
        currentDialogue = null;

        InputManager.CanControl = true; 

        if (circularTimer != null)
        {
            circularTimer.StartCountdown();
        }
    }
}
