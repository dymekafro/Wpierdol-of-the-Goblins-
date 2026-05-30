namespace WPG.Items
{
    // Pojedynczy slot ekwipunku: id itemu + ilość. Pusty gdy quantity <= 0.
    public class InventorySlot
    {
        public string itemId;
        public int quantity;

        public bool IsEmpty => string.IsNullOrEmpty(itemId) || quantity <= 0;

        public ItemDefinition Definition => IsEmpty ? null : ItemDatabase.Get(itemId);

        public void Clear()
        {
            itemId = null;
            quantity = 0;
        }

        public void Set(string id, int qty)
        {
            itemId = id;
            quantity = qty;
            if (quantity <= 0) Clear();
        }
    }
}
