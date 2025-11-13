using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;

public class NewGameDollyTransition : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup uiGroup;            
    public Image fadeImage;                
    public CinemachineDollyCart dollyCart; 
    public float fadeDuration = 1f;      
    public float travelSpeed = 100f;       
    public string nextSceneName = "GameStage1";
    public float accelOffset = 0f;

    private bool isTransitioning = false;

    void Start()
    {
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);
        if (dollyCart != null)
        {
            dollyCart.m_Position = 0f;
            dollyCart.m_Speed = 0f;
        }
    }

    public void OnNewGamePressed()
    {
        if (!isTransitioning)
            StartCoroutine(TransitionSequence());
    }

    IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        // 1. Fade UI
        yield return StartCoroutine(FadeCanvasGroup(uiGroup, 1f, 0f, fadeDuration));

        // 2. Di chuyển dolly dựa trên chiều dài thật của path
        float pathLength = dollyCart.m_Path.PathLength; // Độ dài thật của track
        float startPos = 0f;
        float endPos = pathLength;
        dollyCart.m_Position = startPos;
        float fadeStartPos = pathLength * 0.5f; // khi đến 80% đường thì fade
        bool fadeStarted = false;

        float currentSpeed = 0f;
        float maxSpeed = travelSpeed;
        float accel = travelSpeed / accelOffset; // tốc độ tăng (tùy chỉnh)
        dollyCart.m_Position = 0f;

        while (dollyCart.m_Position < endPos)
        {
            // Tăng dần tốc độ
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, accel * Time.deltaTime);
            dollyCart.m_Position += currentSpeed * Time.deltaTime;

            // Bắt đầu fade khi đến giữa đường
            if (!fadeStarted && dollyCart.m_Position >= fadeStartPos)
            {
                fadeStarted = true;
                StartCoroutine(FadeImage(fadeImage, 0f, 1f, fadeDuration));
            }

            yield return null;
        }

        dollyCart.m_Position = endPos;

        while (fadeImage.color.a < 1f)
            yield return null;

        // 4. Load scene async (có delay nhỏ để camera render frame cuối)
        yield return new WaitForSeconds(0.2f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = true;
    }
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    IEnumerator FadeImage(Image img, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            img.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        img.color = new Color(0, 0, 0, to);
        Debug.Log($"Alpha = {img.color.a}");
    }
}
