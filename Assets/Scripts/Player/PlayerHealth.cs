using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
  [Header("Health Settings")]
  public int maxHealth = 100;
  private int currentHealth;

  public delegate void OnHealthChanged(int currentHealth, int maxHealth);
  public static event OnHealthChanged HealthChanged;

  private void Start()
  {
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

  public int GetCurrentHealth() => currentHealth;
  public int GetMaxHealth() => maxHealth;
}
