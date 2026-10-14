using ShoppingApp.Model;

namespace ShoppingApplication.Model
{
    public class Invoice
    {
        public string No { get; set; } = "";
        public DateTime Date { get; set; }
        public User Customer { get; set; } = new();
        public List<CartItem> Items { get; set; } = new();
        public decimal Total
        {
            get
            {
                return Items.Sum(i => i.Total);
            }
        }
    }
}
