#pragma warning disable 0649
using UnityEngine;

namespace CubeAdventure.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;

        [Header("Sound Clips")]
        [SerializeField] private AudioClip backgroundMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.4f;
        }

        private void Start()
        {
            if (backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
            }
        }
    }
}
