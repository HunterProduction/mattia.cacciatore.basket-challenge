using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    /**
     * #NOTE: For the sake of immediacy and time optimization, scene referencing has been
     * left string-based. It is possible to adopt a more refined asset-driven approach less
     * prone to error. 
     * #MattiaCacciatore
     */
    [SerializeField] private string targetScene;

    public void Load()
    {
        SceneFlowManager.Instance.LoadSceneByNameAsync(targetScene);
    }
}
