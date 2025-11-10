using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PulseEffect : MonoBehaviour
{
    public float duration = 1f;       
    public float maxScale = 3f;       

    private SpriteRenderer sr;
    private Sequence seq;             

    
    private static readonly List<PulseEffect> Instances = new List<PulseEffect>();
    public static bool GlobalVisible { get; private set; } = true;

    private void Awake()
    {
        Instances.Add(this);
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
        KillSequence();
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (GlobalVisible) StartPulse();
        else HideImmediate();
    }

    private void KillSequence()
    {
        if (seq != null)
        {
            seq.Kill();
            seq = null;
        }
    }

    private void HideImmediate()
    {
        KillSequence();
        transform.localScale = Vector3.zero;
        if (sr != null)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
    }

    void StartPulse()
    {
        KillSequence();
        transform.localScale = Vector3.zero;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);

        seq = DOTween.Sequence();
        seq.Append(transform.DOScale(maxScale, duration));
        seq.Join(sr.DOFade(0f, duration));
        seq.OnComplete(() =>
        {
            transform.localScale = Vector3.zero;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
            StartPulse();
        });
    }

    public void SetActive(bool active)
    {
        if (active) StartPulse();
        else HideImmediate();
    }

    public static void SetAllActive(bool active)
    {
        GlobalVisible = active;
        foreach (var p in Instances)
        {
            if (p != null) p.SetActive(active);
        }
    }
}
