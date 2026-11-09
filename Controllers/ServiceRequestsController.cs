using Microsoft.AspNetCore.Mvc;
using Q4UPrototypeWeb.Models;

namespace Q4UPrototypeWeb.Controllers
{
    public class ServiceRequestsController : Controller
    {
        public IActionResult Index()
        {
            var model = new ServiceRequestOverviewViewModel
            {
                Services = BuildServices(),

                Requests =
                [
                    new()
                    {
                        Reference = "Q4U-260814-018",
                        ServiceId = "renewal",
                        ServiceName = "Licence Renewal",
                        VehicleName = "2020 BMW 320i",
                        Registration = "KP 19 FX GP",
                        SubmittedDate = "14 Aug 2026",
                        Status = "Documents received",
                        StatusTone = "info",
                        ProgressPercent = 42
                    },

                    new()
                    {
                        Reference = "Q4U-260810-011",
                        ServiceId = "plates",
                        ServiceName = "Number Plates",
                        VehicleName = "2021 Volkswagen Polo",
                        Registration = "JT 42 ZN GP",
                        SubmittedDate = "10 Aug 2026",
                        Status = "Ready for collection",
                        StatusTone = "success",
                        ProgressPercent = 100
                    },

                    new()
                    {
                        Reference = "Q4U-260812-015",
                        ServiceId = "roadworthy",
                        ServiceName = "Roadworthy Assistance",
                        VehicleName = "2022 Toyota Hilux",
                        Registration = "LM 81 TK GP",
                        SubmittedDate = "12 Aug 2026",
                        Status = "Appointment confirmed",
                        StatusTone = "info",
                        ProgressPercent = 58
                    },

                    new()
                    {
                        Reference = "Q4U-260722-006",
                        ServiceId = "fines",
                        ServiceName = "Traffic Fine Assistance",
                        VehicleName = "2022 Toyota Hilux",
                        Registration = "LM 81 TK GP",
                        SubmittedDate = "22 Jul 2026",
                        Status = "Completed",
                        StatusTone = "success",
                        ProgressPercent = 100
                    }
                ]
            };

            return View(model);
        }


        public IActionResult Start(
            string service = "plates",
            string vehicle = "polo")
        {
            if (service.Equals(
                "renewal",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    "Start",
                    "Renewals",
                    new
                    {
                        id = vehicle
                    });
            }

            var validServices =
                BuildServices()
                    .Where(s => !s.HasDedicatedJourney)
                    .ToList();

            if (!validServices.Any(s =>
                    s.Id.Equals(
                        service,
                        StringComparison.OrdinalIgnoreCase)))
            {
                service = "plates";
            }

            var model = new ServiceRequestStartViewModel
            {
                SelectedServiceId = service,

                SelectedVehicleId = vehicle,

                Services = validServices,

                Vehicles = BuildVehicles()
            };

            return View(model);
        }


        public IActionResult Track(
            string reference = "Q4U-260812-015",
            bool newRequest = false)
        {
            if (newRequest)
            {
                var newModel =
                    new ServiceRequestTrackViewModel
                    {
                        Reference = reference,
                        ServiceName = "Number Plates",
                        VehicleName = "2021 Volkswagen Polo 1.0 TSI",
                        Registration = "JT 42 ZN GP",
                        SubmittedDate = "16 August 2026 · 17:28",
                        CurrentStatus = "Request received",
                        StatusTone = "info",
                        ProgressPercent = 20,
                        ContactPreference = "SMS",
                        PrototypeEstimate = "R420.00",
                        IsNewRequest = true,

                        Timeline =
                        [
                            new()
                            {
                                Title = "Request submitted",
                                Description =
                                    "Your service request has been received by Q4U.",
                                Timestamp = "16 Aug · 17:28",
                                State = "complete"
                            },

                            new()
                            {
                                Title = "Initial review",
                                Description =
                                    "The request details and supporting information are being reviewed.",
                                Timestamp = "Current stage",
                                State = "active"
                            },

                            new()
                            {
                                Title = "Service preparation",
                                Description =
                                    "Q4U will prepare the requested service once the review is complete.",
                                Timestamp = "Pending",
                                State = "pending"
                            },

                            new()
                            {
                                Title = "Ready / complete",
                                Description =
                                    "You will be notified when the service reaches its completion stage.",
                                Timestamp = "Pending",
                                State = "pending"
                            }
                        ]
                    };

                return View(newModel);
            }


            if (reference.Equals(
                "Q4U-260810-011",
                StringComparison.OrdinalIgnoreCase))
            {
                return View(
                    new ServiceRequestTrackViewModel
                    {
                        Reference = reference,
                        ServiceName = "Number Plates",
                        VehicleName = "2021 Volkswagen Polo 1.0 TSI",
                        Registration = "JT 42 ZN GP",
                        SubmittedDate = "10 August 2026 · 10:04",
                        CurrentStatus = "Ready for collection",
                        StatusTone = "success",
                        ProgressPercent = 100,
                        ContactPreference = "SMS",
                        PrototypeEstimate = "R420.00",

                        Timeline =
                        [
                            new()
                            {
                                Title = "Request submitted",
                                Description =
                                    "Replacement number plate request received.",
                                Timestamp = "10 Aug · 10:04",
                                State = "complete"
                            },

                            new()
                            {
                                Title = "Information confirmed",
                                Description =
                                    "Vehicle and registration details were confirmed.",
                                Timestamp = "10 Aug · 11:12",
                                State = "complete"
                            },

                            new()
                            {
                                Title = "Plate preparation",
                                Description =
                                    "The prototype request moved into plate preparation.",
                                Timestamp = "12 Aug · 09:30",
                                State = "complete"
                            },

                            new()
                            {
                                Title = "Ready for collection",
                                Description =
                                    "The service is complete and ready for collection.",
                                Timestamp = "15 Aug · 15:16",
                                State = "complete"
                            }
                        ]
                    });
            }


            var model =
                new ServiceRequestTrackViewModel
                {
                    Reference = reference,
                    ServiceName = "Roadworthy Assistance",
                    VehicleName = "2022 Toyota Hilux 2.4 GD-6",
                    Registration = "LM 81 TK GP",
                    SubmittedDate = "12 August 2026 · 14:22",
                    CurrentStatus = "Appointment confirmed",
                    StatusTone = "info",
                    ProgressPercent = 58,
                    ContactPreference = "SMS & Portal",
                    PrototypeEstimate = "R650.00",

                    Timeline =
                    [
                        new()
                        {
                            Title = "Request submitted",
                            Description =
                                "Roadworthy assistance request created.",
                            Timestamp = "12 Aug · 14:22",
                            State = "complete"
                        },

                        new()
                        {
                            Title = "Vehicle details reviewed",
                            Description =
                                "Vehicle information was reviewed for appointment preparation.",
                            Timestamp = "12 Aug · 15:04",
                            State = "complete"
                        },

                        new()
                        {
                            Title = "Appointment confirmed",
                            Description =
                                "The requested appointment preference has been confirmed.",
                            Timestamp = "14 Aug · 10:15",
                            State = "active"
                        },

                        new()
                        {
                            Title = "Roadworthy assistance",
                            Description =
                                "The next service stage will occur on the confirmed appointment date.",
                            Timestamp = "Upcoming",
                            State = "pending"
                        },

                        new()
                        {
                            Title = "Complete",
                            Description =
                                "The portal will show the final service outcome when complete.",
                            Timestamp = "Pending",
                            State = "pending"
                        }
                    ]
                };

            return View(model);
        }


        private static List<ServiceCatalogueItemViewModel> BuildServices()
        {
            return
            [
                new()
                {
                    Id = "renewal",
                    Name = "Licence Renewals & Registrations",
                    ShortName = "Licence Renewal",
                    Description =
                        "Prepare and track vehicle licence renewal administration through the dedicated Q4U renewal journey.",
                    Icon = "R",
                    Badge = "Dedicated journey",
                    BadgeTone = "warning",
                    Turnaround = "Status dependent",
                    PrototypeEstimate = "See renewal",
                    HasDedicatedJourney = true,
                    Featured = true
                },

                new()
                {
                    Id = "plates",
                    Name = "Number Plates",
                    ShortName = "Number Plates",
                    Description =
                        "Request replacement or new number plates with vehicle and registration information already linked to your profile.",
                    Icon = "NP",
                    Badge = "While you wait",
                    BadgeTone = "warning",
                    Turnaround = "Service dependent",
                    PrototypeEstimate = "From R420",
                    Featured = true
                },

                new()
                {
                    Id = "personalised",
                    Name = "Personalised Plates",
                    ShortName = "Personalised Plates",
                    Description =
                        "Start a personalised-plate assistance request and capture the registration text you would like to explore.",
                    Icon = "★",
                    Badge = "Personal service",
                    BadgeTone = "info",
                    Turnaround = "Approval dependent",
                    PrototypeEstimate = "From R850"
                },

                new()
                {
                    Id = "roadworthy",
                    Name = "Roadworthy Assistance",
                    ShortName = "Roadworthy",
                    Description =
                        "Request assistance with preparing and coordinating a vehicle roadworthy process.",
                    Icon = "✓",
                    Badge = "Appointment",
                    BadgeTone = "success",
                    Turnaround = "By appointment",
                    PrototypeEstimate = "From R650"
                },

                new()
                {
                    Id = "fines",
                    Name = "Traffic Fine Assistance",
                    ShortName = "Traffic Fines",
                    Description =
                        "Capture a traffic-fine reference and request administrative assistance from the Q4U team.",
                    Icon = "!",
                    Badge = "Admin support",
                    BadgeTone = "info",
                    Turnaround = "Case dependent",
                    PrototypeEstimate = "From R350"
                },

                new()
                {
                    Id = "fleet",
                    Name = "Fleet Services",
                    ShortName = "Fleet Services",
                    Description =
                        "Request a consultation for multi-vehicle administration, renewals and fleet-related support.",
                    Icon = "F",
                    Badge = "Business",
                    BadgeTone = "info",
                    Turnaround = "Consultation",
                    PrototypeEstimate = "Custom"
                }
            ];
        }


        private static List<ServiceRequestVehicleViewModel> BuildVehicles()
        {
            return
            [
                new()
                {
                    Id = "polo",
                    Name = "2021 Volkswagen Polo 1.0 TSI",
                    Registration = "JT 42 ZN",
                    Province = "GP",
                    Colour = "Pure White"
                },

                new()
                {
                    Id = "hilux",
                    Name = "2022 Toyota Hilux 2.4 GD-6",
                    Registration = "LM 81 TK",
                    Province = "GP",
                    Colour = "Graphite Grey"
                },

                new()
                {
                    Id = "bmw",
                    Name = "2020 BMW 320i",
                    Registration = "KP 19 FX",
                    Province = "GP",
                    Colour = "Mineral Grey"
                }
            ];
        }
    }
}