
using UnityEngine;

public class AudioPresenter : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundEffectSource;

    private void Awake()
    {
        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (soundEffectSource != null)
            soundEffectSource.playOnAwake = false;
    }

    public void EnterNode(DialogueNode node)
    {
        if (node == null)
            return;

        switch (node.MusicAction)
        {
            case NodeMusicAction.KeepPrevious:
                break;

            case NodeMusicAction.ChangeMusic:
                ChangeMusic(node.MusicClip);
                break;

            case NodeMusicAction.StopMusic:
                StopMusic();
                break;
        }

        if (node.SoundEffect != null)
            PlaySoundEffect(node.SoundEffect);
    }

    private void ChangeMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip == null || soundEffectSource == null)
            return;

        soundEffectSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
        musicSource.clip = null;
    }

    public void StopAll()
    {
        StopMusic();

        if (soundEffectSource != null)
            soundEffectSource.Stop();
    }
}
