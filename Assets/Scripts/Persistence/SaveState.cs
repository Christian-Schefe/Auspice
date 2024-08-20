using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Persistence;
using UnityEngine;
using Yeast;

public class SaveState : MonoBehaviour
{
    private readonly static Dictionary<PersistenceMode, SaveState> instances = new();

    public PersistenceMode mode;

    private Dictionary<string, string> serializedData = new();
    private readonly Dictionary<string, IListener> listeners = new();
    private readonly Dictionary<string, IBox> data = new();
    private bool isLoaded = false;

    private void Awake()
    {
        if (TryGetInstance(mode, out _))
        {
            Destroy(gameObject);
            return;
        }

        instances[mode] = this;
        if (mode == PersistenceMode.GlobalRuntime || mode == PersistenceMode.GlobalPersistence)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (mode == PersistenceMode.ScenePersistence)
        {
            SceneSystem.AddOnBeforeSceneUnload(() =>
            {
                Save();
            });
        }

        Load();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public string GetPath()
    {
        return mode switch
        {
            PersistenceMode.GlobalPersistence => "SaveState_Global.json",
            PersistenceMode.ScenePersistence => $"SaveState_{gameObject.scene.buildIndex}.json",
            _ => throw new Exception("Invalid mode")
        };
    }

    public static bool TryGetInstance(PersistenceMode mode, out SaveState instance)
    {
        return instances.TryGetValue(mode, out instance) && instance != null;
    }

    public void Save()
    {
        if (mode == PersistenceMode.GlobalRuntime) return;
        if (!isLoaded) throw new Exception("SaveState has not loaded yet.");

        foreach (var (key, box) in data)
        {
            serializedData[key] = box.GetValue().ToJson();
        }
        if (serializedData.Count == 0) return;

        var path = GetPath();
        JsonPersistence.Save(path, serializedData);
    }

    public void Load()
    {
        if (mode == PersistenceMode.GlobalRuntime) return;
        if (isLoaded) return;
        Debug.Log("Loading SaveState " + mode);

        var path = GetPath();
        serializedData = JsonPersistence.LoadDefault(path, new Dictionary<string, string>());

        isLoaded = true;
    }

    public void Delete()
    {
        if (mode == PersistenceMode.GlobalRuntime) return;

        var path = GetPath();
        JsonPersistence.Delete(path);

        serializedData.Clear();
        foreach (var key in data.Keys.ToList())
        {
            data.Remove(key);
            OnUpdate(key);
        }
    }

    public bool TryGetBox<T>(string key, out Box<T> box)
    {
        if (!TryLoadKey<T>(key))
        {
            box = null;
            return false;
        }

        if (data.TryGetValue(key, out var anyBox) && anyBox is Box<T> typedBox)
        {
            box = typedBox;
            return true;
        }
        box = null;
        return false;
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (TryGetBox<T>(key, out var box))
        {
            value = box.value;
            return true;
        }
        value = default;
        return false;
    }

    public void Set<T>(string key, T value)
    {
        if (data.TryGetValue(key, out var box))
        {
            if (box is Box<T> typedBox)
            {
                typedBox.value = value;
            }
            else
            {
                data[key] = new Box<T>(value);
            }
        }
        else
        {
            data.Add(key, new Box<T>(value));
        }
        OnUpdate(key);
    }

    public void Unset(string key)
    {
        if (data.Remove(key))
        {
            OnUpdate(key);
        }
    }

    public void OnUpdate(string key)
    {
        if (listeners.TryGetValue(key, out var listener))
        {
            if (data.TryGetValue(key, out var box))
            {
                listener.OnValueChanged(true, box.GetValue());
            }
            else
            {
                listener.OnValueChanged(false, null);
            }
        }
    }

    public bool TryLoadKey<T>(string key)
    {
        if (data.TryGetValue(key, out var anyBox))
        {
            return anyBox is Box<T>;
        }
        else if (serializedData.ContainsKey(key))
        {
            Debug.Log("loading: " + key + " = " + serializedData[key]);
            if (serializedData[key].TryFromJson(out T value))
            {
                data[key] = new Box<T>(value);
                return true;
            }
        }
        return false;
    }

    public void AddListener<T>(string key, Action<bool, T> action)
    {
        TryLoadKey<T>(key);
        if (!listeners.TryGetValue(key, out var listener))
        {
            listener = new Listener<T>();
            listeners.Add(key, listener);
        }
        (listener as Listener<T>).AddListener(action);
    }

    public void RemoveListener<T>(string key, Action<bool, T> action)
    {
        TryLoadKey<T>(key);
        if (listeners.TryGetValue(key, out var listener))
        {
            (listener as Listener<T>).RemoveListener(action);
        }
    }

    private interface IBox
    {
        public object GetValue();
    }

    public class Box<T> : IBox
    {
        public T value;

        public Box(T value)
        {
            this.value = value;
        }

        public object GetValue()
        {
            return value;
        }
    }

    private interface IListener
    {
        public void OnValueChanged(bool isPresent, object val);
    }

    public class Listener<T> : IListener
    {
        public Action<bool, T> onValueChanged;

        public void OnValueChanged(bool isPresent, object val)
        {
            onValueChanged?.Invoke(isPresent, isPresent ? (T)val : default);
        }

        public void AddListener(Action<bool, T> action)
        {
            onValueChanged += action;
        }

        public void RemoveListener(Action<bool, T> action)
        {
            onValueChanged -= action;
        }
    }
}

public enum PersistenceMode { GlobalPersistence, ScenePersistence, GlobalRuntime }
