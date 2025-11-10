using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PickUpItems : MonoBehaviour
{
    public GameObject arrow;
    public int showCameraIndex = 1;    
    public int defaultCameraIndex = 0; 
    public float fadeDuration = 0.5f;  
    public float showDuration = 3f;    

    public void PickUp()
    {
        if (CameraSwitch.Instance == null || CameraSwitch.Instance.cameras.Length == 0) return;

        
        InputManager.CanControl = false;

        int showIndex = Mathf.Clamp(showCameraIndex, 0, CameraSwitch.Instance.cameras.Length - 1);
        int defaultIndex = Mathf.Clamp(defaultCameraIndex, 0, CameraSwitch.Instance.cameras.Length - 1);

        CameraSwitch.Instance.SwitchCamera(showIndex);

        SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
        Image img = arrow.GetComponent<Image>();

        if (sr != null) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
        if (img != null) img.color = new Color(img.color.r, img.color.g, img.color.b, 0);

        Sequence seq = DOTween.Sequence();

        if (sr != null)
            seq.Append(sr.DOFade(1, fadeDuration));
        else if (img != null)
            seq.Append(img.DOFade(1, fadeDuration));

        seq.AppendInterval(showDuration);

        if (sr != null)
            seq.Append(sr.DOFade(0, fadeDuration));
        else if (img != null)
            seq.Append(img.DOFade(0, fadeDuration));

        seq.OnComplete(() =>
        {
            CameraSwitch.Instance.SwitchCamera(defaultIndex);
            
            InputManager.CanControl = true;

            Destroy(gameObject);
        });
    }

}
