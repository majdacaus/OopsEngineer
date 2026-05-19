using UnityEngine;

public class TruckEngine : MonoBehaviour
{
    public AudioClip idleSound;

    private AudioSource audioSource;
    private bool gameStarted = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = idleSound;
    }

    void Update()
    {
        if (!gameStarted) return;
    }

    public void StartEngine()
    {
        gameStarted = true;
        audioSource.Play();
    }
}