using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    private static SFX Instance;

    public AudioSource audioSource;

    public SFXClip[] clips;

    private Dictionary<Type, SFXClip> clipDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDictionary()
    {
        clipDictionary = new();
        foreach (var clip in clips)
        {
            clipDictionary.Add(clip.type, clip);
        }
    }

    public static void Play(Type type)
    {
        Instance.PlaySFX(type);
    }

    private void PlaySFX(Type type)
    {
        if (clipDictionary.ContainsKey(type))
        {
            var clip = clipDictionary[type];
            audioSource.PlayOneShot(clip.clip, clip.volume);
        }
    }

    public enum Type
    {
        Erase,
        Place,
        Move,
    }

    [Serializable]
    public struct SFXClip
    {
        public Type type;
        public AudioClip clip;
        public float volume;
    }
}
