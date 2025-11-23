using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject graphicsPanel;
    public GameObject soundPanel;
    public GameObject optionsPanel;

    [Header("Player/Camera")]
    public FirstPersonController playerController; // gán PlayerController script

    [Header("Audio")]
    public GameObject ambientSound; // gán PlayerController script

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
    public Volume volume;
    public VolumeProfile volumeProfile;
    private Bloom bloom;
    private MotionBlur motionBlur;

    private Resolution[] resolutions;
    private int currentResIndex;
    private string[] textureOptions = { "HIGH", "MEDIUM", "LOW" };
    private int currentTexIndex;
    private InputSystem_Actions input;
    private bool isPaused = false;
    public bool IsPaused => isPaused;
    private GameObject previousPanel;
    private GameObject currentPanel;

    void Awake()
    {
        input = new InputSystem_Actions();
        input.Player.Pause.performed += ctx => TogglePause();
    }

    void OnEnable() => input.Player.Enable();

    void OnDisable() => input.Player.Disable();

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

    void TogglePause()
    {
        if (!isPaused)
        {
            Pause();
            return;
        }

        // Nếu đang ở pausePanel → resume
        if (currentPanel == pausePanel)
        {
            Resume();
            return;
        }

        // Nếu đang ở panel con → back đúng panel cha
        BackOneLevel();
    }

    void BackOneLevel()
    {
        // tắt current panel
        currentPanel.SetActive(false);

        // bật panel cha
        previousPanel.SetActive(true);

        // cập nhật lại currentPanel
        currentPanel = previousPanel;

        // cập nhật previousPanel mới
        if (currentPanel == pausePanel)
            previousPanel = null;
        else
            previousPanel = pausePanel; // mọi panel con đều có cha là options/pause
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        ambientSound.SetActive(false);
        currentPanel = pausePanel;
        previousPanel = null;
        // Dừng FirstPersonController hoàn toàn
        if (playerController != null)
            playerController.enabled = !isPaused;
        // Chuột
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        if (volume != null)
            volume.enabled = false; //
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        ambientSound.SetActive(true);
        pausePanel.SetActive(false);
        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (volume != null)
            volume.enabled = true;
    }

    void SetPanel(GameObject current, GameObject next)
    {
        previousPanel = current;
        currentPanel = next;

        current.SetActive(false);
        next.SetActive(true);
    }

    public void OpenOptions()
    {
        SetPanel(pausePanel, optionsPanel);
    }

    public void CloseOptions()
    {
        previousPanel = null; // khi đóng thì quay về pausePanel
        SetPanel(optionsPanel, pausePanel);
    }

    public void OpenGraphicsSettings()
    {
        SetPanel(optionsPanel, graphicsPanel);
    }

    public void CloseGraphicsSettings()
    {
        SetPanel(graphicsPanel, optionsPanel);
    }

    public void OpenSoundSettings()
    {
        SetPanel(optionsPanel, soundPanel);
    }

    public void CloseSoundSettings()
    {
        previousPanel = optionsPanel;
        SetPanel(soundPanel, optionsPanel);
    }

    void UpdateResolutionText()
    {
        Resolution res = resolutions[currentResIndex];
        resolutionText.text = $"{res.width} x {res.height}";
    }

    public void PreviousResolution()
    {
        currentResIndex--;
        if (currentResIndex < 0)
            currentResIndex = resolutions.Length - 1;
        UpdateResolutionText();
    }

    public void NextResolution()
    {
        currentResIndex++;
        if (currentResIndex >= resolutions.Length)
            currentResIndex = 0;
        UpdateResolutionText();
    }

    // --- Texture Quality ---
    void UpdateTextureText()
    {
        textureText.text = textureOptions[currentTexIndex];
    }

    public void NextTexture()
    {
        currentTexIndex--;
        if (currentTexIndex < 0)
            currentTexIndex = textureOptions.Length - 1;
        UpdateTextureText();
    }

    public void PreviousTexture()
    {
        currentTexIndex++;
        if (currentTexIndex >= textureOptions.Length)
            currentTexIndex = 0;
        UpdateTextureText();
    }

    public void ApplySettings()
    {
        // Áp dụng resolution + full screen
        Resolution res = resolutions[currentResIndex];
        bool isFullScreen = fullScreenToggle.isOn;
        Screen.SetResolution(
            res.width,
            res.height,
            isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed
        );

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

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.sceneToLoad = "MainMenu";
        SceneManager.LoadScene("LoadingScene");
    }
}
