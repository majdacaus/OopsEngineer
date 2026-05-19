using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip bridgeBuildMusic;
    public AudioClip stressTestMusic;
    public AudioClip startMusic;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = startMusic;
        audioSource.Play();
    }

    public void PlayBridgeBuildMusic()
    {
        audioSource.Stop();
        audioSource.clip = bridgeBuildMusic;
        audioSource.Play();
    }

    public void PlayStressTestMusic()
    {
        audioSource.clip = stressTestMusic;
        audioSource.Play();
    }
}