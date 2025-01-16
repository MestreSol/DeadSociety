using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{

  public GameObject itemDropPrefab;
  public enum ItemCategory { Food, Drink, Tools, Weapons, Armor, QuestItems, Misc }

  [Header("Inventory Settings")]
  public int maxSlots = 20;

  private List<Item> inventory = new List<Item>();

  public delegate void OnInventoryChanged(List<Item> currentInventory);
  public static event OnInventoryChanged InventoryChanged;

  public void DropItem(Item item)
  {
    if (item == null) return;

    // Remove o item do inventário
    inventory.Remove(item);
    InventoryChanged?.Invoke(inventory);

    // Instancia o item no mundo
    Vector3 dropPosition = transform.position + transform.forward; // Dropa à frente do jogador
    GameObject droppedItem = Instantiate(itemDropPrefab, dropPosition, Quaternion.identity);

    // Configura o nome ou outros atributos no item dropado
    CollectableItem itemWorld = droppedItem.GetComponent<CollectableItem>();
    if (itemWorld != null)
    {
      itemWorld.SetItem(item);
    }

    Debug.Log($"Item {item.itemName} foi dropado no mundo!");
  }
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
  public void ConsumeItem(Item item)
  {
    // Garante que o item existe e pode ser consumido
    if (item != null && item is ConsumableItem consumable)
    {
      consumable.Consume(FindObjectOfType<PlayerHealth>());
      inventory.Remove(item); // Remove o item do inventário após consumir
      InventoryChanged?.Invoke(inventory);
      Debug.Log($"Consumiu {item.itemName}");
    }
  }

  public void Remove(Item item)
  {
    inventory.Remove(item);
    InventoryChanged?.Invoke(inventory);
  }

  public List<Item> GetInventory() => inventory;
}
