using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] GameObject loadingMenu;
    [SerializeField] Image loadingBar;
    [SerializeField] TextMeshProUGUI loadingText;
    float elapsedTime, delayTimer = 2f;
    bool isLoadingStarted;

    public void LoadScene(int sceneID)
    {
        loadingMenu.SetActive(true);
        isLoadingStarted = true;
        StartCoroutine(LoadSceneAsync(sceneID));  // Start the asynchronous loading process
    }

    void Update()
    {
        if (isLoadingStarted)  // Check if loading has started
        {
            elapsedTime = Mathf.Min(delayTimer, elapsedTime + Time.deltaTime);  // Increase elapsedTime until it reaches delayTimer
            loadingText.text = "Loading " + Mathf.RoundToInt((elapsedTime / delayTimer) * 100).ToString() + "%";  // Update loading text to show progress percentage
            loadingBar.fillAmount = elapsedTime / delayTimer;  // Update loading bar fill amount based on progress
        }
    }

    IEnumerator LoadSceneAsync(int sceneID)
    {
        yield return new WaitForSeconds(delayTimer);  // Artificial delay timer

        // Start loading scene asynchronously and whait until it's done
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);
        while (!operation.isDone) yield return null;
    }
}