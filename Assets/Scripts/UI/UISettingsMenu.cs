using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsMenu : MonoBehaviour
{
    private PersistentValue<SettingsData> settingsData = new("SettingsData", PersistenceMode.GlobalPersistence, new() { masterVolume = 1, musicVolume = 1, sfxVolume = 1 });

    public UIModalWindow window;

    public UISettingsSlider masterVolumeSlider;
    public UISettingsSlider musicVolumeSlider;
    public UISettingsSlider sfxVolumeSlider;

    private void Awake()
    {
        masterVolumeSlider.AddListener(OnMasterVolumeSliderValueChanged);
        musicVolumeSlider.AddListener(OnMusicVolumeSliderValueChanged);
        sfxVolumeSlider.AddListener(OnSFXVolumeSliderValueChanged);

        masterVolumeSlider.formatter = (value) => (value * 100).ToString("0") + "%";
        musicVolumeSlider.formatter = (value) => (value * 100).ToString("0") + "%";
        sfxVolumeSlider.formatter = (value) => (value * 100).ToString("0") + "%";
    }

    private void Start()
    {
        ref var settings = ref settingsData.GetRef();

        masterVolumeSlider.SetValue(settings.masterVolume);
        musicVolumeSlider.SetValue(settings.musicVolume);
        sfxVolumeSlider.SetValue(settings.sfxVolume);

        settingsData.MarkDirty();
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
}

public struct SettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
}
