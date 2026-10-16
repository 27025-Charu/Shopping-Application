using ShoppingApplication.Repository;
using ShoppingApplication.Service;
using ShoppingApplication.View;

namespace ShoppingApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UserRepository userRepository = new UserRepository();
            CartService cartService = new CartService();
            AuthService authService = new AuthService(userRepository, cartService);
            ProductRepository productRepository = new ProductRepository();
            InvoiceRepository invoiceRepository = new InvoiceRepository();
            InvoiceService invoiceService = new InvoiceService(invoiceRepository);
            UserView userView = new UserView(authService, cartService, productRepository, invoiceService);
            userView.RunApplicationLoop();
            Console.WriteLine("Thank you for your visit...");
        }
    }
}
