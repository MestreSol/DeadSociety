using UnityEngine;
using UnityEngine.UI;
using TMPro; // Para texto avançado (opcional)
using System.Collections.Generic;

public class PlayerInventoryUI : MonoBehaviour
{
    [Header("Inventory UI Settings")]
    public GameObject inventoryPanel; // O painel do inventário
    public Transform itemContainer; // O container para os itens (Grid Layout Group)
    public GameObject itemPrefab; // Prefab de um item para exibição

    private bool isInventoryOpen = false;
    public PlayerInventory playerInventory;

    private void Start()
    {
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory não encontrado no jogador!");
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

            // Atualiza a UI do inventário
            RefreshInventoryUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
        Debug.Log("Inventário " + (isInventoryOpen ? "aberto" : "fechado"));
    }

    private void RefreshInventoryUI()
    {
      // Limpa os itens antigos na UI
      foreach (Transform child in itemContainer)
      {
        Destroy(child.gameObject);
      }

      // Adiciona os itens filtrados na UI
      List<Item> items = playerInventory.GetInventory();
      foreach (var item in items)
      {
        GameObject newItem = Instantiate(itemPrefab, itemContainer); // Cria um item na UI
        TextMeshProUGUI itemText = newItem.GetComponentInChildren<TextMeshProUGUI>();
        if (itemText != null)
        {
          itemText.text = item.itemName; // Exibe o nome do item
        }

        // Botão de consumir
        Button consumeButton = newItem.GetComponentInChildren<Button>();
        if (consumeButton != null)
        {
          consumeButton.onClick.AddListener(() =>
          {
            // Consome o item e remove do inventário
            playerInventory.ConsumeItem(item);
            RefreshInventoryUI(); // Atualiza a UI
          });
        }
      }
    }
    public void ConsumeItem(Item item)
    {
      // Garante que o item existe e pode ser consumido
      if (item != null && item is ConsumableItem consumable)
      {
        consumable.Consume(FindObjectOfType<PlayerHealth>());
        playerInventory.Remove(item); // Remove o item do inventário após consumir
        Debug.Log($"Consumiu {item.itemName}");
      }
    }

    private void OnItemSlotClicked(Item item)
    {
      // Verifica se o item é consumível
      if (item is ConsumableItem consumableItem)
      {
        consumableItem.Consume(PlayerHealth.Instance); // Consome o item
        playerInventory.GetInventory().Remove(item); // Remove o item do inventário
        Debug.Log($"Consumiu o item {item.itemName}");
      }
      else
      {
        // Dropar o item no mundo
        playerInventory.DropItem(item);
        Debug.Log($"Item {item.itemName} dropado no mundo!");
      }

      // Atualiza a UI do inventário
      RefreshInventoryUI();
    }
}
