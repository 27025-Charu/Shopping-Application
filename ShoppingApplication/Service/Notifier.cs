using ShoppingApplication.Model;

namespace ShoppingApplication.service
{
    internal static class Notifier
    {
        private const int Capacity = 20;
        private static readonly List<Notification> _notices = new();
        private static readonly object _lock = new();
        private static int _lastId;

        public static int Id
        {
            get
            {
                lock (_lock)
                {
                    return _lastId;
                }
            }
        }

        public static void Success(string message) => Add(NotificationLevel.Success, message);
        public static void Warning(string message) => Add(NotificationLevel.Warning, message);
        public static void Error(string message) => Add(NotificationLevel.Error, message);

        public static List<Notification> Latest(int count)
        {
            lock (_lock)
            {
                return _notices.TakeLast(count).ToList();
            }
        }

        public static Notification? LastAfter(int mark)
        {
            lock (_lock)
            {
                return _notices.LastOrDefault(n => n.Id > mark);
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _notices.Clear();
            }
        }

        private static void Add(NotificationLevel level, string message)
        {
            lock (_lock)
            {
                _notices.Add(new Notification(++_lastId, level, message));

                if (_notices.Count > Capacity)
                {
                    _notices.RemoveAt(0);
                }
            }
        }
    }
}
