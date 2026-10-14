using ShoppingApplication.Repository;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Shopping App!");
            UserRepository userRepository = new UserRepository();
            AuthService authService = new AuthService(userRepository);
            CartService cartService = new CartService();    
            ProductRepository productRepository = new ProductRepository();
            InvoiceRepository invoiceRepository=new InvoiceRepository();
            InvoiceService invoiceService = new InvoiceService();
            AuthService userService = new AuthService(userRepository);
            UserView userView = new UserView(authService,cartService,productRepository,invoiceService);
            userView.RunApplicationLoop();
            Console.WriteLine("Thank you for your visit...");
        }
    }
}
