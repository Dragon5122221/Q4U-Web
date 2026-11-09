namespace Q4UPrototypeWeb.Models
{
    public sealed class DocumentLibraryViewModel
    {
        public List<PortalDocumentRecordViewModel> Documents { get; init; } = [];

        public int TotalDocuments =>
            Documents.Count;

        public int VerifiedDocuments =>
            Documents.Count(d => d.StatusTone == "success");

        public int AttentionDocuments =>
            Documents.Count(d =>
                d.StatusTone == "warning" ||
                d.StatusTone == "danger");

        public int VehicleDocuments =>
            Documents.Count(d =>
                !string.IsNullOrWhiteSpace(d.VehicleId));
    }


    public sealed class PortalDocumentRecordViewModel
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public string FileType { get; init; } = "PDF";

        public string FileSize { get; init; } = string.Empty;

        public string UploadedDate { get; init; } = string.Empty;

        public string UploadedBy { get; init; } = "Ayesha Khan";

        public string Status { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "success";

        public string VehicleId { get; init; } = string.Empty;

        public string VehicleName { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public bool RenewalRelevant { get; init; }

        public bool CustomerLevel =>
            string.IsNullOrWhiteSpace(VehicleId);

        public string Association =>
            CustomerLevel
                ? "Customer profile"
                : $"{VehicleName} · {Registration}";
    }


    public sealed class DocumentUploadViewModel
    {
        public List<DocumentVehicleOptionViewModel> Vehicles { get; init; } = [];
    }


    public sealed class DocumentVehicleOptionViewModel
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public string Province { get; init; } = "GP";

        public string Display =>
            $"{Name} · {Registration} {Province}";
    }
}