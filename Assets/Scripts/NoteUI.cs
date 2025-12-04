using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance;

    public GameObject panel;
    public Image noteImage;
    public FirstPersonController controller;
    public StarterAssetsInputs inputs;
    private bool isOpening = false;
    public bool IsOpening => isOpening;
    public TMP_Text contentText; // nếu không dùng text thì để null cũng được

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(NoteData data)
    {
        if (noteImage != null)
            noteImage.sprite = data.image;

        if (contentText != null)
            contentText.text = data.content;

        panel.SetActive(true);
        isOpening = true;
        Cursor.lockState = isOpening ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpening;
        controller.enabled = false;
        inputs.blockQuickInputs = true;
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        isOpening = false;
        panel.SetActive(false);
        Cursor.lockState = isOpening ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpening;
        controller.enabled = true;
        inputs.blockQuickInputs = false;
        Time.timeScale = 1f;
    }
}
