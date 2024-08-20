using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast;

public class SFX : MonoBehaviour
{
    private PersistentValue<SettingsData> settingsData;

    private static SFX Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public SFXClip[] clips;
    public AudioClip music;

    private Dictionary<Type, SFXClip> clipDictionary;

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        settingsData = new("SettingsData", PersistenceMode.GlobalPersistence);
        settingsData.AddListener(OnSettingsDataChanged);
        settingsData.MarkDirty();

        clipDictionary = new();
        foreach (var clip in clips)
        {
            clipDictionary.Add(clip.type, clip);
        }

        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void OnSettingsDataChanged(bool isPresent, SettingsData settings)
    {
        if (!isPresent) settings = new() { masterVolume = 1, musicVolume = 1, sfxVolume = 1 };
        musicSource.volume = settings.musicVolume * settings.masterVolume;
        sfxSource.volume = settings.sfxVolume * settings.masterVolume;
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
            sfxSource.PlayOneShot(clip.clip, clip.volume);
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
