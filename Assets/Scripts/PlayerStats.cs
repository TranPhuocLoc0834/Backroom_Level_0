using UnityEngine;
using StarterAssets;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    private StarterAssetsInputs inputs;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("Stamina Settings")]
    public float sprintConsumeRate = 20f;   // stamina mất mỗi giây khi chạy nhanh
    public float regenRate = 15f;           // stamina hồi mỗi giây khi không chạy nhanh
    public bool isSprinting;
    public bool canSprint = true;
    public FirstPersonController controller;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        inputs = GetComponent<StarterAssetsInputs>();

    }
    void Update()
    {
        bool isMoving = inputs.move.magnitude > 0.1f;
        bool isSprintingKey = inputs.sprint;  // shift
        bool isTryingToSprint = isMoving && isSprintingKey;

       // Nếu đang cố chạy nhưng hết stamina → tắt chạy luôn
        if (!canSprint)
            controller.SprintSpeed = controller.MoveSpeed;

        // Nếu đang chạy nhanh
        if (isTryingToSprint && canSprint)
        {
            controller.SprintSpeed = controller.MoveSpeed * 2;
            ConsumeStamina(20f * Time.deltaTime); // tốc độ tụt
        }
        else
        {
            controller.SprintSpeed = controller.MoveSpeed;
            RestoreStamina(10f * Time.deltaTime); // tốc độ hồi
        }
    }

    // Hàm hồi máu
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        PlayerStatsUI.Instance.UpdateHealthUI(currentHealth / maxHealth);
    }

    // Hàm hồi stamina
    public void RestoreStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (currentStamina > maxStamina * 0.2f)
            canSprint = true; // có lại một ít thể lực -> cho chạy lại

        PlayerStatsUI.Instance.UpdateStaminaUI(currentStamina / maxStamina);
    }
    // Hàm tiêu hao stamina
    public void ConsumeStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (currentStamina <= 0)
            canSprint = false; // hết thể lực -> không cho chạy

        PlayerStatsUI.Instance.UpdateStaminaUI(currentStamina / maxStamina);
    }

    // Hàm trừ máu khi bị damage
    // public void TakeDamage(float amount)
    // {
    //     currentHealth -= amount;
    //     currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    //     PlayerStatsUI.Instance.UpdateHealthUI(currentHealth / maxHealth);

    //     if (currentHealth <= 0)
    //     {
    //         // Player die
    //         Debug.Log("Player dead");
    //     }
    // }
}
