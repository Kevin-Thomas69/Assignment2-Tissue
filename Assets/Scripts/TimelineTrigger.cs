using UnityEngine;
using UnityEngine.Playables;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector director;     
    public Dialogue dialogueData;         
    public bool unlockIfDialogueMissing = true;

    void Start()
    {
        
        InputManager.CanControl = false;

        if (director == null) director = GetComponent<PlayableDirector>();
        if (director != null)
        {
            director.stopped += OnTimelineStopped;
        }
        else if (unlockIfDialogueMissing)
        {
            InputManager.CanControl = true;
            Debug.LogWarning("TimelineTrigger: PlayableDirector missing, unlocking controls.");
        }
    }

    void OnTimelineStopped(PlayableDirector obj)
    {
        
        
        if (DialogueManager.Instance != null && dialogueData != null && dialogueData.dialogueLines != null && dialogueData.dialogueLines.Count > 0)
        {
            DialogueManager.Instance.StartDialogue(dialogueData);
            InputManager.CanControl = false;
        }
        else if (unlockIfDialogueMissing)
        {
            InputManager.CanControl = true;
            Debug.LogWarning("TimelineTrigger: Dialogue missing, unlocking controls after timeline.");
        }
    }

    void OnDestroy()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }
    }
}
