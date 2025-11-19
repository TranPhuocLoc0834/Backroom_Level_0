using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingBar : MonoBehaviour
{
    public Image loadingBar;
    public string sceneToLoad;
    public float minLoadingTime = 7f;   // ít nhất 5 giây
    public int minChunks = 7;
    public int maxChunks = 12;
    public float minStep = 0.02f;
    public float maxStep = 0.07f;

    private void Start()
    {
        if (loadingBar != null)
            loadingBar.fillAmount = 0f;

        StartCoroutine(FillRandomChunks());
    }

    private IEnumerator FillRandomChunks()
    {
        float progress = 0f;
        float elapsedTime = 0f;

        int totalChunks = Random.Range(minChunks, maxChunks + 1);

        // Chia minLoadingTime thành các khúc dựa trên totalChunks
        float[] chunkDelays = new float[totalChunks];
        float sumDelays = 0f;

        for (int i = 0; i < totalChunks; i++)
        {
            // tạo delay ngẫu nhiên cho mỗi khúc
            chunkDelays[i] = Random.Range(0.1f, 0.3f);
            sumDelays += chunkDelays[i];
        }

        // chuẩn hóa delay để tổng = minLoadingTime
        for (int i = 0; i < totalChunks; i++)
        {
            chunkDelays[i] = chunkDelays[i] / sumDelays * minLoadingTime;
        }

        for (int i = 0; i < totalChunks; i++)
        {
            // fill ngẫu nhiên từng khúc
            float step = Random.Range(minStep, maxStep);
            progress = Mathf.Min(progress + step, 1f);

            if (loadingBar != null)
                loadingBar.fillAmount = progress;

            // delay khúc hiện tại
            yield return new WaitForSeconds(chunkDelays[i]);
            elapsedTime += chunkDelays[i];
        }

        // chắc chắn fill = 100%
        if (loadingBar != null)
            loadingBar.fillAmount = 1f;

        // load scene
        SceneManager.LoadScene(sceneToLoad);
    }
}
