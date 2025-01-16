  using UnityEngine;

  [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
  public class Item : ScriptableObject
  {
    public string itemName;
    public PlayerInventory.ItemCategory itemCategory;
    public Sprite itemIcon;
    public int itemAmount;
    public bool isStackable;
    public float currentDurability;
  }
