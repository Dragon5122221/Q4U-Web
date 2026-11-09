namespace Q4UPrototypeWeb.Models
{
    public sealed class PortalSectionViewModel
    {
        public string ActiveNavigation { get; init; } = string.Empty;

        public string Eyebrow { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string PrimaryActionLabel { get; init; } = string.Empty;

        public string PrimaryActionMessage { get; init; } = string.Empty;

        public List<PortalSectionMetricViewModel> Metrics { get; init; } = [];

        public List<PortalSectionItemViewModel> Items { get; init; } = [];
    }


    public sealed class PortalSectionMetricViewModel
    {
        public string Label { get; init; } = string.Empty;

        public string Value { get; init; } = string.Empty;

        public string SupportingText { get; init; } = string.Empty;

        public string Tone { get; init; } = "default";
    }


    public sealed class PortalSectionItemViewModel
    {
        public string Reference { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Subtitle { get; init; } = string.Empty;

        public string Meta { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string StatusTone { get; init; } = "default";

        public string ActionLabel { get; init; } = "View";
    }
}