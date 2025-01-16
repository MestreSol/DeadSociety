using UnityEngine;

public class CollectableItem : MonoBehaviour, IInteractableObject
{
  public Item itemName;

  public void SetItem(Item item)
  {
    itemName = item;
  }
  public void Interact()
  {
    Debug.Log($"Você coletou: {itemName.itemName}");
    PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
    playerInventory?.AddItem(itemName);
    Destroy(gameObject);
  }
  public string GetInteractionMessage()
  {
    return $"Você coletou: {itemName.itemName}";
  }
}
