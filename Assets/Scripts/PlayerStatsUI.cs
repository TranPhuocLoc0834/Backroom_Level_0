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
        if (healthFill != null)
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, healthTarget, Time.deltaTime * smoothSpeed);

        if (staminaFill != null)
            staminaFill.fillAmount = Mathf.Lerp(staminaFill.fillAmount, staminaTarget, Time.deltaTime * smoothSpeed);
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
