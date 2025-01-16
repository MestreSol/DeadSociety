using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Inventory/Consumable Item")]
  public class ConsumableItem : Item
  {

      [Header("Basic Info")]
      public string itemName;
      public string description;

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
        Defense,
        Stamina,
        Hunger,
        Thirst,
        Sleep
      }
      public BoostType boostType; // Tipo de boost (ex.: velocidade ou força)

      // Método que define o que acontece ao consumir o item
      public virtual void Consume(PlayerHealth playerStats)
      {
        playerStats.Heal(healthRestoration);
        playerStats.RestoreEnergy(energyRestoration);
        playerStats.RestoreStamina(staminaRestoration);
        playerStats.RestoreHunger(hungerRestoration);
        playerStats.RestoreThirst(thirstRestoration);
        playerStats.RestoreSleep(sleepRestoration);
      }
  }
