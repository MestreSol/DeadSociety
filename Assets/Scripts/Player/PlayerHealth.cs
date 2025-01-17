using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

  public static PlayerHealth Instance { get; private set; }

  [Header("Health Settings")]

  public int maxEnergy = 100;
  private int currentEnergy;

  public int maxHealth = 100;
  private int currentHealth;

  public int maxSanity = 100;
  private int currentSanity;

  public int maxStamina = 100;
  private int currentStamina;

  public int maxHunger = 100;
  private int currentHunger;

  public int maxThirst = 100;
  private int currentThirst;

  public int maxSleep= 100;
  private int currentSleep;

  public delegate void OnHealthChanged(int currentHealth, int maxHealth);
  public static event OnHealthChanged HealthChanged;

  public delegate void OnEnergyChanged(int currentEnergy, int maxEnergy);
  public static event OnEnergyChanged EnergyChanged;

  public delegate void OnSanityChanged(int currentSanity, int maxSanity);
  public static event OnSanityChanged SanityChanged;

  public delegate void OnStaminaChanged(int currentStamina, int maxStamina);
  public static event OnStaminaChanged StaminaChanged;

  public delegate void OnHungerChanged(int currentHunger, int maxHunger);
  public static event OnHungerChanged HungerChanged;

  public delegate void OnThirstChanged(int currentThirst, int maxThirst);
  public static event OnThirstChanged ThirstChanged;

  public delegate void OnSleepChanged(int currentSleep, int maxSleep);
  public static event OnSleepChanged SleepChanged;

  private void Start()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(gameObject);
    }
    currentHealth = maxHealth;
    HealthChanged?.Invoke(currentHealth, maxHealth); // Atualiza HUD inicial
  }

  public void TakeDamage(int damage)
  {
    currentHealth -= damage;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    Debug.Log($"Jogador sofreu dano: {damage}. Saúde atual: {currentHealth}");

    HealthChanged?.Invoke(currentHealth, maxHealth);

    if (currentHealth <= 0)
    {
      Die();
    }
  }

  public void Heal(int healAmount)
  {
    currentHealth += healAmount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    Debug.Log($"Jogador curado: {healAmount}. Saúde atual: {currentHealth}");

    HealthChanged?.Invoke(currentHealth, maxHealth);
  }

  private void Die()
  {
    Debug.Log("O jogador morreu!");
    GameManager.Instance.GameOver();
    // Aqui você pode adicionar animações de morte ou lógica adicional.
  }

  public void RestoreEnergy(int energyAmount)
  {
    currentEnergy += energyAmount;
    currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

    Debug.Log($"Energia restaurada: {energyAmount}. Energia atual: {currentEnergy}");

    EnergyChanged?.Invoke(currentEnergy, maxEnergy);
  }

  public void RestoreSanity(int sanityAmount)
  {
    currentSanity += sanityAmount;
    currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);

    Debug.Log($"Sanidade restaurada: {sanityAmount}. Sanidade atual: {currentSanity}");

    SanityChanged?.Invoke(currentSanity, maxSanity);
  }

  public void RestoreStamina(int staminaAmount)
  {
    currentStamina += staminaAmount;
    currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

    Debug.Log($"Estamina restaurada: {staminaAmount}. Estamina atual: {currentStamina}");

    StaminaChanged?.Invoke(currentStamina, maxStamina);
  }

  public void RestoreHunger(int hungerAmount)
  {
    currentHunger += hungerAmount;
    currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

    Debug.Log($"Fome restaurada: {hungerAmount}. Fome atual: {currentHunger}");

    HungerChanged?.Invoke(currentHunger, maxHunger);
  }

  public void RestoreThirst(int thirstAmount)
  {
    currentThirst += thirstAmount;
    currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);

    Debug.Log($"Sede restaurada: {thirstAmount}. Sede atual: {currentThirst}");

    ThirstChanged?.Invoke(currentThirst, maxThirst);
  }

  public void RestoreSleep(int sleepAmount)
  {
    currentSleep += sleepAmount;
    currentSleep = Mathf.Clamp(currentSleep, 0, maxSleep);

    Debug.Log($"Sono restaurado: {sleepAmount}. Sono atual: {currentSleep}");

    SleepChanged?.Invoke(currentSleep, maxSleep);
  }

  public int GetCurrentEnergy() => currentEnergy;
  public int GetMaxEnergy() => maxEnergy;
  public int GetCurrentHealth() => currentHealth;
  public int GetMaxHealth() => maxHealth;
  public int GetCurrentSanity() => currentSanity;
  public int GetMaxSanity() => maxSanity;
  public int GetCurrentStamina() => currentStamina;
  public int GetMaxStamina() => maxStamina;
  public int GetCurrentHunger() => currentHunger;
  public int GetMaxHunger() => maxHunger;
  public int GetCurrentThirst() => currentThirst;
  public int GetMaxThirst() => maxThirst;
  public int GetCurrentSleep() => currentSleep;
  public int GetMaxSleep() => maxSleep;
}
