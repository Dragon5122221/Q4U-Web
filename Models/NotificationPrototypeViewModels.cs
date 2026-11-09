namespace Q4UPrototypeWeb.Models
{
    public sealed class NotificationCentreViewModel
    {
        public List<PortalNotificationViewModel> Notifications { get; init; } = [];

        public int UnreadCount =>
            Notifications.Count(n => !n.IsRead);

        public int UrgentCount =>
            Notifications.Count(n =>
                n.Priority == "urgent");

        public int ReminderCount =>
            Notifications.Count(n =>
                n.Type == "renewal");

        public int UpdateCount =>
            Notifications.Count(n =>
                n.Type == "service" ||
                n.Type == "document" ||
                n.Type == "payment");
    }


    public sealed class PortalNotificationViewModel
    {
        public string Id { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;

        public string Icon { get; init; } = string.Empty;

        public string Eyebrow { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;

        public string Timestamp { get; init; } = string.Empty;

        public string Priority { get; init; } = "normal";

        public bool IsRead { get; init; }

        public string ActionText { get; init; } = string.Empty;

        public string Controller { get; init; } = string.Empty;

        public string Action { get; init; } = string.Empty;

        public string RouteReference { get; init; } = string.Empty;

        public string RouteId { get; init; } = string.Empty;
    }
}