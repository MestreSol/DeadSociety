using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotController : MonoBehaviour
{
  public GameObject durabilityPanel;
  public GameObject itemIcon;
  public GameObject itemName;
  public GameObject itemQTD;
  public GameObject btn_ItemInfo;
  public Item item;

  public void Initialize(Item item)
  {
    this.item = item;
    itemIcon.GetComponent<Image>().sprite = item.itemIcon;
    itemName.GetComponent<TMPro.TextMeshProUGUI>().text = item.itemName;
    itemQTD.GetComponent<TMPro.TextMeshProUGUI>().text = item.itemAmount.ToString();
    UpdateDurability();
  }

  public void UpdateDurability()
  {
    if (item.currentDurability > 0)
    {
      durabilityPanel.SetActive(true);
      float durabilityPercentage = (float)item.currentDurability / 100;
      durabilityPanel.transform.localScale = new Vector3(durabilityPercentage, 1, 1);

      if (durabilityPercentage > 0.5f)
      {
        durabilityPanel.GetComponent<SpriteRenderer>().color = Color.green;
      }
      else if (durabilityPercentage > 0.25f)
      {
        durabilityPanel.GetComponent<SpriteRenderer>().color = Color.yellow;
      }
      else
      {
        durabilityPanel.GetComponent<SpriteRenderer>().color = Color.red;
      }
    }
    else
    {
      durabilityPanel.SetActive(false);
    }
  }


}
