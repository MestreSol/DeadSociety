using UnityEngine;

public class CollectableItem : MonoBehaviour, IInteractable
{
    [SerializeField] private Item item;

    public void SetItem(Item newItem)
    {
        item = newItem;
    }

    public void Interact()
    {
        if (item == null) return;

        Debug.Log($"Você coletou: {item.itemName}");

        // Dispara evento de coleta para outros sistemas
        CollectableItemEventSystem.RaiseItemCollected(item);

        Destroy(gameObject);
    }

    public string GetInteractionText()
    {
        return "Pressione E para coletar";
    }
}