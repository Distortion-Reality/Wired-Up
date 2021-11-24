using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource audioSource;

    public AudioClip background;
    public AudioClip battle;
    public AudioClip bossBattle;
    public AudioClip bossDefeated;

    Dictionary<AudioClip, float> audioClipVolumes;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioClipVolumes = new Dictionary<AudioClip, float>()
        {
            { background, 0.5f },
            { battle, 0.1f },
            { bossBattle, 0.1f },
            { bossDefeated, 0.1f }
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("BossHealthBar(Clone)"))
            PlayAudioClip(bossBattle, audioClipVolumes[bossBattle]);
        else if (!GameObject.Find("KirinBoss(Clone)"))
            PlayAudioClip(bossDefeated, audioClipVolumes[bossDefeated]);
        else if (GameObject.Find("EnemyHealthBar(Clone)"))
            PlayAudioClip(battle, audioClipVolumes[battle]);
        else
            PlayAudioClip(background, audioClipVolumes[background]);
    }

    void PlayAudioClip(AudioClip audioClip, float volume)
    {
        if (audioSource.clip != audioClip)
        {
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
}
