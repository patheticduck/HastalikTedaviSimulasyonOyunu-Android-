using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // =====================================================
    // AUDIO MANAGER - ANA SES YÖNETİM SİSTEMİ
    // =====================================================

    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip[] gameplayMusics;

    [Header("SFX")]
    public AudioClip clickSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip bookOpenCloseSound;
    public AudioClip pageTurnSound;
    public AudioClip notificationSound;

    [Header("Volume Values")]
    [Range(0f, 1f)] public float mainMenuVolume = 1f;
    [Range(0f, 1f)] public float gameplayVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // Sadece sayfa çevirme sesi için ekstra volume çarpanı
    [Range(0f, 1f)] public float pageTurnVolumeMultiplier = 0.5f;

    private Coroutine gameplayRoutine;
    private bool isMainMenuMusicPlaying = false;
    private bool isGameplayMusicPlaying = false;

    // =====================================================
    // INITIALIZATION
    // =====================================================

    // Singleton yapısını kurar ve AudioManager'ın sahneler arasında korunmasını sağlar.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ApplySFXVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =====================================================
    // MUSIC MANAGEMENT
    // =====================================================

    // Ana menü müziğini loop halinde oynatır.
    public void PlayMainMenuMusic()
    {
        StopGameplayMusic();

        if (mainMenuMusic == null || musicSource == null)
            return;

        isMainMenuMusicPlaying = true;
        isGameplayMusicPlaying = false;

        musicSource.Stop();
        musicSource.volume = mainMenuVolume;
        musicSource.clip = mainMenuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Gameplay müzik döngüsünü başlatır.
    public void PlayGameplayMusic()
    {
        if (musicSource == null || gameplayMusics == null || gameplayMusics.Length == 0)
            return;

        isMainMenuMusicPlaying = false;
        isGameplayMusicPlaying = true;

        StopGameplayMusic();
        gameplayRoutine = StartCoroutine(GameplayMusicLoop());
    }

    // Gameplay müziklerini sırayla oynatır.
    private IEnumerator GameplayMusicLoop()
    {
        int index = 0;

        while (true)
        {
            AudioClip clip = gameplayMusics[index];

            if (clip != null)
            {
                musicSource.Stop();
                musicSource.volume = gameplayVolume;
                musicSource.clip = clip;
                musicSource.loop = false;
                musicSource.Play();

                yield return new WaitForSeconds(clip.length);
            }
            else
            {
                yield return null;
            }

            index++;

            if (index >= gameplayMusics.Length)
                index = 0;
        }
    }

    // Gameplay müzik coroutine'ini durdurur.
    public void StopGameplayMusic()
    {
        if (gameplayRoutine != null)
        {
            StopCoroutine(gameplayRoutine);
            gameplayRoutine = null;
        }
    }

    // Çalan tüm müzikleri durdurur.
    public void StopAllMusic()
    {
        StopGameplayMusic();

        isMainMenuMusicPlaying = false;
        isGameplayMusicPlaying = false;

        if (musicSource != null)
            musicSource.Stop();
    }

    // =====================================================
    // SOUND EFFECTS
    // =====================================================

    // Hazır ses efektlerini oynatmak için kullanılan helper metodlar.
    public void PlayClick()
    {
        PlaySFX(clickSound);
    }

    public void PlayWin()
    {
        PlaySFX(winSound);
    }

    public void PlayLose()
    {
        PlaySFX(loseSound);
    }

    public void PlayBookOpenClose()
    {
        PlaySFX(bookOpenCloseSound);
    }

    public void PlayPageTurn()
    {
        PlaySFX(pageTurnSound, pageTurnVolumeMultiplier);
    }

    public void PlayNotification()
    {
        PlaySFX(notificationSound);
    }

    // Verilen ses efektini belirtilen ses çarpanı ile oynatır.
    private void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(clip, volumeMultiplier);
    }

    // =====================================================
    // VOLUME SETTINGS
    // =====================================================

    // Ana menü müzik ses seviyesini günceller.
    public void SetMainMenuVolume(float value)
    {
        mainMenuVolume = Mathf.Clamp01(value);

        if (musicSource != null && isMainMenuMusicPlaying)
            musicSource.volume = mainMenuVolume;
    }

    // Gameplay müzik ses seviyesini günceller.
    public void SetGameplayVolume(float value)
    {
        gameplayVolume = Mathf.Clamp01(value);

        if (musicSource != null && isGameplayMusicPlaying)
            musicSource.volume = gameplayVolume;
    }

    // Genel ses efekti seviyesini günceller.
    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplySFXVolume();
    }

    // Güncel SFX ses seviyesini AudioSource üzerine uygular.
    private void ApplySFXVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
}