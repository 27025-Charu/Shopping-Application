using ShoppingApplication.Model;
using ShoppingApplication.Service;

namespace ShoppingApplication
{
    internal class CartService
    {
        public const int MaxQty = 5;
        private readonly Dictionary<string, CartItem> _items = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<CartItem> Items => _items.Values;
        public int Count => _items.Values.Count();
        public decimal Total => _items.Values.Sum(i => i.Total);
        public bool IsEmpty => _items.Count == 0;

        public bool Contains(string id)
        {
            return _items.ContainsKey(id);
        }
        public int GetQty(string id)
        {
            if (_items.TryGetValue(id, out var i))
            {
                return i.Quantity;
            }
            else
            {
                return 0;
            }
        }
        public void SetQuantity(Product product, int qty)
        {
            if (qty <= 0)
            {
                _items.Remove(product.Id);
                Logger.Info($"Product - {product.Name} removed from cart");
                return;
            }
            var existed = _items.ContainsKey(product.Id);
            _items[product.Id] = new CartItem { Product = product, Quantity = qty };
            if (existed)
            {
                Logger.Info($"Product - {product.Name} quantity is updated to {qty}");
            }
            else
            {
                Logger.Info($"Product - {product.Name} with quantity - {qty} is added to cart");
            }
        }
        public bool AddQuantity(Product product, int qty)
        {
            if (qty <= 0)
            {
                return false;
            }

            if (_items.ContainsKey(product.Id))
            {
                int currentQuantity = _items[product.Id].Quantity;
                int newQuantity = currentQuantity + qty;
                if (newQuantity > 5)
                {
                    return false;
                }

                _items[product.Id].Quantity = newQuantity;
                Logger.Info($"Product - {product.Name} quantity updated to {newQuantity} in cart.");
                return true;
            }
            else
            {
                if (qty > 5)
                {
                    return false;
                }

                _items[product.Id] = new CartItem { Product = product, Quantity = qty };
                Logger.Info($"Product - {product.Name} with quantity - {qty} is added to cart.");
                return true;
            }
        }


        public void Clear()
        {
            _items.Clear();
        }
    }
}