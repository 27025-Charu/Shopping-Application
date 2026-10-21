namespace ShoppingApplication.Model
{
    public class Invoice
    {
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public User Customer { get; set; } = new();
        public List<CartItem> Items { get; set; } = new();
        public Address DeliveryAddress { get; set; }
        public string AppliedCouponCode { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }

        public decimal Total => Items.Sum(i => i.Total);
        public decimal GrandTotal => Total - DiscountAmount;
    }
}