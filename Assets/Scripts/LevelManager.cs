using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public float levelDuration = 100f;
    
    public Text timerText;
    public Text gameText;
    public Text enemyCountText;

    public AudioSource bgmSource;
    public AudioClip gameOverSFX;
    public AudioClip gameWonSFX;

    public static bool isGameOver = false;
    public static bool isLightOn = true;

    public string nextLevel;
    private float countDown;

    void Start()
    {
        isGameOver = false;
        isLightOn = true;
        countDown = levelDuration;
        SetTimerText();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameOver)
        {
            if (countDown > 0)
            {
                countDown -= Time.deltaTime;
                bgmSource.pitch = countDown > levelDuration * 0.25f ? 
                        0.9f + (levelDuration - countDown) / (levelDuration * 0.75f) * 0.2f : 
                        1.15f;
            }
            else
            {
                countDown = 0.0f;

                LevelLost();
            }
            SetTimerText();
        }
        SetCountText();
    }

    private void SetTimerText()
    {
        timerText.text = countDown.ToString("f2");
    }

    private void SetCountText()
    {
        enemyCountText.text = "Targets Left: " + AstronautAI.keyEnemyCount.ToString("00");
    }
    
    public void LevelLost()
    {
        isGameOver = true;
        gameText.text = "Game Over!";
        gameText.gameObject.SetActive(true);

        // Camera.main.GetComponent<AudioSource>().pitch = 1;
        AudioSource.PlayClipAtPoint(gameOverSFX, Camera.main.transform.position);

        Invoke("LoadCurrentLevel", 2);
    }
    
    public void LevelBeat()
    {
        isGameOver = true;
        gameText.text = "You Win!";
        gameText.gameObject.SetActive(true);

        // Camera.main.GetComponent<AudioSource>().pitch = 5;
        AudioSource.PlayClipAtPoint(gameWonSFX, Camera.main.transform.position);

        if (!string.IsNullOrEmpty(nextLevel))
        {
            Invoke("LoadNextLevel", 2);
        }

    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevel);
    }

    void LoadCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}