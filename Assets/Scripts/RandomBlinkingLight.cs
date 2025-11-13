using UnityEngine;
using System.Collections;

public class RandomBlinkingLight : MonoBehaviour
{
    [Header("References")]
    public Light lightSource;          // Component Light
    public Renderer bulbRenderer;  
    public AudioSource audioSource;    // Âm thanh khi đèn nhấp nháy
    public AudioClip flickerSound;    // Mesh Renderer (bóng đèn)

    [Header("Blink Settings")]
    public float minBlinkInterval = 0.05f;  // thời gian tối thiểu giữa các lần chớp
    public float maxBlinkInterval = 0.2f;   // thời gian tối đa giữa các lần chớp
    public int minBlinkCount = 3;           // số lần chớp tối thiểu trong 1 đợt
    public int maxBlinkCount = 5;           // số lần chớp tối đa trong 1 đợt

    [Header("Pause Settings")]
    public float minPause = 4f;             // thời gian nghỉ tối thiểu giữa 2 đợt
    public float maxPause = 8f;             // thời gian nghỉ tối đa giữa 2 đợt

    [Header("Colors")]
    public Color onColor = Color.yellow;
    public Color offColor = Color.gray;

    private Material bulbMat;
    private bool isOn = true;

    void Start()
    {
        if (lightSource == null)
            lightSource = GetComponent<Light>();

        if (bulbRenderer == null)
            bulbRenderer = GetComponent<Renderer>();

        // Lấy bản sao material để tránh thay đổi shared material
        bulbMat = bulbRenderer.material;

        // Bật emission để có thể đổi màu sáng
        bulbMat.EnableKeyword("_EMISSION");

        StartCoroutine(RandomBlinkLoop());
    }

    IEnumerator RandomBlinkLoop()
    {
        while (true)
        {   
            yield return new WaitForSeconds(2f);
            
            // --- Nhấp nháy một đợt ---
            int blinkCount = Random.Range(minBlinkCount, maxBlinkCount + 1);
            for (int i = 0; i < blinkCount; i++)
            {
                isOn = !isOn;
                lightSource.enabled = isOn;
                UpdateBulbColor(isOn);
                PlayFlickerSound();
                float interval = Random.Range(minBlinkInterval, maxBlinkInterval);
                yield return new WaitForSeconds(interval);
            }

            // --- Sau khi nhấp nháy xong, giữ sáng trong thời gian nghỉ ---
            isOn = true;
            lightSource.enabled = true;
            UpdateBulbColor(true);

            float pause = Random.Range(minPause, maxPause);
            yield return new WaitForSeconds(pause);
        }
    }

    private void UpdateBulbColor(bool state)
    {
        Color targetColor = state ? onColor : offColor;

        // Dành cho URP Lit: đổi Base Color và Emission
        bulbMat.SetColor("_BaseColor", targetColor);
        bulbMat.SetColor("_EmissionColor", targetColor * (state ? 3f : 0.2f)); // sáng mạnh hơn khi bật
    }
    void PlayFlickerSound()
    {
        if (audioSource != null && flickerSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(flickerSound, Random.Range(0.6f, 1f)); // âm lượng ngẫu nhiên
        }
    }
}
