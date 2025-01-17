using System;

public static class CollectableItemEventSystem
{
    public static event Action<Item> OnItemCollected;

    public static void RaiseItemCollected(Item item)
    {
        OnItemCollected?.Invoke(item);
    }
}

