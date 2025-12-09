using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Text healthText;
    [SerializeField] private PlayerHealth playerHealth;

    private void Update()
    {
        if (playerHealth != null)
            healthText.text = "Health: " + playerHealth.GetHealth() + " / " + playerHealth.GetMaxHealth();
    }
}

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Transform restartPosition;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player took " + damage + " damage! Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Debug.LogWarning("Player lost a life! Health remaining: " + currentHealth);
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("Player healed! Current Health: " + currentHealth);
    }

    private void Die()
    {
        Debug.LogError("Player Died! Restarting scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public int GetHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
