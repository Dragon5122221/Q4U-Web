namespace Q4UPrototypeWeb.Models
{
    public sealed class PortalDashboardViewModel
    {
        public string FirstName { get; init; } = string.Empty;

        public string FullName { get; init; } = string.Empty;

        public string CustomerNumber { get; init; } = string.Empty;

        public string EmailAddress { get; init; } = string.Empty;

        public int UnreadNotificationCount { get; init; }

        public List<PortalVehicleViewModel> Vehicles { get; init; } = [];

        public List<PortalServiceRequestViewModel> RecentRequests { get; init; } = [];

        public List<PortalDocumentViewModel> RecentDocuments { get; init; } = [];

        public PortalVehicleViewModel SelectedVehicle =>
            Vehicles.First();
    }


    public sealed class PortalVehicleViewModel
    {
        public string Id { get; init; } = string.Empty;

        public int Year { get; init; }

        public string Make { get; init; } = string.Empty;

        public string Model { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string Province { get; init; } = "GP";

        public DateOnly LicenceExpiry { get; init; }

        public int DaysRemaining { get; init; }

        public string RenewalStatus { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "success";

        public int DocumentCount { get; init; }

        public decimal OutstandingBalance { get; init; }

        public int CompletedServices { get; init; }

        public string DisplayName =>
            $"{Year} {Make} {Model}";
    }


    public sealed class PortalServiceRequestViewModel
    {
        public string Reference { get; init; } = string.Empty;

        public string ServiceName { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string SubmittedDate { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "info";
    }


    public sealed class PortalDocumentViewModel
    {
        public string Name { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string UploadedDate { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "success";
    }
}