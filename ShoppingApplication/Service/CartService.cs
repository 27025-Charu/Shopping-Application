using ShoppingApplication.Model;

namespace ShoppingApplication.Service
{
    internal class CartService
    {
        public const int MaxQty = 5;
        private readonly Dictionary<string, CartItem> _items = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<CartItem> Items => _items.Values;
        public int Count => _items.Values.Sum(i => i.Quantity);
        public decimal Total => _items.Values.Sum(i => i.Total);
        public Coupon? AppliedCoupon { get; private set; }
        public bool IsEmpty => _items.Count == 0;
        public decimal PriceTotal => _items.Values.Sum(i => i.Product.Price * i.Quantity);
        public decimal TaxTotal => _items.Values.Sum(i => i.Product.Tax * i.Quantity);
        public decimal SubTotal => _items.Values.Sum(i => i.Total);
        public decimal DiscountTotal
        {
            get
            {
                if (AppliedCoupon == null || !AppliedCoupon.IsActive)
                {
                    return 0;
                }
                return AppliedCoupon.DiscountType switch
                {
                    "Percentage" => SubTotal * (AppliedCoupon.Value / 100m),
                    "Flat" => AppliedCoupon.Value > SubTotal ? SubTotal : AppliedCoupon.Value,
                    _ => 0m
                };
            }
        }
        public decimal GrandTotal => SubTotal - DiscountTotal;
        public bool ApplyCoupon(Coupon? coupon)
        {
            if (coupon != null && coupon.IsActive)
            {
                AppliedCoupon = coupon;
                return true;
            }
            return false;
        }

        public void RemoveCoupon()
        {
            AppliedCoupon = null;
        }

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

        public void Clear()
        {
            _items.Clear();
            AppliedCoupon = null;
        }

        public void SaveSession(string username)
        {
            CartPersistence.Save(username, _items.Values);
        }

        public void LoadSession(string username)
        {
            _items.Clear();
            AppliedCoupon = null; // coupon code status is cleared on session reload
            var savedItems = CartPersistence.Load(username);
            foreach (var item in savedItems)
            {
                if (item?.Product != null)
                {
                    _items[item.Product.Id] = item;
                }
            }
        }
    }
}