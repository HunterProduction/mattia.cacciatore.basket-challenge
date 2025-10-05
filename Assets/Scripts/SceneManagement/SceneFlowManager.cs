using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviourSingleton<SceneFlowManager>
{
    [SerializeField] private string firstSceneToLoad;
    [SerializeField] private Canvas loadingScreen;

    private bool _isLoading;
    public bool IsLoading => _isLoading;

    private Scene _currentSceneLoaded;
    public Scene CurrentSceneLoaded => _currentSceneLoaded;

    private void Start()
    {
        loadingScreen.gameObject.SetActive(false);
        LoadSceneByNameAsync(firstSceneToLoad);
    }

    public void LoadSceneByNameAsync(string sceneName)
    {
        if (_isLoading)
        {
            Debug.LogWarning($"[{GetType().Name}] Busy loading.");
            return;
        }

        if (_currentSceneLoaded.name == sceneName)
        {
            Debug.LogWarning($"[{GetType().Name}] Scene {sceneName} is already loaded.");
            return;
        }

        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string newSceneName)
    {
        _isLoading = true;
        loadingScreen.gameObject.SetActive(true);

        AsyncOperation asyncOperation;
        if (_currentSceneLoaded.IsValid())
        {
            // First, unload current Scene if present.
            Debug.Log($"[{GetType().Name}] Unloading {_currentSceneLoaded.name} scene.");
            asyncOperation = SceneManager.UnloadSceneAsync(_currentSceneLoaded);
            yield return new WaitUntil(() => asyncOperation.isDone);
        }

        // Then load the requested Scene.
        Debug.Log($"[{GetType().Name}] Loading {newSceneName} scene.");
        asyncOperation = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncOperation.isDone);
        _currentSceneLoaded = SceneManager.GetSceneByName(newSceneName);

        loadingScreen.gameObject.SetActive(false);
        _isLoading = false;
    }
}
