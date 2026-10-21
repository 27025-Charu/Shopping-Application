namespace ShoppingApplication.Model
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new();
        public Coupon? AppliedCoupon { get; set; }

        public decimal PriceTotal => Items.Sum(item => item.Total);

        public decimal DiscountAmount
        {
            get
            {
                if (AppliedCoupon == null || !AppliedCoupon.IsActive) return 0m;

                return AppliedCoupon.DiscountType switch
                {
                    "Percentage" => PriceTotal * (AppliedCoupon.Value / 100m),
                    "Flat" => AppliedCoupon.Value > PriceTotal ? PriceTotal : AppliedCoupon.Value,
                    _ => 0m
                };
            }
        }
        public decimal Total => PriceTotal - DiscountAmount;
    }
}
