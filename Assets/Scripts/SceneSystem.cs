using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSystem : MonoBehaviour
{
    private static SceneSystem instance;

    private Action onBeforeSceneUnload;

    public static void AddOnBeforeSceneUnload(Action action)
    {
        instance.onBeforeSceneUnload += action;
    }

    public static void RemoveOnBeforeSceneUnload(Action action)
    {
        instance.onBeforeSceneUnload -= action;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void LoadScene(SceneSO scene)
    {
        instance.onBeforeSceneUnload?.Invoke();
        SceneManager.LoadScene(scene.sceneName);
    }
}
