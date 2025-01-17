using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Inventory/Consumable Item")]
public class ConsumableItem : Item
{
    [Header("Effects")]
    public int healthRestoration;
    public int staminaRestoration;
    public int sanityRestoration;
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

    public void Consume(PlayerStatus playerStats)
    {
        if (playerStats == null) return;

        playerStats.Heal(healthRestoration);
        playerStats.RestoreStamina(staminaRestoration);
        playerStats.RestoreSanity(sanityRestoration);
        playerStats.RestoreHunger(hungerRestoration);
        playerStats.RestoreThirst(thirstRestoration);
        playerStats.RestoreSleep(sleepRestoration);
    }
}
