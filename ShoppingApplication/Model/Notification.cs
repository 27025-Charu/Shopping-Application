namespace ShoppingApplication.Model
{
    internal class Notification
    {
        public int Id { get; init; }
        public NotificationLevel Level { get; init; }
        public string Message { get; init; }

        public Notification(int Id, NotificationLevel Level, string Message)
        {
            this.Id = Id;
            this.Level = Level;
            this.Message = Message;
        }
    }
}
