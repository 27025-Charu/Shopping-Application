using ShoppingApp.Model;
using ShoppingApp.Service;
using ShoppingApplication;
using ShoppingApplication.Repository;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShoppingApp
{
    internal class UserView
    {
        private readonly AuthService _auth;
        private readonly CartService _cart;
        private readonly ProductRepository _products;
        private readonly InvoiceService _invoices;
        private const int MaxAttempts = 3;

        public UserView(AuthService auth, CartService cart, ProductRepository products, InvoiceService invoices)
        {
            _auth = auth;
            _cart = cart;
            _products = products;
            _invoices = invoices;
        }

        public void RunApplicationLoop()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(@"==================================
SHOPPING APPLICATION
==================================
1. Sign Up
2. Login
3. Exit
----------------------------------
Enter choice: ");

                switch ((Console.ReadLine() ?? "").Trim())
                {
                    case "1":
                        if (SignUpPage())
                        {
                            DashboardPage();
                        }
                        break;
                    case "2":
                        if (LoginPage())
                        {
                            DashboardPage();
                        }
                        break;
                    case "3":
                        Logger.Info("Application exited");
                        Console.WriteLine(" Happy shopping!");
                        return;
                    default:
                        Logger.Warn("Invalid menu choice");
                        Console.WriteLine(" Press any key to continue...");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

        private bool LoginPage()
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                Console.Clear();
                Console.WriteLine("============ LOGIN ============");
                Console.Write(" Username : ");
                var username = (Console.ReadLine() ?? "").Trim();
                Console.Write(" Password : ");
                var password = ReadPassword();

                if (_auth.Login(username, password))
                {
                    Console.WriteLine("\n[SUCCESS] Login successful!");
                    Thread.Sleep(800);
                    return true;
                }
                Console.WriteLine($"\n[ERROR] Invalid username or password ({attempt}/3)");
                Console.WriteLine(" Press any key to continue...");
                Console.ReadKey(true);
            }
            Logger.Warn("Too many failed login attempts");
            return false;
        }

        private bool SignUpPage()
        {
            Console.Clear();
            Console.WriteLine("========== SIGN UP ==========");
            var first = ReadValid("First Name       : ", Validator.FirstName);
            var last = ReadValid("Last Name (opt)  : ", Validator.LastName);
            var dobText = ReadValid("DOB (DD/MM/YYYY): ", Validator.Dob);
            var phone = ReadValid("Phone Number     : ", Validator.Phone);
            var username = ReadValid("Username         : ", name =>
            {
                var formatValidation = Validator.Username(name);
                if (formatValidation is not null)
                {
                    return formatValidation;
                }

                if (_auth.UserNameExists(name))
                {
                    return "Username already taken.";
                }
                return null;
            });

            Console.WriteLine("Password: 8+ chars, 1 upper, 1 lower, 1 number, 1 special");
            string password;
            while (true)
            {
                Console.Write("Password : ");
                password = ReadPassword();
                var formatValidation = Validator.Password(password);
                if (formatValidation != null) 
                { 
                    Console.WriteLine($"[ERROR] { formatValidation}"); 
                    continue; 
                }

                Console.Write("Confirm Password : ");
                if (ReadPassword() == password)
                {
                    break;
                }
                Console.WriteLine("[ERROR] Passwords do not match. Try again.");
            }

            var user = new User
            {
                FirstName = first,
                LastName = string.IsNullOrWhiteSpace(last) ? null : last,
                Dob = DateTime.ParseExact(dobText, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                Phone = phone,
                Username = username
            };
            _auth.Register(user, password);
            Console.WriteLine("\n[SUCCESS] Account created! Redirecting to Dashboard...");
            Thread.Sleep(1200);
            return true;
        }


        private void DashboardPage()
        {

        }

        private static string ReadValid(string prompt, Func<string, string?> validate)
        {
            while (true)
            {
                Console.Write(prompt);
                var value = (Console.ReadLine() ?? "").Trim();
                var error = validate(value);
                if (error == null) return value;
                Console.WriteLine($"[ERROR] {error}");
            }
        }
        private static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out var n) && n >= min && n <= max) return n;
                Console.WriteLine($"[ERROR] Enter a number between {min} and {max}.");
            }
        }

        private static string ReadPassword()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) 
                { 
                    Console.WriteLine(); 
                    return sb.ToString(); 
                }
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0) 
                    { 
                        sb.Length--; 
                        Console.Write("\b \b"); 
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write('*');
                }
            }
        }

        private static string Trim(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            {
                return input;
            }

            if (maxLength <= 3)
            {
                return "...";
            }

            string truncatedText = input[..(maxLength - 3)];
            return truncatedText + "...";
        }
    }
}
