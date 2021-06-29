using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource audioSource;

    public AudioClip background;
    public AudioClip battle;
    public AudioClip bossBattle;
    public AudioClip bossDefeated;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("EnemyHealthBar(Clone)"))
            PlayAudioClip(battle);
        else if (GameObject.Find("BossHealthBar(Clone)"))
            PlayAudioClip(bossBattle);
        else if (!GameObject.Find("KirinBoss(Clone)"))
            PlayAudioClip(bossDefeated);
        else
            PlayAudioClip(background);
    }

    void PlayAudioClip(AudioClip audioClip)
    {
        if (audioSource.clip != audioClip)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
}
