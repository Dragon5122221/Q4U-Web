namespace Q4UPrototypeWeb.Models
{
    public sealed class PaymentOverviewViewModel
    {
        public List<PaymentChargeViewModel> Charges { get; init; } = [];

        public decimal OutstandingTotal =>
            Charges
                .Where(c => c.StatusTone == "warning")
                .Sum(c => c.Amount);

        public decimal PaidTotal =>
            Charges
                .Where(c => c.StatusTone == "success")
                .Sum(c => c.Amount);

        public int OutstandingCount =>
            Charges.Count(c => c.StatusTone == "warning");

        public int PaidCount =>
            Charges.Count(c => c.StatusTone == "success");
    }


    public sealed class PaymentChargeViewModel
    {
        public string Id { get; init; } = string.Empty;

        public string Reference { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public string VehicleName { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public decimal Amount { get; init; }

        public string Date { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "info";

        public string SourceType { get; init; } = string.Empty;

        public string SourceReference { get; init; } = string.Empty;

        public bool IsOutstanding =>
            StatusTone == "warning";
    }


    public sealed class PaymentStartViewModel
    {
        public required PaymentChargeViewModel Charge { get; init; }

        public decimal ServiceAmount { get; init; }

        public decimal HandlingAmount { get; init; }

        public decimal Total =>
            ServiceAmount + HandlingAmount;
    }


    public sealed class PaymentReceiptViewModel
    {
        public string PaymentReference { get; init; } = string.Empty;

        public string ServiceReference { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string VehicleName { get; init; } = string.Empty;

        public string Registration { get; init; } = string.Empty;

        public decimal Amount { get; init; }

        public string PaymentMethod { get; init; } = string.Empty;

        public string PaymentDate { get; init; } = string.Empty;

        public string Status { get; init; } = "Paid";

        public bool IsNewPayment { get; init; }
    }
}