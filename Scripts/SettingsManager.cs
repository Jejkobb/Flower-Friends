using System;
using UnityEngine;

public static class SettingsManager
{
    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat("MusicVolume", 1f);
        set
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            OnSettingsChanged?.Invoke();
        }
    }

    public static float SFXVolume
    {
        get => PlayerPrefs.GetFloat("SFXVolume", 1f);
        set
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            OnSettingsChanged?.Invoke();
        }
    }

    public static event Action OnSettingsChanged;
}
