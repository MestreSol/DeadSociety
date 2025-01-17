using UnityEngine;
using UnityEngine.UI;

public class ItemSlotController : MonoBehaviour
{
  [Header("UI Components")]
  [SerializeField] private Image itemIcon;
  [SerializeField] private TMPro.TextMeshProUGUI itemNameText;
  [SerializeField] private TMPro.TextMeshProUGUI itemQuantityText;
  [SerializeField] private GameObject durabilityPanel;
  [SerializeField] private Image durabilityBar;
  [SerializeField] private Button itemInfoButton;

  private Item currentItem;

  public void Initialize(Item item)
  {
    currentItem = item;
    UpdateUI();
  }

  private void UpdateUI()
  {
    if (currentItem == null) return;

    // Atualiza os componentes de UI com os dados do item
    itemIcon.sprite = currentItem.itemIcon;
    itemNameText.text = currentItem.itemName;
    itemQuantityText.text = currentItem.itemAmount.ToString();

    // Atualiza a durabilidade
    UpdateDurabilityUI();
  }

  private void UpdateDurabilityUI()
  {
    if (currentItem == null || currentItem.currentDurability <= 0)
    {
      durabilityPanel.SetActive(false);
      return;
    }

    durabilityPanel.SetActive(true);
    float durabilityPercentage = currentItem.currentDurability / 100f;
    durabilityBar.fillAmount = durabilityPercentage;

    // Define a cor da barra com base na durabilidade
    if (durabilityPercentage > 0.5f)
    {
      durabilityBar.color = Color.green;
    }
    else if (durabilityPercentage > 0.25f)
    {
      durabilityBar.color = Color.yellow;
    }
    else
    {
      durabilityBar.color = Color.red;
    }
  }
}
