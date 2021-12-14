using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance_SM;

    [Header("UI_Audio")]
    public AudioSource BGM;
    public AudioSource EffectSound;

    [Header("Monsters_AudioClip")]
    public AudioClip BGM_1;
    public AudioClip BGM_2;
    public AudioClip attack_Monster;
    public AudioClip attacked_Target;
    public AudioClip attacked_Player;
    public AudioClip spawn_Monster;

    private void Start()
    {
        if (instance_SM == null)
        {
            instance_SM = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void PlayEffectSound(AudioClip clip)
    {
        EffectSound.clip = clip;
        EffectSound.Play();
    }
}