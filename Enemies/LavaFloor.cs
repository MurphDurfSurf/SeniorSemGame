using UnityEngine;
using System.Reflection;

public class LavaFloor : MonoBehaviour
{
	[Tooltip("Amount of health to remove when the player touches this object.")]
	[SerializeField] private int damage = 10;
	[Tooltip("Time between damage ticks in seconds")]
	[SerializeField] private float damageInterval = 1f;

	private float timeSinceLastDamage = 0f;
	private PlayerInfo playerInfo;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter detected: {other.name}, Tag: {other.tag}");

        // Check by tag first
        if (other.CompareTag("Player"))
        {
            playerInfo = other.GetComponent<PlayerInfo>();
            timeSinceLastDamage = 0f;
            Debug.Log($"Player entered lava floor. PlayerInfo found: {playerInfo != null}");
        }
        // Fallback: Check by component if tag doesn't work
        else
        {
            PlayerInfo info = other.GetComponent<PlayerInfo>();
            if (info != null)
            {
                playerInfo = info;
                timeSinceLastDamage = 0f;
                Debug.Log("Player detected via PlayerInfo component (tag check failed)");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (playerInfo != null && (other.CompareTag("Player") || other.GetComponent<PlayerInfo>() != null))
        {
            timeSinceLastDamage += Time.deltaTime;
            if (timeSinceLastDamage >= damageInterval)
            {
                playerInfo.TakeDamage(damage);
                timeSinceLastDamage = 0f;
                Debug.Log($"Damage applied. Current Health: {playerInfo.currentHealth}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check both by tag and by component
        if (other.CompareTag("Player") || other.GetComponent<PlayerInfo>() != null)
        {
            playerInfo = null;
            timeSinceLastDamage = 0f;
            Debug.Log("Player exited lava floor.");
        }
    }
}
