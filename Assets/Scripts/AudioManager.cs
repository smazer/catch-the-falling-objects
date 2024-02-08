using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
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
            return;
        }

        //only happens with original instance
        Init();
    }

    private void Init()
    {
        //if Init is called we are the original instance
        DontDestroyOnLoad(gameObject);
        _audioSource = GetComponent<AudioSource>();
        
        gameObject.AddComponent<AudioListener>();
        _audioSource.Play();
    }
    
}
