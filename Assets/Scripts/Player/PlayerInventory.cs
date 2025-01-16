using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
  public enum ItemCategory { Food, Drink, Tools, Weapons, Armor, QuestItems, Misc }

  [Header("Inventory Settings")]
  public int maxSlots = 20;

  private List<Item> inventory = new List<Item>();

  public delegate void OnInventoryChanged(List<Item> currentInventory);
  public static event OnInventoryChanged InventoryChanged;

  public void AddItem(Item item)
  {
    if (inventory.Count < maxSlots)
    {
      inventory.Add(item);
      Debug.Log($"Item coletado: {item.itemName} (Categoria: {item.itemCategory})");
      InventoryChanged?.Invoke(inventory);
    }
    else
    {
      Debug.Log("O inventário está cheio!");
    }
  }

  public void RemoveItem(string itemName)
  {
    Item itemToRemove = inventory.Find(item => item.itemName == itemName);
    if (itemToRemove != null)
    {
      inventory.Remove(itemToRemove);
      Debug.Log($"Item removido: {itemName}");
      InventoryChanged?.Invoke(inventory);
    }
    else
    {
      Debug.Log($"O item {itemName} não está no inventário!");
    }
  }

  public List<Item> GetInventory() => inventory;
}
