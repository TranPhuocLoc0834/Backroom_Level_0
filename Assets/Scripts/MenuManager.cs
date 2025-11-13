using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public class MenuManager: MonoBehaviour
{
    [Header("Panels")]
    public GameObject optionsPanel;
    public GameObject mainMenuPanel;
    public GameObject graphicsPanel;
    public GameObject gameTitle;
    public GameObject subTitle;
    public GameObject soundPanel;
    [Header("Resolution UI")]
    public TextMeshProUGUI resolutionText;
    public Button prevResButton;
    public Button nextResButton;

    [Header("Texture Quality UI")]
    public TextMeshProUGUI textureText;
    public Button prevTexButton;
    public Button nextTexButton;

    [Header("Other Options")]
    public Toggle fullScreenToggle;
    public Toggle vSyncToggle;
    public Toggle bloomToggle;
    public Toggle motionBlurToggle;
    public Button applyButton;
    [Header("Post Processing")]
    public VolumeProfile volumeProfile;
    private Bloom bloom;
    private MotionBlur motionBlur;

    private Resolution[] resolutions;
    private int currentResIndex;
    private string[] textureOptions = { "HIGH", "MEDIUM", "LOW" };
    private int currentTexIndex;

    void Start()
    {
        // Lấy effect
        volumeProfile.TryGet(out bloom);
        volumeProfile.TryGet(out motionBlur);

        // Lấy danh sách resolution
        resolutions = Screen.resolutions;
        currentResIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        UpdateResolutionText();

        // Texture
        currentTexIndex = PlayerPrefs.GetInt("TextureIndex", 0);
        UpdateTextureText();

        // Load PlayerPrefs
        fullScreenToggle.isOn = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        bloomToggle.isOn = PlayerPrefs.GetInt("Bloom", 1) == 1;
        motionBlurToggle.isOn = PlayerPrefs.GetInt("MotionBlur", 1) == 1;
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
    }

    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        gameTitle.SetActive(false);
        subTitle.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        gameTitle.SetActive(true);
        subTitle.SetActive(true);
    }
    public void OpenGraphicsSettings()
    {
        optionsPanel.SetActive(false);
        graphicsPanel.SetActive(true);
    }

    public void CloseGraphicsSettings()
    {
        graphicsPanel.SetActive(false);
        optionsPanel.SetActive(true);
        
    }

    public void OpenSoundSettings()
    {
        optionsPanel.SetActive(false);
        soundPanel.SetActive(true);
    }

    public void CloseSoundSettings()
    {
        soundPanel.SetActive(false);
        optionsPanel.SetActive(true);
        
    }

    void UpdateResolutionText()
    {
        Resolution res = resolutions[currentResIndex];
        resolutionText.text = $"{res.width} x {res.height}";
    }

    public void PreviousResolution()
    {
        currentResIndex--;
        if (currentResIndex < 0) currentResIndex = resolutions.Length - 1;
        UpdateResolutionText();
    }

    public void NextResolution()
    {
        currentResIndex++;
        if (currentResIndex >= resolutions.Length) currentResIndex = 0;
        UpdateResolutionText();
    }

    // --- Texture Quality ---
    void UpdateTextureText()
    {
        textureText.text = textureOptions[currentTexIndex];
    }

    public void PreviousTexture()
    {
        currentTexIndex--;
        if (currentTexIndex < 0) currentTexIndex = textureOptions.Length - 1;
        UpdateTextureText();
    }

    public void NextTexture()
    {
        currentTexIndex++;
        if (currentTexIndex >= textureOptions.Length) currentTexIndex = 0;
        UpdateTextureText();
    }

    public void ApplySettings()
    {
        // Áp dụng resolution + full screen
        Resolution res = resolutions[currentResIndex];
        bool isFullScreen = fullScreenToggle.isOn;
        Screen.SetResolution(res.width, res.height, isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

        // Áp dụng V-Sync
        QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;

        // Texture Quality
        QualitySettings.globalTextureMipmapLimit = currentTexIndex;

        // Bloom
        if (bloom != null)
        {
            bloom.active = bloomToggle.isOn;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = bloomToggle.isOn ? 1f : 0f;
        }

        // Motion Blur
        if (motionBlur != null)
        {
            motionBlur.active = motionBlurToggle.isOn;
            motionBlur.intensity.overrideState = true;
            motionBlur.intensity.value = motionBlurToggle.isOn ? 0.5f : 0f;
        }

        // Lưu PlayerPrefs
        PlayerPrefs.SetInt("ResolutionIndex", currentResIndex);
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.SetInt("TextureIndex", currentTexIndex);
        PlayerPrefs.SetInt("VSync", vSyncToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Bloom", bloomToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("MotionBlur", motionBlurToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Graphics settings applied!");
    }
}
