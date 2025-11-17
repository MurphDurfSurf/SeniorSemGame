using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void PlaySound(AudioClip clip, Vector3 position, float volume) // For single clip
    {
        AudioSource audioSource = Instantiate(soundFXObject, position, Quaternion.identity);

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        float cliplength = audioSource.clip.length;

        Destroy(audioSource.gameObject, cliplength);
    }

    public void PlayRandomSound(AudioClip[] clips, Vector3 position, float volume) // For array of clips (Varrying SFX)
    {
        int rand = Random.Range(0, clips.Length);

        
        AudioSource audioSource = Instantiate(soundFXObject, position, Quaternion.identity);

        audioSource.clip = clips[rand];
        audioSource.volume = volume;
        audioSource.Play();

        float cliplength = audioSource.clip.length;

        Destroy(audioSource.gameObject, cliplength);
    }
}
