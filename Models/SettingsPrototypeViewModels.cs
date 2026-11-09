namespace Q4UPrototypeWeb.Models
{
    public sealed class SettingsViewModel
    {
        public string FullName { get; init; } = "Ayesha Khan";

        public string FirstName { get; init; } = "Ayesha";

        public string LastName { get; init; } = "Khan";

        public string CustomerNumber { get; init; } = "Q4U-10482";

        public string EmailAddress { get; init; } =
            "ayesha.khan@example.co.za";

        public string MobileNumber { get; init; } =
            "082 555 0184";

        public string IdDisplay { get; init; } =
            "•••••• •••• •• •";

        public string AddressLine1 { get; init; } =
            "24 Rose Avenue";

        public string Suburb { get; init; } =
            "Mayfair";

        public string City { get; init; } =
            "Johannesburg";

        public string Province { get; init; } =
            "Gauteng";

        public string PostalCode { get; init; } =
            "2092";

        public int VehicleCount { get; init; } = 3;

        public int DocumentCount { get; init; } = 9;

        public string MemberSince { get; init; } =
            "July 2026";

        public string LastSignIn { get; init; } =
            "16 August 2026 · 17:34";

        public string PasswordLastChanged { get; init; } =
            "03 July 2026";
    }
}