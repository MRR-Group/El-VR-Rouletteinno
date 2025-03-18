using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            
            instance = FindFirstObjectByType<T>();
            
            if (instance != null)
            {
                return instance;
            }
            
            var singletonObject = new GameObject(typeof(T).Name);
            
            instance = singletonObject.AddComponent<T>();
            DontDestroyOnLoad(singletonObject);
            
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}