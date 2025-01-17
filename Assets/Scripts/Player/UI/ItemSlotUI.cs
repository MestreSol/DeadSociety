using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
  [Header("UI Components")]
  [SerializeField] private TextMeshProUGUI itemNameText;
  [SerializeField] private Button consumeButton;
  [SerializeField] private Button dropButton;

  private Item item;
  private PlayerInventoryUI inventoryUI;

  public void SetItem(Item item, PlayerInventoryUI inventoryUI)
  {
    this.item = item;
    this.inventoryUI = inventoryUI;

    if (itemNameText != null)
    {
      itemNameText.text = item.itemName;
    }

    if (consumeButton != null)
    {
      consumeButton.onClick.AddListener(() => inventoryUI.OnConsumeItem(item));
    }

    if (dropButton != null)
    {
      dropButton.onClick.AddListener(() => inventoryUI.OnDropItem(item));
    }
  }
}
