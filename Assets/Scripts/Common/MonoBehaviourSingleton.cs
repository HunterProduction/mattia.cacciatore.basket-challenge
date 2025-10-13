using UnityEngine;

[DefaultExecutionOrder(-50)]
public class MonoBehaviourSingleton<T> : MonoBehaviour where T : Component
{
    [SerializeField] private bool dontDestroyOnLoad = true;
    public static bool IsAvailable => _instance != null;

    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    public virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;

        }
        else
        {
            _instance = this as T;
            if(dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);
        }
    }
}
