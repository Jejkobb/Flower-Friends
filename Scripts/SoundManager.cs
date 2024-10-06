using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    public float fadeTime = 1f;

    public Sound[] sounds;
    public Sound[] musicTracks;

    public AudioSource sfxSource;
    public AudioSource musicSource;

    private Coroutine musicCoroutine;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SettingsManager.MusicVolume = 0.5f;

        // Update volume settings on awake
        UpdateVolume();

        // Listen for changes in settings
        SettingsManager.OnSettingsChanged += UpdateVolume;
    }

    private void OnDestroy()
    {
        SettingsManager.OnSettingsChanged -= UpdateVolume;
    }

    private void UpdateVolume()
    {
        sfxSource.volume = SettingsManager.SFXVolume;
        musicSource.volume = SettingsManager.MusicVolume;
    }

    void OnEnable()
    {
        Broadcaster.AddListener("PlaySFX", PlaySFX);
        Broadcaster.AddListener("PlayMusic", PlayMusic);
    }

    private void PlaySFX(object obj)
    {
        string sfxName = (string)obj;
        Sound sfx = System.Array.Find(sounds, sound => sound.name == sfxName);
        if (sfx != null)
        {
            sfxSource.PlayOneShot(sfx.clip);
        }
    }

    public void StopMusic()
    {
        musicSource.enabled = false;
    }

    private void PlayMusic(object obj)
    {
        string musicName = (string)obj;
        Sound music = System.Array.Find(musicTracks, track => track.name == musicName);

        if (music != null)
        {
            // Stop any ongoing music coroutines to prevent overlapping
            if (musicCoroutine != null)
            {
                StopCoroutine(musicCoroutine);
            }

            // If music is currently playing, fade out the current track and fade in the new one.
            // Otherwise, just fade in the new track.
            if (musicSource.isPlaying)
            {
                musicCoroutine = StartCoroutine(FadeSwapTracks(musicSource, music.clip, fadeTime));
            }
            else
            {
                musicSource.clip = music.clip;
                musicSource.Play();
                musicCoroutine = StartCoroutine(FadeIn(musicSource, fadeTime));
            }
        }
    }

    private IEnumerator FadeOut(AudioSource source, float fadeTime)
    {
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

    private IEnumerator FadeSwapTracks(AudioSource source, AudioClip newClip, float fadeTime)
    {
        // Fade out
        yield return StartCoroutine(FadeOut(source, fadeTime));

        // Swap and fade in
        source.clip = newClip;
        source.Play();
        yield return StartCoroutine(FadeIn(source, fadeTime));
    }

    private IEnumerator FadeIn(AudioSource source, float fadeTime)
    {
        source.volume = 0;
        float targetVolume = SettingsManager.MusicVolume;

        while (source.volume < targetVolume)
        {
            source.volume += targetVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
    }
}
