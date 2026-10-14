using ShoppingApplication.Model;
using Newtonsoft.Json;

namespace ShoppingApplication
{
    internal class UserRepository
    {
        private readonly string _path = Path.Combine(AppContext.BaseDirectory, "ShoppingAppData", "users.json");
        private readonly List<User> _users;
        public UserRepository()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            _users = File.Exists(_path)
                ? JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(_path)) ?? new()
                : new();
        }

        public bool Exists(string username)
        {
            return _users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        internal User? GetUser(string username)
        {
            return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        internal void RegisterUser(User newUser)
        {
            _users.Add(newUser);
            File.WriteAllText(_path, JsonConvert.SerializeObject(_users, Formatting.Indented));
        }
    }
}