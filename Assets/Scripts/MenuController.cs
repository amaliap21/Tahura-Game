using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MenuController : MonoBehaviour
{
    [Header("=== PANELS ===")]
    public GameObject mainMenuPanel;
    public GameObject startPanel;
    public GameObject optionsPanel;

    [Header("=== SLIDERS ===")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("=== AUDIO (Opsional) ===")]
    public AudioMixer audioMixer;

    [Header("=== SCENE NAMES ===")]
    public string gameSceneName = "GameScene";
    public string creditsSceneName = "";

    const string SaveSlotKey = "HasSaveSlot";

    void Start()
    {
        ShowMainMenu();

        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        if (bgmSlider != null) bgmSlider.value = savedBGM;
        if (sfxSlider != null) sfxSlider.value = savedSFX;

        ApplyBGMVolume(savedBGM);
        ApplySFXVolume(savedSFX);

        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    // === BUTTON CALLBACKS — MAIN MENU ===

    public void OnStartClick()
    {
        if (startPanel != null)
            startPanel.SetActive(true);
        else if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
    }

    public void OnOptionsClick()
    {
        mainMenuPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnCreditsClick()
    {
        if (!string.IsNullOrEmpty(creditsSceneName))
            SceneManager.LoadScene(creditsSceneName);
        else
            Debug.Log("Credits scene belum di-set.");
    }

    public void OnExitClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // === BUTTON CALLBACKS — START SUBMENU ===

    public void OnNewGameClick()
    {
        PlayerPrefs.SetInt(SaveSlotKey, 1);
        PlayerPrefs.Save();
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            Debug.LogWarning("Game scene name belum di-set di MenuController.");
    }

    public void OnContinueClick()
    {
        if (PlayerPrefs.GetInt(SaveSlotKey, 0) == 0)
        {
            Debug.Log("Belum ada save data — silakan New game.");
            return;
        }
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
    }

    // === BUTTON CALLBACKS — BACK / OPTIONS ===

    public void OnBackClick()
    {
        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
            PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
            PlayerPrefs.Save();
        }
        ShowMainMenu();
    }

    public void OnHowToPlayClick()
    {
        Debug.Log("How to Play — tambahkan logic di sini");
    }

    // === SLIDER ===

    void OnBGMChanged(float value)
    {
        ApplyBGMVolume(value);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    void OnSFXChanged(float value)
    {
        ApplySFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    void ApplyBGMVolume(float value)
    {
        if (audioMixer != null)
        {
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat("BGMVolume", dB);
        }
        else
        {
            AudioListener.volume = value;
        }
    }

    void ApplySFXVolume(float value)
    {
        if (audioMixer != null)
        {
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat("SFXVolume", dB);
        }
    }

    void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (startPanel != null) startPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }
}
