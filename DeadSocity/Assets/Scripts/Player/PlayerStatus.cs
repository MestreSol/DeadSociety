using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Status Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    public float maxStamina = 100f;
    private float currentStamina;
    
    public float maxSanity = 100f;
    private float currentSanity;
    
    public float maxHunger = 100f;
    private float currentHunger;
    
    public float maxThirst = 100f;
    private float currentThirst;
    
    public float maxSleep = 100f;
    private float currentSleep;
    
    public delegate void OnHealthChange(float currentHealth, float maxHealth);
    public event OnHealthChange onHealthChange;
    
    public delegate void OnStaminaChange(float currentStamina, float maxStamina);
    public event OnStaminaChange onStaminaChange;
    
    public delegate void OnSanityChange(float currentSanity, float maxSanity);
    public event OnSanityChange onSanityChange;
    
    public delegate void OnHungerChange(float currentHunger, float maxHunger);
    public event OnHungerChange onHungerChange;
    
    public delegate void OnThirstChange(float currentThirst, float maxThirst);
    public event OnThirstChange onThirstChange;
    
    public delegate void OnSleepChange(float currentSleep, float maxSleep);
    public event OnSleepChange onSleepChange;

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentSanity = maxSanity;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentSleep = maxSleep;
        
        onHealthChange?.Invoke(currentHealth, maxHealth);
        onStaminaChange?.Invoke(currentStamina, maxStamina);
        onSanityChange?.Invoke(currentSanity, maxSanity);
        onHungerChange?.Invoke(currentHunger, maxHunger);
        onThirstChange?.Invoke(currentThirst, maxThirst);
        onSleepChange?.Invoke(currentSleep, maxSleep);
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        Debug.Log($"Jogador Sofreu dano de {damage} pontos de vida. Vida atual: {currentHealth}");
        
        onHealthChange?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        Debug.Log($"Jogador curado: {healAmount} pontos de vida. Vida atual: {currentHealth}");
        
        onHealthChange?.Invoke(currentHealth, maxHealth);
    }

    public void RestoreSanity(float amount)
    {
        currentSanity += amount;
        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);
        
        Debug.Log($"Jogador recuperou {amount} pontos de sanidade. Sanidade atual: {currentSanity}");
        
        onSanityChange?.Invoke(currentSanity, maxSanity);
    }
    
    public void RestoreHunger(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        
        Debug.Log($"Jogador recuperou {amount} pontos de fome. Fome atual: {currentHunger}");
        
        onHungerChange?.Invoke(currentHunger, maxHunger);
    }
    
    public void RestoreThirst(float amount)
    {
        currentThirst += amount;
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);
        
        Debug.Log($"Jogador recuperou {amount} pontos de sede. Sede atual: {currentThirst}");
        
        onThirstChange?.Invoke(currentThirst, maxThirst);
    }
    
    public void RestoreSleep(float amount)
    {
        currentSleep += amount;
        currentSleep = Mathf.Clamp(currentSleep, 0, maxSleep);
        
        Debug.Log($"Jogador recuperou {amount} pontos de sono. Sono atual: {currentSleep}");
        
        onSleepChange?.Invoke(currentSleep, maxSleep);
    }
    
    public void RestoreStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        
        Debug.Log($"Jogador recuperou {amount} pontos de estamina. Estamina atual: {currentStamina}");
        
        onStaminaChange?.Invoke(currentStamina, maxStamina);
    }
    
    private void Die()
    {
        Debug.Log("O Jogador Morreu");
        GameManager.Instance.GameOver();
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;   
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
    public float GetCurrentSanity() => currentSanity;
    public float GetMaxSanity() => maxSanity;
    public float GetCurrentHunger() => currentHunger;
    public float GetMaxHunger() => maxHunger;
    public float GetCurrentThirst() => currentThirst;
    public float GetMaxThirst() => maxThirst;
    public float GetCurrentSleep() => currentSleep;
    public float GetMaxSleep() => maxSleep;

}