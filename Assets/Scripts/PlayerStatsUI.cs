using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    public static PlayerStatsUI Instance;

    [Header("Player Bars")]
    public Slider healthSlider;
    public Slider staminaSlider;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateHealthUI(float value)
    {
        healthSlider.value = value; // value từ 0 -> 1
    }

    public void UpdateStaminaUI(float value)
    {
        staminaSlider.value = value;
    }
}
