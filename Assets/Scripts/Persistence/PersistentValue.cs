using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentValue<T>
{
    private readonly string key;
    private readonly PersistenceMode mode;

    private SaveState saveState;

    public PersistentValue(string key, PersistenceMode mode)
    {
        this.key = key;
        this.mode = mode;
    }

    private void FindSaveState()
    {
        if (!SaveState.TryGetInstance(mode, out saveState))
        {
            throw new System.Exception($"SaveState with mode {mode} not found");
        }
    }

    public void AddListener(System.Action<bool, T> action)
    {
        if (saveState == null) FindSaveState();
        saveState.AddListener(key, action);
    }

    public void RemoveListener(System.Action<bool, T> action)
    {
        if (saveState == null) FindSaveState();
        saveState.RemoveListener(key, action);
    }

    public bool TryGet(out T value)
    {
        if (saveState == null) FindSaveState();
        return saveState.TryGet(key, out value);
    }

    public T Get()
    {
        if (saveState == null) FindSaveState();
        if (!saveState.TryGet(key, out T value))
        {
            throw new System.Exception($"PersistentValue.Get: {key} not found");
        }
        return value;
    }

    public T GetOrDefault(T defaultValue)
    {
        if (saveState == null) FindSaveState();
        if (!saveState.TryGet(key, out T value))
        {
            return defaultValue;
        }
        return value;
    }

    public ref T GetRef()
    {
        if (saveState == null) FindSaveState();
        if (!saveState.TryGetBox(key, out SaveState.Box<T> box))
        {
            throw new System.Exception($"PersistentValue.Get: {key} not found");
        }
        return ref box.value;
    }

    public ref T GetOrCreateRef(T defaultValue)
    {
        if (saveState == null) FindSaveState();
        if (!saveState.TryGetBox(key, out SaveState.Box<T> box))
        {
            saveState.Set(key, defaultValue);
            saveState.TryGetBox(key, out box);
        }
        return ref box.value;
    }

    public void Set(T value)
    {
        if (saveState == null) FindSaveState();
        saveState.Set(key, value);
    }

    public void Unset()
    {
        if (saveState == null) FindSaveState();
        saveState.Unset(key);
    }

    public void MarkDirty()
    {
        if (saveState == null) FindSaveState();
        saveState.OnUpdate(key);
    }
}
