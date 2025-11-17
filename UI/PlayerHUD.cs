using UnityEngine;
using TMPro;
using System;

public class PlayerHUD : MonoBehaviour
{
    public TMP_Text healthText;      // Drag your bottom-left TMP text here
    public PlayerInfo playerInfo;    // Drag your PlayerInfo here



    void Start()
    {
        // Initialize display
        UpdateHealthDisplay(playerInfo.currentHealth);

        // Subscribe to health changes
        playerInfo.OnHealthChanged += () => UpdateHealthDisplay(playerInfo.currentHealth);
    }

    void UpdateHealthDisplay(float newHealth)
    {
        if (healthText != null)
            healthText.text = Mathf.RoundToInt(newHealth).ToString();
    }
}


