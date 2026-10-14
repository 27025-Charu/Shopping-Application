using ShoppingApp.Model;
using ShoppingApp.Service;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ShoppingApp
{
    internal class AuthService
    {
        private readonly UserRepository _repo;
        public User? CurrentUser { get; private set; }

        public AuthService(UserRepository repo)
        {
            _repo = repo;
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

        internal void Register(User newUser, string password)
        {
            newUser.PasswordHash = Hash(password);
            _repo.RegisterUser(newUser);
            CurrentUser = newUser;
            Logger.Info($"New user - {newUser.Username} registered.");
        }

        private string Hash(string password)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }
    }
}