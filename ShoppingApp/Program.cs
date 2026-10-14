using System;
using System.Collections.Generic;
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
            UserService userService = new UserService(userRepository);
            UserView userView=new UserView(userService);
            userView.RunApplicationLoop();
            Console.WriteLine("Thank you for your visit...");
        }
    }
}
