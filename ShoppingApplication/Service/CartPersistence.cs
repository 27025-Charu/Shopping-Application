using ShoppingApplication.Model;
using System.Text;
using System.Text.Json;

namespace ShoppingApplication.Service
{
    internal class CartPersistence
    {
        public static void Delete(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            try
            {
                string path = GetPath(username);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Logger.Info($"Persistent cart file deleted for user: {username}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to delete persistent cart file for user {username}: {ex.Message}");
            }
        }

        public static List<CartItem> Load(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new List<CartItem>();
            }
            try
            {
                string path = GetPath(username);
                if (!File.Exists(path))
                {
                    return new List<CartItem>();
                }
                string json = File.ReadAllText(path, Encoding.UTF8);
                return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load persistent cart for user {username}: {ex.Message}");
                return new List<CartItem>();
            }
        }

        private static string GetPath(string username)
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "Data");
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                return Path.Combine(baseDir, "guest.json");
            }

            string perfectUsername = username.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                perfectUsername = perfectUsername.Replace(c, '_');
            }

            return Path.Combine(baseDir, $"cart_{perfectUsername.ToLower()}.json");
        }

        public static void Save(string username, IEnumerable<CartItem> items)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }
            try
            {
                string path = GetPath(username);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(items, options);

                File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to save persistent cart for user {username}: {ex.Message}");
            }
        }
    }
}
