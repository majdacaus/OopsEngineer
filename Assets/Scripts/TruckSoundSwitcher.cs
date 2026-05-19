using UnityEngine;

public class TruckEngine : MonoBehaviour
{
    public AudioClip idleSound;

    private AudioSource audioSource;
    private MusicManager musicManager;
    private bool gameStarted = false;
    private bool isStressPlaying = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = idleSound;

        musicManager = FindObjectOfType<MusicManager>();
    }

    void Update()
    {
        if (!gameStarted) return;

        bool moving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

        if (moving)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            if (!isStressPlaying)
            {
                isStressPlaying = true;
                musicManager.PlayStressTestMusic();
            }
        }
    }

    public void StartEngine()
    {
        gameStarted = true;
    }
}