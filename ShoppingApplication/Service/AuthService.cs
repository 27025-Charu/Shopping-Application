using ShoppingApplication.Model;
using ShoppingApplication.Repository;
using ShoppingApplication.Service;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ShoppingApplication
{
    internal class AuthService
    {
        public const int MaxAddresses = 5;
        private readonly UserRepository _repo;
        private readonly CartService _cart;
        public User? CurrentUser { get; private set; }

        public AuthService(UserRepository repo, CartService cart)
        {
            _repo = repo;
            _cart = cart;
        }

        public bool UserNameExists(string username)
        {
            return _repo.Exists(username);
        }

        internal bool Login(string username, string password)
        {
            var user = _repo.GetUser(username);
            if (user == null || user.PasswordHash != Hash(password))
            {
                Logger.Warn($"Failed login - '{username}'");
                return false;
            }
            CurrentUser = user;
            Logger.Info($"User - {user.Username} logged in.");
            return true;
        }

        public bool Logout()
        {
            if (CurrentUser != null)
            {
                _cart.SaveSession(CurrentUser.Username);
                _cart.Clear();
                Logger.Info($"User - {CurrentUser.Username} logged out successfully.");
            }
            CurrentUser = null;
            return true;
        }


        internal void Register(User newUser, string password)
        {
            newUser.PasswordHash = Hash(password);
            _repo.RegisterUser(newUser);
            CurrentUser = newUser;
            Logger.Info($"New user - {newUser.Username} registered.");
        }

        internal bool VerifyIdentity(string username, string phone, string dobText)
        {
            var user = _repo.GetUser(username);
            var dobFlag = DateTime.TryParseExact(dobText, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dob);

            if (user == null || !dobFlag || user.Phone != phone || user.Dob.Date != dob.Date)
            {
                Logger.Warn($"Password reset verification failed - '{username}'");
                return false;
            }
            return true;
        }

        internal bool IsCurrentPassword(string username, string password)
        {
            var user = _repo.GetUser(username);
            return user != null && user.PasswordHash == Hash(password);
        }

        internal bool ResetPassword(string username, string newPassword)
        {
            var user = _repo.GetUser(username);
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = Hash(newPassword);
            _repo.UpdateUser(user);
            Logger.Info($"Password reset - {username}");
            return true;
        }
        internal bool AddAddress(Address address)
        {
            if (CurrentUser == null || CurrentUser.Addresses.Count >= MaxAddresses)
            {
                return false;
            }

            CurrentUser.Addresses.Add(address);
            _repo.UpdateUser(CurrentUser);
            Logger.Info($"Address added - {CurrentUser.Username} now has {CurrentUser.Addresses.Count} address(es).");
            return true;
        }

        private string Hash(string password)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }
    }
}