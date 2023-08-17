using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int startingHealth = 100;
    public Slider healthSlider;
    public Text healthText;

    private int currentHealth;
    
    private LevelManager lm;
    private Animator anim;
    
    void Start()
    {
        currentHealth = startingHealth;
        healthSlider.value = currentHealth;
        
        lm = FindObjectOfType<LevelManager>();
        anim = GetComponent<Animator>();
    }


    public void TakeDamage(int damageAmount)
    {
        if (LevelManager.isGameOver) return; 

        if (currentHealth > 0)
        {
            currentHealth -= damageAmount;
            healthSlider.value = Mathf.Clamp(currentHealth, 0, 100);
            SetHealthText();
        }
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            SetHealthText();
            PlayerDies();
        }
    }
    
    public void TakeHealth(int healthAmount)
    {
        if (currentHealth < 100)
        {
            currentHealth += healthAmount;
            healthSlider.value = Mathf.Clamp(currentHealth, 0, 100);
            SetHealthText();
        }
    }
    
    private void SetHealthText()
    {
        healthText.text = currentHealth.ToString("");
    }

    void PlayerDies()
    {
        anim.SetBool("isDead", true);
        lm.LevelLost();
    }
}
