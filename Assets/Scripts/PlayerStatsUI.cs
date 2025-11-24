using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    public static PlayerStatsUI Instance;

    public Image healthFill;
    public Image staminaFill;

    public float smoothSpeed = 3f;
    private float healthTarget;
    private float staminaTarget;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        healthFill.fillAmount = Mathf.MoveTowards(
            healthFill.fillAmount,
            healthTarget,
            Time.deltaTime * smoothSpeed
        );

        staminaFill.fillAmount = Mathf.MoveTowards(
            staminaFill.fillAmount,
            staminaTarget,
            Time.deltaTime * smoothSpeed
        );

    }

    public void UpdateHealthUI(float value)
    {
        healthTarget = value;
    }

    public void UpdateStaminaUI(float value)
    {
        staminaTarget = value;
    }
}
