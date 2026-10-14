using ShoppingApplication.Model;

namespace ShoppingApplication.Model
{
    public class Invoice
    {
        public string Number { get; set; } = "";
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
