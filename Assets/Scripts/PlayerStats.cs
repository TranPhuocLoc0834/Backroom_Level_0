using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
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
        PlayerStatsUI.Instance.UpdateStaminaUI(currentStamina / maxStamina);
    }

    // Hàm tiêu hao stamina
    public void ConsumeStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        PlayerStatsUI.Instance.UpdateStaminaUI(currentStamina / maxStamina);
    }

    // Hàm trừ máu khi bị damage
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        PlayerStatsUI.Instance.UpdateHealthUI(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            // Player die
            Debug.Log("Player dead");
        }
    }
}
