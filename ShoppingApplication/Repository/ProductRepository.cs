using Newtonsoft.Json;
using ShoppingApplication.Model;

namespace ShoppingApplication.Repository
{
    internal class ProductRepository
    {
        private readonly string _prodPath = Path.Combine(AppContext.BaseDirectory, "ShoppingAppData", "products.json");
        private readonly string _couponPath = Path.Combine(AppContext.BaseDirectory, "ShoppingAppData", "coupons.json");

        private readonly List<Product> _products;
        private readonly List<Coupon> _coupons;
        private readonly object _lock = new();

        public ProductRepository()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_prodPath)!);
            if (!File.Exists(_prodPath))
            {
                File.WriteAllText(_prodPath, JsonConvert.SerializeObject(SeedProducts(), Formatting.Indented));
            }
            _products = JsonConvert.DeserializeObject<List<Product>>(File.ReadAllText(_prodPath)) ?? new();

            if (!File.Exists(_couponPath))
            {
                File.WriteAllText(_couponPath, JsonConvert.SerializeObject(SeedCoupons(), Formatting.Indented));
            }
            _coupons = JsonConvert.DeserializeObject<List<Coupon>>(File.ReadAllText(_couponPath)) ?? new();
        }

        public List<Product> GetAll()
        {
            lock (_lock)
            {
                return _products;
            }
        }

        public Product? GetById(string id)
        {
            lock (_lock)
            {
                var prod = _products.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (prod == null) return null;

                return new Product
                {
                    Id = prod.Id,
                    Name = prod.Name,
                    Category = prod.Category,
                    Price = prod.Price,
                };
            }
        }

        public Coupon? GetCouponByCode(string code)
        {
            lock (_lock)
            {
                var coupon = _coupons.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase) && c.IsActive);
                if (coupon == null) return null;

                return new Coupon
                {
                    Code = coupon.Code,
                    DiscountType = coupon.DiscountType,
                    Value = coupon.Value,
                    IsActive = coupon.IsActive
                };
            }
        }

        private static List<Product> SeedProducts() => new()
        {
            new() { Id = "G101", Name = "Game Controller",    Category = "Gaming",      Price = 2499 },
            new() { Id = "G102", Name = "Gaming Headset",     Category = "Gaming",      Price = 1999 },
            new() { Id = "G103", Name = "Gaming Mouse",       Category = "Gaming",      Price = 1299 },
            new() { Id = "E201", Name = "Bluetooth Speaker",  Category = "Electronics", Price = 1799 },
            new() { Id = "E202", Name = "Power Bank 10000",   Category = "Electronics", Price = 999 },
            new() { Id = "E203", Name = "Smart Watch",        Category = "Electronics", Price = 3499 },
            new() { Id = "C301", Name = "T-Shirt",            Category = "Clothing",    Price = 499 },
            new() { Id = "C302", Name = "Hoodie",             Category = "Clothing",    Price = 1299 },
            new() { Id = "C303", Name = "Jeans",              Category = "Clothing",    Price = 1599 },
        };

        private static List<Coupon> SeedCoupons() => new()
        {
            new() { Code = "CLOTHING10", DiscountType = "Percentage", Value = 10, IsActive = true },
            new() { Code = "GAMER20",   DiscountType = "Percentage", Value = 20, IsActive = true },
            new() { Code = "FLAT500",   DiscountType = "Flat",       Value = 500, IsActive = true },
            new() { Code = "EXPIRED5",  DiscountType = "Percentage", Value = 5,  IsActive = false }
        };
    }
}
