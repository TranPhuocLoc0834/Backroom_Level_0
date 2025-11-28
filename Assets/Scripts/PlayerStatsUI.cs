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

    [Header("Buff UI")]
    public TMPro.TextMeshProUGUI staminaBuffTimerText;
    

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
        if (PlayerStats.Instance != null && PlayerStats.Instance.staminaBuffActive)
        {
            float t = PlayerStats.Instance.staminaBuffTimeLeft;
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);
            if (staminaBuffTimerText != null)
            {
                staminaBuffTimerText.text = $"{m}:{s:00}";
                if (!staminaBuffTimerText.gameObject.activeSelf)
                    staminaBuffTimerText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (staminaBuffTimerText != null && staminaBuffTimerText.gameObject.activeSelf)
                staminaBuffTimerText.gameObject.SetActive(false);
        }
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
