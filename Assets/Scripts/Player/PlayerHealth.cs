using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("Attributes")]
    public int maxHealth = 100;
    public int maxEnergy = 100;
    public int maxSanity = 100;
    public int maxStamina = 100;
    public int maxHunger = 100;
    public int maxThirst = 100;
    public int maxSleep = 100;

    private int currentHealth;
    private int currentEnergy;
    private int currentSanity;
    private int currentStamina;
    private int currentHunger;
    private int currentThirst;
    private int currentSleep;

    public delegate void OnAttributeChanged(string attribute, int currentValue, int maxValue);
    public static event OnAttributeChanged AttributeChanged;

    private void Awake()
    {
        ResetAttributes();
        Instance = this;
    }

    private void ResetAttributes()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentSanity = maxSanity;
        currentStamina = maxStamina;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentSleep = maxSleep;

        InvokeAttributeChanged("Health", currentHealth, maxHealth);
    }

    public void ModifyAttribute(string attribute, int value)
    {
        switch (attribute)
        {
            case "Health": UpdateValue(ref currentHealth, maxHealth, value); break;
            case "Energy": UpdateValue(ref currentEnergy, maxEnergy, value); break;
            case "Sanity": UpdateValue(ref currentSanity, maxSanity, value); break;
            case "Stamina": UpdateValue(ref currentStamina, maxStamina, value); break;
            case "Hunger": UpdateValue(ref currentHunger, maxHunger, value); break;
            case "Thirst": UpdateValue(ref currentThirst, maxThirst, value); break;
            case "Sleep": UpdateValue(ref currentSleep, maxSleep, value); break;
        }
    }

    private void UpdateValue(ref int currentValue, int maxValue, int change)
    {
        currentValue = Mathf.Clamp(currentValue + change, 0, maxValue);
        InvokeAttributeChanged("Health", currentValue, maxValue);

        if (currentValue <= 0 && maxValue == maxHealth)
        {
            Die();
        }
    }

    private void InvokeAttributeChanged(string attribute, int currentValue, int maxValue)
    {
        AttributeChanged?.Invoke(attribute, currentValue, maxValue);
    }

    private void Die()
    {
        Debug.Log("O jogador morreu!");
        GameManager.Instance.GameOver();
    }
}
