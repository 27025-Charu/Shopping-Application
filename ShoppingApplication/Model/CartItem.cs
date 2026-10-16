namespace ShoppingApplication.Model
{
    public class CartItem
    {
        public Product Product { get; set; } = new();
        public int Quantity { get; set; }
        public decimal Total
        {
            get
            {
                return Product.Price * Quantity;
            }
        }
    }
}