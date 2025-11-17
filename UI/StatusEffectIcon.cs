using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectIcon : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public Image durationFill;
    public TextMeshProUGUI stackCountText;
    
    private StatusEffect effect;
    
    public void Initialize(StatusEffect effect)
    {
        this.effect = effect;
        
        // Load icon based on effect name
        string iconPath = $"StatusIcons/{effect.name}";
        Sprite icon = Resources.Load<Sprite>(iconPath);
        if (icon != null)
            iconImage.sprite = icon;
            
        // Hide duration fill for permanent effects
        if (effect.isPermanent)
            durationFill.gameObject.SetActive(false);
            
        // Hide stack count by default
        stackCountText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (!effect.isPermanent && durationFill != null)
        {
            durationFill.fillAmount = effect.remainingTime / effect.duration;
        }
    }
}