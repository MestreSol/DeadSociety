using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
  public enum ItemCategory
  {
    Food,
    Drink,
    Tools,
    Misc // Para itens diversos
  }

  [Header("Inventory Settings")]
  public int maxSlots = 20;
  private List<Item> inventory = new List<Item>();

  public delegate void OnInventoryChanged(List<Item> currentInventory);
  public event OnInventoryChanged InventoryChanged;
  public void AddItem(Item item)
  {
    if (inventory.Count < maxSlots)
    {
      inventory.Add(item);
      Debug.Log($"Item coletado: {item.itemName} (Categoria: {item.category})");
      InventoryChanged?.Invoke(inventory);
    }
    else
    {
      Debug.Log("O inventário está cheio!");
    }
  }
  public void DropItem(Item item)
  {
    if (inventory.Contains(item))
    {
      inventory.Remove(item);
      Debug.Log($"Item descartado: {item.itemName}");
      InventoryChanged?.Invoke(inventory);
    }
    else
    {
      Debug.Log($"O item {item.itemName} não está no inventário!");
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

  public void Remove(Item item)
  {
    if (inventory.Contains(item))
    {
      inventory.Remove(item);
      Debug.Log($"Item removido: {item.itemName}");
      InventoryChanged?.Invoke(inventory);
    }
    else
    {
      Debug.Log($"O item {item.itemName} não está no inventário!");
    }
  }

  public List<Item> GetInventory() => inventory;
}
