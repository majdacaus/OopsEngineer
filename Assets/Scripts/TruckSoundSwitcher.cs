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

        if (Input.GetKeyDown(KeyCode.L))
        {
           // PlayStressMusic();
        }

        bool moving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        if (moving)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }

    void PlayStressMusic()
    {
        if (!isStressPlaying && musicManager != null)
        {
            isStressPlaying = true;
            musicManager.PlayStressTestMusic();
            Debug.Log("L pritisnuto: Pokrećem Stress Test muziku!");
        }
    }

    public void StartEngine()
    {
        gameStarted = true;
    }
}