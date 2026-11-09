namespace Q4UPrototypeWeb.Models
{
    public sealed class VehicleLibraryViewModel
    {
        public List<VehicleSummaryViewModel> Vehicles { get; init; } = [];

        public int TotalVehicles =>
            Vehicles.Count;

        public int UpToDateCount =>
            Vehicles.Count(v => v.StatusTone == "success");

        public int AttentionCount =>
            Vehicles.Count(v =>
                v.StatusTone == "warning" ||
                v.StatusTone == "danger");
    }


    public sealed class VehicleSummaryViewModel
    {
        public string Id { get; init; } = string.Empty;

        public int Year { get; init; }

        public string Make { get; init; } = string.Empty;

        public string Model { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string Province { get; init; } = "GP";

        public string Colour { get; init; } = string.Empty;

        public string VinLastSix { get; init; } = string.Empty;

        public DateOnly LicenceExpiry { get; init; }

        public int DaysRemaining { get; init; }

        public string RenewalStatus { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "success";

        public int DocumentCount { get; init; }

        public int ServiceCount { get; init; }

        public decimal OutstandingBalance { get; init; }

        public string DisplayName =>
            $"{Year} {Make} {Model}";

        public string FullRegistration =>
            $"{Registration} {Province}";
    }


    public sealed class VehicleDetailsViewModel
    {
        public required VehicleSummaryViewModel Vehicle { get; init; }

        public string OwnerName { get; init; } = "Ayesha Khan";

        public string OwnerCustomerNumber { get; init; } = "Q4U-10482";

        public string EngineNumber { get; init; } = string.Empty;

        public string FullVin { get; init; } = string.Empty;

        public string BodyType { get; init; } = string.Empty;

        public string FuelType { get; init; } = string.Empty;

        public string Transmission { get; init; } = string.Empty;

        public string LicenceDiscNumber { get; init; } = string.Empty;

        public string RegistrationDate { get; init; } = string.Empty;

        public string Odometer { get; init; } = string.Empty;

        public List<VehicleDocumentViewModel> Documents { get; init; } = [];

        public List<VehicleServiceHistoryViewModel> ServiceHistory { get; init; } = [];

        public List<VehicleActivityViewModel> Activity { get; init; } = [];
    }


    public sealed class VehicleDocumentViewModel
    {
        public string Name { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public string UploadedDate { get; init; } = string.Empty;

        public string FileType { get; init; } = "PDF";

        public string Status { get; init; } = "Verified";

        public string StatusTone { get; init; } = "success";
    }


    public sealed class VehicleServiceHistoryViewModel
    {
        public string Reference { get; init; } = string.Empty;

        public string Service { get; init; } = string.Empty;

        public string Date { get; init; } = string.Empty;

        public string Amount { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "success";
    }


    public sealed class VehicleActivityViewModel
    {
        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string Date { get; init; } = string.Empty;

        public string Tone { get; init; } = "info";
    }
}