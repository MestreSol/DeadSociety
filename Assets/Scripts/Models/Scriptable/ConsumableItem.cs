using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Inventory/Consumable Item")]
public class ConsumableItem : Item
{
  [Header("Effects")]
  public int healthRestoration;
  public int energyRestoration;
  public int staminaRestoration;
  public int hungerRestoration;
  public int thirstRestoration;
  public int sleepRestoration;

  public enum BoostType
  {
    None,
    Speed,
    Strength,
    Defense
  }
  public BoostType boostType;

  public void Consume(PlayerHealth playerStats)
  {
    if (playerStats == null) return;

    playerStats.ModifyAttribute("Health", healthRestoration);
    playerStats.ModifyAttribute("Energy", energyRestoration);
    playerStats.ModifyAttribute("Stamina", staminaRestoration);
    playerStats.ModifyAttribute("Hunger", hungerRestoration);
    playerStats.ModifyAttribute("Thirst", thirstRestoration);
    playerStats.ModifyAttribute("Sleep", sleepRestoration);
  }
}
