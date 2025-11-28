using UnityEngine;
using StarterAssets;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    private StarterAssetsInputs inputs;
    [Header("Audio")]
    public AudioSource breathingAudio;   // kéo file thở gấp vào đây
    public AudioClip heavyBreathingClip; // clip thở gấp
    private bool isBreathing = false;
    

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("Stamina Settings")]
    public float sprintConsumeRate = 0f;   // stamina mất mỗi giây khi chạy nhanh
    public float regenRate = 15f;          // stamina hồi mỗi giây khi không chạy nhanh
    public bool isSprinting;
    public bool canSprint = true;
    public FirstPersonController controller;

    [Header("Temporary Buffs")]
    public float staminaConsumeMultiplier = 1f; // mặc định 1 = tiêu thụ bình thường
    private Coroutine staminaBuffRoutine;

    public float staminaBuffTimeLeft = 0f;
    public bool staminaBuffActive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        PlayerStatsUI.Instance.UpdateHealthUI(currentHealth / maxHealth);
        PlayerStatsUI.Instance.UpdateStaminaUI(currentStamina / maxStamina);

        inputs = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        bool isMoving = inputs.move.magnitude > 0.1f;
        bool isSprintingKey = inputs.sprint;  
        bool isTryingToSprint = isMoving && isSprintingKey;

        // Không cho chạy nếu stamina dưới ngưỡng
        if (!canSprint)
            controller.SprintSpeed = controller.MoveSpeed;

        // Đang cố chạy nhanh và còn stamina
        if (isTryingToSprint && canSprint)
        {
            controller.SprintSpeed = controller.MoveSpeed * 2;
            ConsumeStamina(sprintConsumeRate * staminaConsumeMultiplier * Time.deltaTime);
        }
        else
        {
            controller.SprintSpeed = controller.MoveSpeed;
            RestoreStamina(regenRate * Time.deltaTime);
        }
        //===============================================
        if (staminaBuffActive)
        {
            staminaBuffTimeLeft -= Time.deltaTime;
            if (staminaBuffTimeLeft <= 0f)
            {
                staminaBuffTimeLeft = 0f;
                staminaBuffActive = false;
            }
        }
    }

    // ------------------------------
    //  HỒI MÁU (dùng cho item consumable)
    // ------------------------------
    public void Heal(float amount)
    {
        if (amount <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Cập nhật UI mượt
        PlayerStatsUI.Instance.UpdateHealthUI(currentHealth / maxHealth);
    }
    // ------------------------------
    //  HỒI STAMINA (dùng cho item consumable)
    // ------------------------------

    public void RestoreStamina(float amount)
    {
        if (amount <= 0) return;

        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

         if (currentStamina > maxStamina * 0.5f)
        {
            canSprint = true;

            // ngừng thở gấp
            if (isBreathing)
                StopBreathing();
        }

        // Cập nhật UI mượt
        PlayerStatsUI.Instance.UpdateStaminaUI(currentStamina / maxStamina);
    }

    // ------------------------------
    //  TIÊU HAO STAMINA (khi chạy)
    // ------------------------------
    public void ConsumeStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (currentStamina <= 0f)
        {
            canSprint = false;

            // bật thở gấp
            if (!isBreathing)
                PlayBreathing();
        }

        PlayerStatsUI.Instance.UpdateStaminaUI(currentStamina / maxStamina);
    }
    public void ApplyStaminaBuff(float duration, float multiplier)
    {
        // Nếu đang có buff cũ, hủy trước
        if (staminaBuffRoutine != null)
        StopCoroutine(staminaBuffRoutine);

        staminaBuffTimeLeft = duration;   // thêm
        staminaBuffActive = true;         // thêm
        staminaBuffRoutine = StartCoroutine(StaminaBuffCoroutine(duration, multiplier));
    }

    private IEnumerator StaminaBuffCoroutine(float duration, float multiplier)
    {
        staminaConsumeMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        staminaConsumeMultiplier = 1f; // reset về bình thường
        staminaBuffRoutine = null;
    }
    private void PlayBreathing()
    {
        if (breathingAudio == null || heavyBreathingClip == null) return;

        breathingAudio.clip = heavyBreathingClip;
        breathingAudio.loop = true;
        breathingAudio.Play();

        isBreathing = true;
    }

    private void StopBreathing()
    {
        if (breathingAudio == null) return;

        breathingAudio.Stop();
        isBreathing = false;
    }
}
