using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-500)]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Serializable]
    public class SceneMusic
    {
        public string sceneName;           
        public AudioClip clip;             
        public string resourcesName;       
    }

    [Header("Output & Defaults")]
    public AudioMixerGroup musicOutput;    
    public AudioClip startupClip;          
    public bool playOnStart = true;
    public bool loop = true;
    [Range(0f, 1f)] public float volume = 0.6f;
    [Min(0f)] public float defaultFadeSeconds = 0.8f;

    [Header("Per-Scene Auto Music (Optional)")]
    public List<SceneMusic> sceneBindings = new List<SceneMusic>();

    private AudioSource _a, _b;
    private AudioSource _active, _inactive;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _a = CreateSource("A");
        _b = CreateSource("B");
        _active = _a; _inactive = _b;
        ApplySourceSettings(_a);
        ApplySourceSettings(_b);

        if (playOnStart)
        {
            var firstScene = SceneManager.GetActiveScene().name;
            if (startupClip != null)
            {
                Play(startupClip, 0f);
            }
            else if (TryGetSceneDefault(firstScene, out var sceneClip))
            {
                Play(sceneClip, 0f);
            }
        }

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private AudioSource CreateSource(string tag)
    {
        var go = new GameObject("MusicSource_" + tag);
        go.transform.SetParent(transform, false);
        var src = go.AddComponent<AudioSource>();
        return src;
    }

    private void ApplySourceSettings(AudioSource src)
    {
        src.playOnAwake = false;
        src.loop = loop;
        src.volume = volume;
        src.spatialBlend = 0f; 
        if (musicOutput != null) src.outputAudioMixerGroup = musicOutput;
    }

    private void OnSceneChanged(Scene prev, Scene next)
    {
        if (TryGetSceneDefault(next.name, out var clip))
        {
            Play(clip, defaultFadeSeconds);
        }
    }

    private bool TryGetSceneDefault(string sceneName, out AudioClip clip)
    {
        foreach (var s in sceneBindings)
        {
            if (!string.IsNullOrEmpty(s.sceneName) && s.sceneName == sceneName)
            {
                if (s.clip != null) { clip = s.clip; return true; }
                if (!string.IsNullOrEmpty(s.resourcesName))
                {
                    var loaded = Resources.Load<AudioClip>(s.resourcesName);
                    if (loaded != null) { clip = loaded; return true; }
                }
            }
        }
        clip = null; return false;
    }

    public void Play(AudioClip clip, float fadeSeconds = -1f)
    {
        if (clip == null) return;
        if (fadeSeconds < 0f) fadeSeconds = defaultFadeSeconds;

        
        if (_active.clip == null && !_active.isPlaying)
        {
            _active.clip = clip;
            _active.loop = loop;
            _active.volume = volume;
            _active.Play();
            return;
        }

        
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(CrossfadeCoroutine(clip, fadeSeconds));
    }

    public void PlayByResources(string pathOrName, float fadeSeconds = -1f)
    {
        var clip = Resources.Load<AudioClip>(pathOrName);
        if (clip != null) Play(clip, fadeSeconds);
    }

    public void Stop(float fadeSeconds = -1f)
    {
        if (fadeSeconds < 0f) fadeSeconds = defaultFadeSeconds;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        StartCoroutine(FadeOutCoroutine(fadeSeconds));
    }

    private System.Collections.IEnumerator CrossfadeCoroutine(AudioClip newClip, float seconds)
    {
        _inactive.clip = newClip;
        _inactive.loop = loop;
        _inactive.volume = 0f;
        _inactive.Play();

        float t = 0f;
        float startVol = _active.volume;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            _active.volume = Mathf.Lerp(startVol, 0f, k);
            _inactive.volume = Mathf.Lerp(0f, volume, k);
            yield return null;
        }
        _active.Stop();
        SwapSources();
        _fadeRoutine = null;
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float seconds)
    {
        float t = 0f;
        float startVol = _active.volume;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            _active.volume = Mathf.Lerp(startVol, 0f, k);
            yield return null;
        }
        _active.Stop();
        _active.clip = null;
    }

    private void SwapSources()
    {
        var tmp = _active;
        _active = _inactive;
        _inactive = tmp;
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (_active != null) _active.volume = volume;
        if (_inactive != null && _inactive.isPlaying) _inactive.volume = volume;
    }

    public void SetLoop(bool l)
    {
        loop = l;
        if (_active != null) _active.loop = loop;
        if (_inactive != null) _inactive.loop = loop;
    }

    public void Mute(bool mute)
    {
        if (_active != null) _active.mute = mute;
        if (_inactive != null) _inactive.mute = mute;
    }
}