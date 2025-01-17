using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerInventoryUI : MonoBehaviour
{
  [Header("Inventory UI Settings")]
  [SerializeField] private GameObject inventoryPanel;
  [SerializeField] private Transform itemContainer;
  [SerializeField] private GameObject itemSlotPrefab;

  private bool isInventoryOpen = false;
  private PlayerInventory playerInventory;

  private void Start()
  {
    playerInventory = FindObjectOfType<PlayerInventory>();

    if (playerInventory == null)
    {
      Debug.LogError("PlayerInventory não encontrado!");
      return;
    }

    playerInventory.InventoryChanged += RefreshInventoryUI;
    RefreshInventoryUI(playerInventory.GetInventory());
  }

  private void OnDestroy()
  {
    if (playerInventory != null)
    {
      playerInventory.InventoryChanged -= RefreshInventoryUI;
    }
  }

  public void ToggleInventory()
  {
    isInventoryOpen = !isInventoryOpen;
    inventoryPanel.SetActive(isInventoryOpen);
    UpdateCursorState();
    Debug.Log("Inventário " + (isInventoryOpen ? "aberto" : "fechado"));
  }

  private void UpdateCursorState()
  {
    if (isInventoryOpen)
    {
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
      Time.timeScale = 0f;
    }
    else
    {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
      Time.timeScale = 1f;
    }
  }

  private void RefreshInventoryUI(List<Item> items)
  {
    foreach (Transform child in itemContainer)
    {
      Destroy(child.gameObject);
    }

    foreach (var item in items)
    {
      GameObject itemSlot = Instantiate(itemSlotPrefab, itemContainer);
      ItemSlotUI slotUI = itemSlot.GetComponent<ItemSlotUI>();

      if (slotUI != null)
      {
        slotUI.SetItem(item, this);
      }
    }
  }

  public void OnConsumeItem(Item item)
  {
    if (item is ConsumableItem consumable)
    {
      consumable.Consume(FindObjectOfType<PlayerHealth>());
      playerInventory.Remove(item);
      Debug.Log($"Consumiu {item.itemName}");
    }
  }

  public void OnDropItem(Item item)
  {
    playerInventory.DropItem(item);
    Debug.Log($"Item {item.itemName} dropado!");
  }
}
