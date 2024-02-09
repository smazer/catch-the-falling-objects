using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    //singleton for a quick exercise, generally would use DI or Service Locator
    private static AudioManager Instance;
    private AudioSource _audioSource;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// set up the audio source and make it persist between scenes, also add an audio listener
    /// prevents any issue with music stopping/restarting when changing scenes
    /// </summary>
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        _audioSource = GetComponent<AudioSource>();
        
        gameObject.AddComponent<AudioListener>();
        _audioSource.Play();
    }
    
}
