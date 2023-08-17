using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject aboutPanel;
    public GameObject settingsPanel;
    public Slider sensitivitySlider; // Reference to the slider UI element for sensitivity

    private float mouseSensitivity = 2f; // Default mouse sensitivity value

    private void Start()
    {
        // Load the saved sensitivity value from PlayerPrefs
        if (PlayerPrefs.HasKey("MouseSensitivity"))
        {
            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
        }
        sensitivitySlider.value = mouseSensitivity; // Update the slider value to reflect the current sensitivity
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void AboutPanel()
    {
        aboutPanel.SetActive(true);
    }

    public void ReturnToMain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        aboutPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void SettingsPanel()
    {
        settingsPanel.SetActive(true);
    }

    public void ChangeSensitivity(float value)
    {
        mouseSensitivity = value;
    }

    public void SaveSettings()
    {
        // Save the current mouse sensitivity value to PlayerPrefs
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
    }
}

