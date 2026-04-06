using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource _audioSource;

    private void Awake()
    {
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

        // Ensure we have an AudioSource to play simple UI/system sounds
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        // Ensure there is at least one AudioListener in the scene/project.
        // If none exists, add one to this persistent AudioManager GameObject.
#if UNITY_2023_1_OR_NEWER
        var existingListener = UnityEngine.Object.FindAnyObjectByType<AudioListener>();
#else
        var existingListener = UnityEngine.Object.FindObjectOfType<AudioListener>();
#endif
        if (existingListener == null)
        {
            if (GetComponent<AudioListener>() == null)
            {
                gameObject.AddComponent<AudioListener>();
                Debug.Log("AudioManager: Nenhum AudioListener encontrado. Foi adicionado um AudioListener ao AudioManager.");
            }
        }
        else
        {
            Debug.Log("AudioManager: AudioListener encontrado na cena.");
        }

        Debug.Log("AudioManager: Inicializado.");
    }

    // Funções de gerenciamento de audio
    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        // Toca o som de forma simples (não interrompe outros sons):
        _audioSource.PlayOneShot(clip);
    }
}
