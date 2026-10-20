namespace ShoppingApplication.Model
{
    public class CartItem
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public Product Product { get; set; } = new();

        public decimal Total
        {
            get
            {
                return (Product.Price * Quantity) + (Product.Tax * Quantity);
            }
        }
    }
}