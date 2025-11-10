using UnityEngine;


public class UIMusicTrigger : MonoBehaviour
{
    [Tooltip("Resources path, e.g., Audio/Music/Victory (no extension)")] 
    public string resourcesName;
    public AudioClip clip; 
    [Min(0f)] public float fadeSeconds = 0.8f;

    private void OnEnable()
    {
        if (MusicManager.Instance == null) return;
        if (clip != null)
        {
            MusicManager.Instance.Play(clip, fadeSeconds);
        }
        else if (!string.IsNullOrEmpty(resourcesName))
        {
            MusicManager.Instance.PlayByResources(resourcesName, fadeSeconds);
        }
    }
}