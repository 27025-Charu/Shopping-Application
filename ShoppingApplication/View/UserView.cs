using ShoppingApplication.Model;
using ShoppingApplication.Repository;
using ShoppingApplication.Service;
using Spectre.Console;
using System.Globalization;
using System.Text;

namespace ShoppingApplication.View
{
    internal class UserView
    {
        private readonly AuthService _auth;
        private readonly CartService _cart;
        private readonly ProductRepository _products;
        private readonly InvoiceService _invoices;
        private const int MaxAttempts = 3;
        private const int NoticeLines = 3;
        private const int MaxCartLines = 4;

        private const int MinAddQty = 1, MaxAddQty = 5;
        private const int MinCols = 70, MinRows = 30;

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
                ClearScreen();
                Console.Write(@"Welcome to the Shopping App!
==================================
SHOPPING APPLICATION
==================================
1. Sign Up
2. Login
3. Reset Password
4. Exit
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
                        ResetPasswordPage();
                        break;
                    case "4":
                        Logger.Info("Application exited");
                        Console.WriteLine(" Happy shopping!");
                        return;
                    default:
                        Logger.Warn("Invalid menu choice");
                        Console.WriteLine(" Invalid choice. Please enter 1, 2, 3 or 4.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

        private static void ClearScreen()
        {
            Console.Clear();
            if (AnsiConsole.Profile.Capabilities.Ansi)
            {
                Console.Write("\u001b[3J");
            }
        }

        private static string Line(char c)
        {
            return new string(c, Math.Max(10, Math.Min(50, Console.WindowWidth - 1)));
        }

        private bool LoginPage()
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                ClearScreen();
                Console.WriteLine("============ LOGIN ============");
                AnsiConsole.MarkupLine("[grey]B = Back to menu   S = If New user then Sign Up   F = Forgot password[/]");
                Console.Write("Username : ");
                var username = (Console.ReadLine() ?? "").Trim();

                if (username.Equals("B", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                if (username.Equals("S", StringComparison.OrdinalIgnoreCase))
                {
                    return SignUpPage();
                }
                if (username.Equals("F", StringComparison.OrdinalIgnoreCase))
                {
                    attempt = ResetPasswordPage() ? 0 : attempt - 1;
                    continue;
                }

                Console.Write("Password : ");
                var password = ReadPassword();
                if (_auth.Login(username, password))
                {
                    Console.WriteLine("\n[SUCCESS] Login successful!\nRedirecting to Home screen catalog page...");
                    _cart.LoadSession(username);
                    Thread.Sleep(1000);
                    return true;
                }

                Console.WriteLine($"\n[ERROR] ({attempt}/{MaxAttempts}). Invalid username or password");
                if (attempt < MaxAttempts)
                {
                    Console.WriteLine("Note: type F as the username if you forgot your password, or S to sign up.");
                }
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }

            Logger.Warn("Too many failed login attempts");
            ClearScreen();
            Console.WriteLine("[ERROR] Too many failed attempts. Returning to the main menu.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            return false;
        }

        private bool ResetPasswordPage()
        {
            ClearScreen();
            Console.WriteLine("========== RESET PASSWORD ==========");
            AnsiConsole.MarkupLine("[grey]Confirm your details first. Type B as the username to cancel.[/]");

            Console.Write("Username             : ");
            var username = (Console.ReadLine() ?? "").Trim();
            if (username.Equals("B", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            Console.Write("Registered Phone     : ");
            var phone = (Console.ReadLine() ?? "").Trim();
            Console.Write("DOB (DD/MM/YYYY)     : ");
            var dob = (Console.ReadLine() ?? "").Trim();

            if (!_auth.VerifyIdentity(username, phone, dob))
            {
                Console.WriteLine("\n[ERROR] The details do not match our records. Password was not changed.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                return false;
            }

            Console.WriteLine("\n[SUCCESS] Details verified.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Password: 8+ chars, 1 upper, 1 lower, 1 number, 1 special");
            Console.ResetColor();

            string newPassword;
            while (true)
            {
                Console.Write("New Password         : ");
                newPassword = ReadPassword();
                var formatValidation = Validator.Password(newPassword);
                if (formatValidation != null)
                {
                    Console.WriteLine($"[ERROR] {formatValidation}");
                    continue;
                }
                if (_auth.IsCurrentPassword(username, newPassword))
                {
                    Console.WriteLine("[ERROR] New password must be different from the old password.");
                    continue;
                }

                Console.Write("Confirm New Password : ");
                if (ReadPassword() == newPassword)
                {
                    break;
                }
                Console.WriteLine("[ERROR] Passwords do not match. Try again.");
            }

            _auth.ResetPassword(username, newPassword);
            Console.WriteLine("\n[SUCCESS] Password reset successful! Please login with your new password.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            return true;
        }

        private bool SignUpPage()
        {
            ClearScreen();
            Console.WriteLine("========== SIGN UP ==========");
            if (!ReadValidWithLimit("First Name       : ", Validator.FirstName, out string first))
            {
                return false;
            }
            if (!ReadValidWithLimit("Last Name (opt)  : ", Validator.LastName, out string last))
            {
                return false;
            }
            if (!ReadValidWithLimit("DOB (DD/MM/YYYY): ", input => Validator.Dob((input ?? "").Replace("-", "/").Replace(".", "/")), out string rawDobText))
            {
                return false;
            }
            if (!ReadValidWithLimit("Phone Number     : ", Validator.Phone, out string phone))
            {
                return false;
            }
            bool usernameValid = ReadValidWithLimit("Username         : ", input =>
            {
                string? formatError = Validator.Username(input);
                if (formatError != null)
                {
                    return formatError;
                }

                string? availabilityError = AvailableUsername(input);
                if (availabilityError != null) return availabilityError;

                return null;
            }, out string username);

            if (!usernameValid)
            {
                return false;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Password: 8+ chars, 1 upper, 1 lower, 1 number, 1 special");
            Console.ResetColor();

            string password = string.Empty;
            const int MaxAttempts = 3;
            bool passwordSetupSuccess = false;

            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                Console.Write("Password : ");
                password = ReadPassword();

                var formatValidation = Validator.Password(password);
                if (formatValidation != null)
                {
                    Console.WriteLine($"[ERROR] {formatValidation} (Attempt {attempt}/{MaxAttempts})");
                    continue;
                }

                Console.Write("Confirm Password : ");
                if (ReadPassword() == password)
                {
                    passwordSetupSuccess = true;
                    break;
                }

                Console.WriteLine($"[ERROR] Passwords do not match. (Attempt {attempt}/{MaxAttempts})");
            }

            if (!passwordSetupSuccess)
            {
                Failure("Too many invalid password attempts.");
                return false;
            }

            var addresses = new List<Address>();
            Console.Write("\nAdd a delivery address now? (Y/N, optional): ");

            while ((Console.ReadLine() ?? "").Trim().Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                addresses.Add(ReadAddress());
                if (addresses.Count >= AuthService.MaxAddresses)
                {
                    Console.WriteLine($"You can save up to {AuthService.MaxAddresses} addresses.");
                    break;
                }
                Console.Write("\nAdd another address? (Y/N): ");
            }
            string dobText = rawDobText.Replace("-", "/").Replace(".", "/");

            var user = new User
            {
                FirstName = first,
                LastName = last,
                Dob = DateTime.ParseExact(dobText, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                Phone = phone,
                Username = username,
                Addresses = addresses
            };

            _auth.Register(user, password);
            Console.WriteLine("\n[SUCCESS] Account created! Redirecting to Dashboard...");
            Thread.Sleep(1000);
            return true;
        }

        private string AvailableUsername(string name)
        {
            var formatError = Validator.Username(name);
            if (formatError is not null)
            {
                return formatError;
            }
            if (_auth.UserNameExists(name))
            {
                return "Username already taken.";
            }
            else
            {
                return null;
            }
        }

        private bool ReadValidWithLimit(string prompt, Func<string, string> validator, out string result)
        {
            const int MaxAttempts = 3;

            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                Console.Write(prompt);
                string input = (Console.ReadLine() ?? "").Trim();

                string error = validator(input);
                if (error == null)
                {
                    result = input;
                    return true;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {error} (Attempt {attempt}/{MaxAttempts})");
                Console.ResetColor();
            }

            result = string.Empty;
            Failure("Too many invalid field input attempts.");
            return false;
        }

        private void Failure(string explicitReason)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ERROR] Registration failed. {explicitReason}");
            Console.ResetColor();
            Thread.Sleep(2000);
        }

        private static Address ReadAddress()
        {
            Console.WriteLine("\n--- Delivery Address ---");
            var street = ReadValid("Street / House No  : ", Validator.Street);
            var city = ReadValid("City               : ", Validator.City);
            var pincode = ReadValid("Pincode            : ", Validator.Pincode);
            var phone = ReadValid("Phone (this address): ", Validator.Phone);
            return new Address { Street = street, City = city, Pincode = pincode, Phone = phone };
        }

        private void DashboardPage()
        {
            while (true)
            {
                DrawDashboard();
                var input = ReadLineWithResize(DashboardPrompt, DrawDashboard).Trim();
                switch (input.ToUpper())
                {
                    case "":
                        break;
                    case "X":
                        if (_auth.Logout())
                        {
                            _cart.Clear();
                            Notifier.Clear();
                            return;
                        }
                        break;
                    case "C":
                        Logger.Info("Cart opened");
                        CartPage();
                        break;
                    default:
                        ProcessProductSelection(input);
                        break;
                }
            }
        }

        private void DrawDashboard()
        {
            ClearScreen();
            Console.WriteLine($"Welcome, {_auth.CurrentUser!.FirstName}");
            AnsiConsole.MarkupLine("[bold white on blue] [[C]] CART [/]   [bold white on red] [[X]] LOGOUT [/]");
            Console.WriteLine(Line('='));

            DrawProducts();
            DrawNotificationsAndCart();

            AnsiConsole.Write(new Rule("[bold yellow]YOUR CHOICE[/]").LeftJustified());
            if (Console.WindowWidth < MinCols || Console.WindowHeight < MinRows)
            {
                AnsiConsole.MarkupLine("[yellow]Window is small - make it larger to see the whole screen.[/]");
            }
        }

        private static string DashboardPrompt()
        {
            if (Console.WindowWidth >= MinCols)
            {
                return "Enter Product ID to add (or C = Cart, X = Logout): ";
            }
            else
            {
                return "Product ID (C=Cart, X=Logout): ";
            }
        }

        private void DrawProducts()
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("[bold]Category[/]")
                .AddColumn(new TableColumn("[blue]ID[/]").Centered())
                .AddColumn("[green]Product Name[/]")
                .AddColumn(new TableColumn("[magenta]Price[/]").RightAligned());

            foreach (var group in _products.GetAll().GroupBy(p => p.Category))
            {
                var firstRow = true;
                foreach (var p in group)
                {
                    table.AddRow(
                        firstRow ? $"[bold yellow]{Markup.Escape(group.Key.ToUpper())}[/]" : "",
                        Markup.Escape(p.Id.ToString()),
                        Markup.Escape(p.Name),
                        $"Rs. {p.Price}/-");
                    firstRow = false;
                }
            }
            AnsiConsole.Write(table);
        }

        private void DrawNotificationsAndCart()
        {
            var columnWidth = Math.Max(20, (Console.WindowWidth - 9) / 2);
            var notices = Notifier.Latest(NoticeLines)
                .Select(n => $"[{Color(n.Level)}] > {Markup.Escape(Trim(n.Message, columnWidth - 2))}[/]")
                .ToList();
            if (notices.Count == 0)
            {
                notices.Add("[grey]> No notifications yet[/]");
            }

            var items = _cart.Items.ToList();
            var cartLines = items
                .Take(MaxCartLines)
                .Select(i => Trim($"[{i.Product.Id}] {Trim(i.Product.Name, 18)} * {i.Quantity} Price:  Rs. {i.Total:N2}", columnWidth))
                .ToList();
            if (items.Count == 0)
            {
                cartLines.Add("Cart is empty");
            }
            else if (items.Count > MaxCartLines)
            {
                cartLines.Add($"...and {items.Count - MaxCartLines} more item(s)");
            }
            cartLines.Add(Trim($"Items: {_cart.Count}  Total: Rs. {_cart.GrandTotal:N2}/-", columnWidth));


            var panel = new Table()
                .Border(TableBorder.Rounded)
                .Expand()
                .AddColumn("[bold cyan]NOTIFICATIONS[/]")
                .AddColumn("[bold cyan]LIVE CART[/]");

            panel.AddRow(new Markup(string.Join("\n", notices)), new Text(string.Join("\n", cartLines)));
            AnsiConsole.Write(panel);
        }

        private static string Color(NotificationLevel level)
        {
            if (level == NotificationLevel.Success)
            {
                return "green";
            }
            else if (level == NotificationLevel.Warning)
            {
                return "yellow";
            }
            else
            {
                return "red";
            }
        }

        private void CartPage()
        {
            var currentMark = Notifier.Id;
            while (true)
            {
                DrawCartPage(currentMark);

                var key = ReadKeyWithResize(() => DrawCartPage(currentMark));
                switch (key)
                {
                    case ConsoleKey.E:
                        EditOrRemove(false);
                        break;
                    case ConsoleKey.R:
                        EditOrRemove(true);
                        break;
                    case ConsoleKey.C:
                        int preMark = Notifier.Id;
                        ApplyCouponWorkflow();
                        currentMark = preMark;
                        break;
                    case ConsoleKey.K:
                        if (_cart.IsEmpty)
                        {
                            Logger.Warn("Checkout attempted with an empty cart");
                            Notifier.Warning("Your cart is empty. Add products before checkout.");
                            break;
                        }
                        if (CheckoutPage())
                        {
                            return;
                        }
                        break;
                    case ConsoleKey.B:
                        return;
                    default:
                        Logger.Warn("Invalid option choice.");
                        Notifier.Warning("Please choose E, R, C, K or B.");
                        break;
                }
            }
        }


        private void ApplyCouponWorkflow()
        {
            if (_cart.IsEmpty)
            {
                Notifier.Warning("Your cart is empty. Cannot apply coupon codes.");
                return;
            }

            Console.Write("\nEnter Coupon Code (or type 'NO' to clear): ");
            var code = (Console.ReadLine() ?? "").Trim();

            if (code.Equals("NO", StringComparison.OrdinalIgnoreCase))
            {
                if (_cart.AppliedCoupon != null)
                {
                    string oldCode = _cart.AppliedCoupon.Code;
                    _cart.RemoveCoupon();
                    Logger.Info($"Coupon code '{oldCode}' removed from cart.");
                    Notifier.Success($"Coupon '{oldCode}' successfully removed.");
                }
                else
                {
                    Notifier.Warning("No coupon was active to remove.");
                }
                return;
            }

            var coupon = _products.GetCouponByCode(code);
            if (coupon != null && coupon.IsActive)
            {
                if (_cart.AppliedCoupon != null)
                {
                    string oldCode = _cart.AppliedCoupon.Code;
                    if (oldCode.Equals(coupon.Code, StringComparison.OrdinalIgnoreCase))
                    {
                        Notifier.Warning($"Coupon '{coupon.Code}' is already applied.");
                        return;
                    }

                    Logger.Info($"Replacing existing coupon '{oldCode}' with new coupon '{coupon.Code}'.");
                    Notifier.Success($"Removed old coupon '{oldCode}'.");
                }

                _cart.ApplyCoupon(coupon);
                Logger.Info($"Coupon '{coupon.Code}' applied successfully.");
                Notifier.Success($"Coupon '{coupon.Code}' applied! Saved Rs. {_cart.DiscountTotal:N2}.");
            }
            else
            {
                Logger.Warn($"Invalid or expired coupon attempt: '{code}'");
                Notifier.Error("Invalid or expired coupon code. Please try again.");
            }
        }


        private void DrawCartPage(int id)
        {
            ClearScreen();
            AnsiConsole.Write(new Rule("[bold yellow]YOUR CART[/]").LeftJustified());

            if (_cart.IsEmpty)
            {
                AnsiConsole.MarkupLine("\n[grey][[ Your cart is empty ]][/]\n");
            }
            else
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("[blue]ID[/]")
                    .AddColumn("[green]Product[/]")
                    .AddColumn(new TableColumn("[bold]Qty[/]").RightAligned())
                    .AddColumn(new TableColumn("[yellow]Tax[/]").RightAligned())
                    .AddColumn(new TableColumn("[magenta]Total[/]").RightAligned());

                foreach (var i in _cart.Items)
                {
                    decimal productTaxTotal = i.Product.Tax * i.Quantity;
                    table.AddRow(
                        Markup.Escape(i.Product.Id),
                        Markup.Escape(i.Product.Name),
                        i.Quantity.ToString(),
                        $"Rs. {productTaxTotal:N2}",
                        $"Rs. {i.Total:N2}");
                }
                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"\n[cyan]Price * Quantity:   Rs. {_cart.PriceTotal:N2}/- [/]");
                AnsiConsole.MarkupLine($"[cyan]Total Tax       :   Rs. {_cart.TaxTotal:N2}/- [/]");
                AnsiConsole.MarkupLine($"[yellow]SubTotal        :   Rs. {_cart.Total:N2}/-[/]");
                if (_cart.AppliedCoupon != null)
                {
                    AnsiConsole.MarkupLine($"Discount ({_cart.AppliedCoupon.Code}): -Rs. {_cart.DiscountTotal:N2}/-");
                }
                AnsiConsole.MarkupLine($"[yellow]Grand Total     :   Rs. {_cart.GrandTotal:N2}[/]/-");
            }

            Console.WriteLine(Line('='));
            DrawCartNotifications(id);
            Console.Write(@"
[E] Edit Quantity   [R] Remove [C] Apply Coupon
[K] Checkout   [B] Back

Choice: ");
        }

        private void DrawCartNotifications(int id)
        {
            var textWidth = Math.Max(20, Console.WindowWidth - 8);

            var lines = Notifier.GetNotificationsAfter(id, NoticeLines).Select(n => $"[{Color(n.Level)}]> {Markup.Escape(Trim(n.Message, textWidth))}[/]").ToList();
            if (lines.Count == 0 && _cart.IsEmpty)
            {
                lines.Add("[yellow]> Your cart is empty. Press A to add a product or B to go back.[/]");
            }
            if (lines.Count == 0)
            {
                lines.Add("[grey]> No new notifications[/]");
            }

            var panel = new Table()
    .Border(TableBorder.Rounded)
    .Expand()
    .AddColumn("[bold cyan]NOTIFICATIONS[/]");

            panel.AddRow(new Markup(string.Join("\n", lines)));
            AnsiConsole.Write(panel);
        }

        private void EditOrRemove(bool remove)
        {
            if (_cart.IsEmpty)
            {
                Logger.Warn("Edit/remove attempted with an empty cart");
                if (remove)
                {
                    Notifier.Warning("Your cart is empty. There is nothing to remove.");
                }
                else
                {
                    Notifier.Warning("Your cart is empty. There is nothing to edit.");
                }
                return;
            }

            Console.Write("\nEnter Product ID: ");
            var id = (Console.ReadLine() ?? "").Trim();
            var product = _products.GetById(id);
            if (product == null || !_cart.Contains(product.Id))
            {
                Logger.Warn($"Product Id- '{id}' is not in your cart");
                Notifier.Warning($"'{id}' is not in your cart.");
                return;
            }

            int quantity;
            if (remove)
            {
                quantity = 0;
            }
            else
            {
                quantity = ReadInt($"New quantity for {product.Name} (0 = remove): ", 0, CartService.MaxQty);
            }
            if (quantity == 0)
            {
                bool confirmRemoval = AnsiConsole.Confirm($"Are you sure you want to remove {Markup.Escape(product.Name)}[/] from your cart?");
                if (!confirmRemoval)
                {
                    Logger.Info($"Removal of {product.Name} canceled by user.");
                    Notifier.Success("Action canceled. Item remains in your cart.");
                    return;
                }
            }
            ChangeQuantity(product, quantity);
        }

        private void ProcessProductSelection(string productId)
        {
            var product = _products.GetById(productId);
            if (product == null)
            {
                Logger.Warn($"Invalid product ID '{productId}'");
                Notifier.Warning($"Product ID '{productId}' was not found.");
                return;
            }

            var current = _cart.GetQty(product.Id);
            var size = Math.Min(MaxAddQty, CartService.MaxQty - current);
            if (size < MinAddQty)
            {
                Logger.Warn($"Max quantity already in cart for {product.Id}");
                Notifier.Warning($"{product.Name}: maximum quantity ({CartService.MaxQty}) already in cart. Change it from the cart.");
                return;
            }
            string prompt;

            if (current == 0)
            {
                prompt = $"Quantity for {product.Name} ({MinAddQty}-{size}): ";
            }
            else
            {
                if (size == MinAddQty)
                {
                    prompt = $"{product.Name} is already in cart ( x {current}). Add how many more? (can add only {size}): ";
                }
                else
                {
                    prompt = $"{product.Name} is already in cart ( x {current}). Add how many more? (Can be between {MinAddQty} and {size}): ";
                }
            }
            var qty = ReadInt(prompt, MinAddQty, size);
            AddQuantity(product, qty);
        }

        private void AddQuantity(Product product, int qty)
        {
            var current = _cart.GetQty(product.Id);
            _cart.SetQuantity(product, current + qty);
            if (current == 0)
            {
                Notifier.Success($"{product.Name} * {qty} added to cart");
            }
            else
            {
                Notifier.Success($"{product.Name} +{qty} added (now x {current + qty})");
            }
        }

        private void ChangeQuantity(Product product, int qty)
        {
            _cart.SetQuantity(product, qty);
            if (qty == 0)
            {
                Notifier.Success($"{product.Name} removed from cart");
            }
            else
            {
                Notifier.Success($"{product.Name} quantity changed to {qty}");
            }
        }

        private bool CheckoutPage()
        {
            var user = _auth.CurrentUser!;
            ClearScreen();
            Console.WriteLine($@"=========== CHECKOUT ===========
Customer : {user.FullName} (UserName - {user.Username})
Items    : {_cart.Count}
Total    : Rs. {_cart.GrandTotal:N2}/-");
            if (_cart.AppliedCoupon != null)
            {
                Console.WriteLine($"Coupon   : {_cart.AppliedCoupon.Code} (Rs. {_cart.DiscountTotal:N2}/-)");
            }
            Console.WriteLine("--------------------------------");

            Address address = ChooseAddress();
            if (address == null)
            {
                Logger.Info("Checkout cancelled - no address chosen");
                return false;
            }
            Console.WriteLine($"\nDeliver to : {address}");
            Console.Write("Confirm payment? (Y/N): ");
            if (Console.ReadLine()?.Trim().ToUpper() != "Y")
            {
                Logger.Info("Checkout cancelled by user");
                return false;
            }

            Console.Clear();
            Logger.Info($"Payment of Rs. {_cart.GrandTotal:N2}/- successful (PAID)");
            Console.WriteLine("\n[SUCCESS] Payment successful!!!");

            var invoice = _invoices.Display(user, _cart);
            invoice.DeliveryAddress = address;

            PrintInvoice(invoice);
            try
            {
                var path = _invoices.CreatePdf(invoice);
                Logger.Info($"Invoice PDF saved: {path}");
                Console.WriteLine($"\n[SUCCESS] Invoice saved to: {path}");
                Notifier.Success($"Payment successful. Invoice {invoice.Number} saved.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Invoice PDF wasn't generated due to : {ex.Message}");
                Logger.Error(ex.ToString());
                Console.WriteLine("\n[ERROR] Payment done, but the PDF could not be created.");
                Notifier.Error("Payment done, but the invoice PDF could not be created.");
            }

            string checkedOutUser = user.Username;
            _cart.Clear();
            CartPersistence.Delete(checkedOutUser);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            return true;
        }


        private Address? ChooseAddress()
        {
            var user = _auth.CurrentUser!;
            while (true)
            {
                Console.WriteLine("\nDELIVERY ADDRESS");
                for (var i = 0; i < user.Addresses.Count; i++)
                {
                    Console.WriteLine($" {i + 1}. {user.Addresses[i]}");
                }
                if (user.Addresses.Count == 0)
                {
                    Console.Write("No saved address yet. [A] Add address, [B] Back: ");
                }
                else
                {
                    Console.Write($"Choose 1-{user.Addresses.Count}, [A] Add new address, [B] Back: ");
                }

                var input = (Console.ReadLine() ?? "").Trim();
                if (input.Equals("B", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
                {
                    var added = AddNewAddress(user);
                    if (added != null)
                    {
                        return added;
                    }
                    continue;
                }

                if (int.TryParse(input, out var number) && number >= 1 && number <= user.Addresses.Count)
                {
                    return user.Addresses[number - 1];
                }

                Console.WriteLine("[ERROR] Invalid choice.");
            }
        }

        private Address? AddNewAddress(User user)
        {
            if (user.Addresses.Count >= AuthService.MaxAddresses)
            {
                Console.WriteLine($"[ERROR] You can save up to {AuthService.MaxAddresses} addresses. Please choose a saved one.");
                return null;
            }

            var address = ReadAddress();
            _auth.AddAddress(address);
            Console.WriteLine("[SUCCESS] Address saved to your account.");
            return address;
        }

        private void PrintInvoice(Invoice inv)
        {
            Console.WriteLine("==================================");
            Console.WriteLine($"INVOICE {inv.Number}");
            Console.WriteLine($"Date: {inv.Date:dd-MM-yyyy}  Time: {inv.Date:HH:mm:ss}");
            Console.WriteLine($"User: {inv.Customer.Username}");
            Console.WriteLine($"Name: {inv.Customer.FirstName}");
            Console.WriteLine($"Deliver to: {inv.DeliveryAddress}");
            Console.WriteLine("==================================");

            foreach (var i in inv.Items)
            {
                Console.WriteLine($"Id: {i.Product.Id} Name: {i.Product.Name} Quantity: {i.Quantity} * Product price: Rs. {i.Product.Price:N2} = Rs. {i.Total:N2}");
            }

            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"Subtotal   : Rs. {inv.Total:N2}");
            if (!string.IsNullOrWhiteSpace(inv.AppliedCouponCode))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Discount   : -Rs. {inv.DiscountAmount:N2} ({inv.AppliedCouponCode})");
                Console.ResetColor();
            }

            Console.WriteLine($"TOTAL PAID : Rs. {inv.GrandTotal:N2}");
            Console.WriteLine("-----------------------------------");
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
                if (int.TryParse(Console.ReadLine(), out var n) && n >= min && n <= max)
                {
                    return n;
                }
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

        private static string ReadLineWithResize(Func<string> prompt, Action redraw)
        {
            var buffer = new StringBuilder();
            var (width, height) = (Console.WindowWidth, Console.WindowHeight);

            Console.Write(prompt());

            while (true)
            {
                if (HasWindowResized(ref width, ref height))
                {
                    redraw();
                    Console.Write(prompt() + buffer);
                }

                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(50);
                    continue;
                }

                var key = Console.ReadKey(intercept: true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return buffer.ToString();

                    case ConsoleKey.Backspace:
                        if (buffer.Length > 0)
                        {
                            buffer.Length--;
                            Console.Write("\b \b");
                        }
                        break;

                    default:
                        if (!char.IsControl(key.KeyChar))
                        {
                            buffer.Append(key.KeyChar);
                            Console.Write(key.KeyChar);
                        }
                        break;
                }
            }
        }

        private static ConsoleKey ReadKeyWithResize(Action redraw)
        {
            var (width, height) = (Console.WindowWidth, Console.WindowHeight);

            while (true)
            {
                if (HasWindowResized(ref width, ref height))
                {
                    redraw();
                }

                if (Console.KeyAvailable)
                {
                    return Console.ReadKey(intercept: true).Key;
                }

                Thread.Sleep(50);
            }
        }

        private static bool HasWindowResized(ref int lastWidth, ref int lastHeight)
        {
            int currentWidth = Console.WindowWidth;
            int currentHeight = Console.WindowHeight;

            if (currentWidth == lastWidth && currentHeight == lastHeight)
            {
                return false;
            }

            do
            {
                lastWidth = currentWidth;
                lastHeight = currentHeight;
                Thread.Sleep(150);
                currentWidth = Console.WindowWidth;
                currentHeight = Console.WindowHeight;
            }
            while (currentWidth != lastWidth || currentHeight != lastHeight);

            return true;
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