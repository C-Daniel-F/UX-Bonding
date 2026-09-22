using UnityEngine;

public class AudioPresenter : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundEffectSource;


    private AudioClip currentMusic;


    private void Awake()
    {
        if (musicSource != null)
        {
            musicSource.loop = true;
        }
    }



    public void UpdateMusic(StoryScene scene)
    {
        if (scene == null)
        {
            return;
        }

        AudioClip newMusic = scene.Music;

        if (newMusic == null)
        {
            return;
        }


        if (newMusic == currentMusic &&
            musicSource.isPlaying)
        {
            return;
        }

        currentMusic = newMusic;

        musicSource.clip = currentMusic;
        musicSource.Play();
    }


    public void PlaySoundEffect(AudioClip soundEffect)
    {
        if (soundEffect == null ||
            soundEffectSource == null)
        {
            return;
        }

        soundEffectSource.PlayOneShot(soundEffect);
    }


    // =========================
    // STOP
    // =========================

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }

        currentMusic = null;
    }


    public void StopAll()
    {
        StopMusic();

        if (soundEffectSource != null)
        {
            soundEffectSource.Stop();
        }
    }
}