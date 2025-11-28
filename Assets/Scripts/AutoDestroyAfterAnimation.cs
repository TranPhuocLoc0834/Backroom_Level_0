using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(AudioSource))] 
public class ConsumableEffectRunner : MonoBehaviour
{
    [Header("When to trigger effect (0..1)")]
    [Range(0f, 1f)]
    public float effectTriggerPercent = 0.5f;

    [Header("Sound effect")]
    [Range(0f, 1f)]
    public float soundTriggerPercent = 0.5f;
    public AudioClip sfx;

    [Header("Effect type")]
    public bool isHeal = false;
    public bool isStamina = false;

    [Header("Stamina buff (only for stamina shot)")]
    public float staminaBuffDuration = 10f;
    public float staminaBuffMultiplier = 0.5f;

    [Header("Animation")]
    public string animName = ""; // nếu để rỗng sẽ dùng anim.clip
    
    [Header("Animation Speed")]
    public float animationSpeed = 1f;

    Animation anim;
    AudioSource audioSource;
    bool triggered = false;
    bool sfxPlayed = false;

    void Awake()
    {
        anim = GetComponent<Animation>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (anim == null)
        {
            Destroy(gameObject, 0.1f);
            return;
        }

        // chọn clip
        AnimationState state = null;
        if (!string.IsNullOrEmpty(animName) && anim[animName] != null)
            state = anim[animName];
        else if (anim.clip != null)
            state = anim[anim.clip.name];

        if (state == null)
        {
            // fallback: destroy soon
            Destroy(gameObject, 1f);
            return;
        }

        StartCoroutine(Run(state));
    }

    IEnumerator Run(AnimationState state)
    {
        // play
        state.speed = animationSpeed;  
        anim.Play(state.name);

        float clipLength = state.length / state.speed; // thời gian thực sau khi tăng speed
        float triggerTime = Mathf.Clamp01(effectTriggerPercent) * clipLength;
        float sfxTime = Mathf.Clamp01(soundTriggerPercent) * clipLength;

        // start SFX coroutine
        if (sfx != null && audioSource != null)
            StartCoroutine(PlaySFXAfterDelay(sfxTime));

        // chờ đến lúc tiêm chạm tay
        if (triggerTime > 0f)
            yield return new WaitForSeconds(triggerTime);

        TriggerEffect();
        

        // chờ phần còn lại của animation
        float remaining = clipLength - triggerTime;
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        Destroy(gameObject); // queue sẽ chạy món tiếp theo
    }

    void TriggerEffect()
    {
        if (triggered) return;
        triggered = true;

        if (isHeal)
            PlayerStats.Instance?.Heal(PlayerStats.Instance.maxHealth);

        if (isStamina)
        {
            PlayerStats.Instance?.RestoreStamina(PlayerStats.Instance.maxStamina);
            PlayerStats.Instance?.ApplyStaminaBuff(staminaBuffDuration, staminaBuffMultiplier);
        }
    }
    IEnumerator PlaySFXAfterDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        audioSource.PlayOneShot(sfx);
    }
}
