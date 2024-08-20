using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsMenu : MonoBehaviour
{
    private readonly PersistentValue<SettingsData> settingsData = new("SettingsData", PersistenceMode.GlobalPersistence);

    public UIModalWindow window;

    public UISettingsSlider masterVolumeSlider;
    public UISettingsSlider musicVolumeSlider;
    public UISettingsSlider sfxVolumeSlider;

    public Button deleteSaveButton;

    private void Awake()
    {
        masterVolumeSlider.AddListener(OnMasterVolumeSliderValueChanged);
        musicVolumeSlider.AddListener(OnMusicVolumeSliderValueChanged);
        sfxVolumeSlider.AddListener(OnSFXVolumeSliderValueChanged);

        deleteSaveButton.onClick.AddListener(DeleteSaveData);

        masterVolumeSlider.formatter = (value) => (value * 100).ToString("0") + "%";
        musicVolumeSlider.formatter = (value) => (value * 100).ToString("0") + "%";
        sfxVolumeSlider.formatter = (value) => (value * 100).ToString("0") + "%";
    }

    private void Start()
    {
        if (settingsData.TryGet(out var settings))
        {
            masterVolumeSlider.SetValue(settings.masterVolume);
            musicVolumeSlider.SetValue(settings.musicVolume);
            sfxVolumeSlider.SetValue(settings.sfxVolume);
        }
        else
        {
            settingsData.Set(new SettingsData
            {
                masterVolume = 1,
                musicVolume = 1,
                sfxVolume = 1
            });
        }
    }

    private void OnMasterVolumeSliderValueChanged(float value)
    {
        ref var settings = ref settingsData.GetRef();
        settings.masterVolume = value;
        settingsData.MarkDirty();
    }

    private void OnMusicVolumeSliderValueChanged(float value)
    {
        ref var settings = ref settingsData.GetRef();
        settings.musicVolume = value;
        settingsData.MarkDirty();
    }

    private void OnSFXVolumeSliderValueChanged(float value)
    {
        ref var settings = ref settingsData.GetRef();
        settings.sfxVolume = value;
        settingsData.MarkDirty();
    }

    public void DeleteSaveData()
    {
        var oldSettingsData = settingsData.Get();
        if (SaveState.TryGetInstance(PersistenceMode.GlobalPersistence, out var globalSaveState))
        {
            globalSaveState.Delete();
        }
        if (SaveState.TryGetInstance(PersistenceMode.ScenePersistence, out var sceneSaveState))
        {
            sceneSaveState.Delete();
        }
        settingsData.Set(oldSettingsData);
    }
}

public struct SettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
}
