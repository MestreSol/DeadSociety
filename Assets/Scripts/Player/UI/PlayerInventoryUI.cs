using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerInventoryUI : MonoBehaviour
{
    [Header("Inventory UI Settings")]
    [SerializeField] private GameObject inventoryPanel; // Painel de inventário
    [SerializeField] private Transform itemContainer; // Container para itens (Grid Layout Group)
    [SerializeField] private GameObject itemSlotPrefab; // Prefab para slots de itens

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

        // Subscrição ao evento de mudança no inventário
        playerInventory.InventoryChanged += RefreshInventoryUI;

        // Atualiza a UI inicialmente
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

        Debug.Log("Inventário " + (isInventoryOpen ? "aberto" : "fechado"));
    }

    private void RefreshInventoryUI(List<Item> items)
    {
        // Limpa apenas se necessário
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
