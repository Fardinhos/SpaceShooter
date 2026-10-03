/*
 * Maurice Mattick - 3103312
 *
 * Marian Müller - 3103387
 *
 * Rezaul Hoque - 3077415
 *
 * Fardin Afzalzada - 3082980
 */

using UnityEngine;

public class SoundSettingsMenu : MonoBehaviour
{
    [Header("Global Sound Control")]
    [Tooltip("If checked, sound is ON. If unchecked, all sound is muted.")]
    public bool soundOn = true;

    private const string SoundMutedKey = "SoundMuted";

    void Awake()
    {
        bool muted = PlayerPrefs.GetInt(SoundMutedKey, 0) == 1;
        soundOn = !muted;

        ApplySoundSetting();
    }

    void OnValidate()
    {
        ApplySoundSetting();
    }

    public void SetSoundOn()
    {
        soundOn = true;
        ApplySoundSetting();
    }

    public void SetSoundOff()
    {
        soundOn = false;
        ApplySoundSetting();
    }

    private void ApplySoundSetting()
    {
        bool muted = !soundOn;

        AudioListener.volume = muted ? 0f : 1f;

        PlayerPrefs.SetInt(SoundMutedKey, muted ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("[SoundSettingsMenu] SoundOn = " + soundOn + " (muted = " + muted + ")");
    }
}