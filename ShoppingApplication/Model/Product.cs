namespace ShoppingApplication.Model
{
    public class Product
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Tax => Category switch
        {
            "Gaming" => Price * 0.18m,
            "Electronics" => Price * 0.15m,
            "Clothing" => Price * 0.05m,
            _ => 0m
        };
    }
}
