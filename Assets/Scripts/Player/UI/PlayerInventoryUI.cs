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

        // Adiciona os itens do inventário do jogador
        List<Item> items = playerInventory.GetInventory(); // Obtem os itens
        foreach (Item item in items)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer); // Cria um item na UI
           newItem.GetComponent<ItemSlotController>().Initialize(item);
        }
    }
}
