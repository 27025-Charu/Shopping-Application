namespace ShoppingApplication.Model
{
    public class Coupon
    {
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "Percentage";
        public decimal Value { get; set; }
        public bool IsActive { get; set; } = true;
    }
}