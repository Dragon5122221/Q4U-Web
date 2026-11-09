namespace Q4UPrototypeWeb.Models
{
    public sealed class ServiceRequestOverviewViewModel
    {
        public List<ServiceCatalogueItemViewModel> Services { get; init; } = [];

        public List<ServiceRequestSummaryViewModel> Requests { get; init; } = [];

        public int ActiveCount =>
            Requests.Count(r =>
                r.StatusTone == "info" ||
                r.StatusTone == "warning");

        public int ActionRequiredCount =>
            Requests.Count(r =>
                r.RequiresCustomerAction);

        public int CompletedCount =>
            Requests.Count(r =>
                r.StatusTone == "success");
    }


    public sealed class ServiceCatalogueItemViewModel
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string ShortName { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string Icon { get; init; } = string.Empty;

        public string Badge { get; init; } = string.Empty;

        public string BadgeTone { get; init; } = "info";

        public string Turnaround { get; init; } = string.Empty;

        public string PrototypeEstimate { get; init; } = string.Empty;

        public bool HasDedicatedJourney { get; init; }

        public bool Featured { get; init; }
    }


    public sealed class ServiceRequestSummaryViewModel
    {
        public string Reference { get; init; } = string.Empty;

        public string ServiceId { get; init; } = string.Empty;

        public string ServiceName { get; init; } = string.Empty;

        public string VehicleName { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string SubmittedDate { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "info";

        public int ProgressPercent { get; init; }

        public bool RequiresCustomerAction { get; init; }
    }


    public sealed class ServiceRequestStartViewModel
    {
        public string SelectedServiceId { get; init; } = "plates";

        public string SelectedVehicleId { get; init; } = "polo";

        public List<ServiceCatalogueItemViewModel> Services { get; init; } = [];

        public List<ServiceRequestVehicleViewModel> Vehicles { get; init; } = [];
    }


    public sealed class ServiceRequestVehicleViewModel
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string Province { get; init; } = "GP";

        public string Colour { get; init; } = string.Empty;

        public string FullRegistration =>
            $"{Registration} {Province}";
    }


    public sealed class ServiceRequestTrackViewModel
    {
        public string Reference { get; init; } = string.Empty;

        public string ServiceName { get; init; } = string.Empty;

        public string VehicleName { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string SubmittedDate { get; init; } = string.Empty;

        public string CurrentStatus { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "info";

        public int ProgressPercent { get; init; }

        public string ContactPreference { get; init; } = string.Empty;

        public string PrototypeEstimate { get; init; } = string.Empty;

        public bool IsNewRequest { get; init; }

        public List<ServiceRequestTimelineItemViewModel> Timeline { get; init; } = [];
    }


    public sealed class ServiceRequestTimelineItemViewModel
    {
        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string Timestamp { get; init; } = string.Empty;

        public string State { get; init; } = "pending";
    }
}