namespace Q4UPrototypeWeb.Models
{
    public sealed class RenewalOverviewViewModel
    {
        public List<RenewalVehicleViewModel> Vehicles { get; init; } = [];

        public List<RenewalRequestViewModel> ActiveRequests { get; init; } = [];

        public int UrgentCount =>
            Vehicles.Count(v => v.StatusTone == "danger");

        public int DueSoonCount =>
            Vehicles.Count(v => v.StatusTone == "warning");

        public int UpToDateCount =>
            Vehicles.Count(v => v.StatusTone == "success");
    }


    public sealed class RenewalVehicleViewModel
    {
        public string Id { get; init; } = string.Empty;

        public string VehicleName { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string Province { get; init; } = "GP";

        public DateOnly ExpiryDate { get; init; }

        public int DaysRemaining { get; init; }

        public string Status { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "success";

        public int ReadyDocumentCount { get; init; }

        public int RequiredDocumentCount { get; init; }

        public decimal PrototypeEstimate { get; init; }

        public string FullRegistration =>
            $"{Registration} {Province}";
    }


    public sealed class RenewalRequestViewModel
    {
        public string Reference { get; init; } = string.Empty;

        public string VehicleName { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string SubmittedDate { get; init; } = string.Empty;

        public string CurrentStage { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "info";

        public int ProgressPercent { get; init; }
    }


    public sealed class RenewalStartViewModel
    {
        public required RenewalVehicleViewModel SelectedVehicle { get; init; }

        public List<RenewalVehicleViewModel> Vehicles { get; init; } = [];

        public List<RenewalDocumentRequirementViewModel> Documents { get; init; } = [];

        public decimal PrototypeVehicleAmount { get; init; }

        public decimal PrototypeHandlingAmount { get; init; }

        public decimal PrototypeTotal =>
            PrototypeVehicleAmount +
            PrototypeHandlingAmount;
    }


    public sealed class RenewalDocumentRequirementViewModel
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "success";

        public bool IsReady { get; init; }

        public bool IsRequired { get; init; } = true;
    }


    public sealed class RenewalTrackViewModel
    {
        public string Reference { get; init; } = string.Empty;

        public string VehicleName { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string SubmittedDate { get; init; } = string.Empty;

        public string CurrentStatus { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "info";

        public decimal Amount { get; init; }

        public string PaymentStatus { get; init; } = string.Empty;

        public int ProgressPercent { get; init; }

        public bool IsNewRequest { get; init; }

        public List<RenewalTimelineStageViewModel> Timeline { get; init; } = [];
    }


    public sealed class RenewalTimelineStageViewModel
    {
        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string Timestamp { get; init; } = string.Empty;

        public string State { get; init; } = "pending";
    }
}