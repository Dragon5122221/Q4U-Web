using Microsoft.AspNetCore.Mvc;
using Q4UPrototypeWeb.Models;

namespace Q4UPrototypeWeb.Controllers
{
    public class RenewalsController : Controller
    {
        public IActionResult Index()
        {
            var model = new RenewalOverviewViewModel
            {
                Vehicles = BuildVehicles(),

                ActiveRequests =
                [
                    new()
                    {
                        Reference = "Q4U-260814-018",
                        VehicleName = "2020 BMW 320i Sport Line",
                        Registration = "KP 19 FX GP",
                        SubmittedDate = "14 August 2026",
                        CurrentStage = "Documents received",
                        StatusTone = "info",
                        ProgressPercent = 42
                    }
                ]
            };

            return View(model);
        }


        public IActionResult Start(string id = "bmw")
        {
            var vehicles = BuildVehicles();

            var selectedVehicle =
                vehicles.FirstOrDefault(v =>
                    v.Id.Equals(
                        id,
                        StringComparison.OrdinalIgnoreCase))
                ?? vehicles.First();

            var model = new RenewalStartViewModel
            {
                Vehicles = vehicles,

                SelectedVehicle = selectedVehicle,

                PrototypeVehicleAmount =
                    selectedVehicle.Id == "bmw"
                        ? 934.50m
                        : selectedVehicle.Id == "polo"
                            ? 912.00m
                            : 965.00m,

                PrototypeHandlingAmount = 350.00m,

                Documents =
                [
                    new()
                    {
                        Id = "registration",
                        Name = "Vehicle Registration Certificate",
                        Description =
                            "Registration document already stored in your Q4U profile.",
                        Status = "Verified",
                        StatusTone = "success",
                        IsReady = true
                    },

                    new()
                    {
                        Id = "identity",
                        Name = "Owner Identification",
                        Description =
                            "Customer identification linked to Ayesha Khan.",
                        Status = "Verified",
                        StatusTone = "success",
                        IsReady = true
                    },

                    new()
                    {
                        Id = "address",
                        Name = "Proof of Address",
                        Description =
                            "Current customer proof of address.",
                        Status = "Verified",
                        StatusTone = "success",
                        IsReady = true
                    },

                    new()
                    {
                        Id = "notice",
                        Name = "Renewal Supporting Document",
                        Description =
                            "Supporting renewal document for the selected vehicle.",
                        Status = "Received",
                        StatusTone = "info",
                        IsReady = true
                    },

                    new()
                    {
                        Id = "disc",
                        Name = "Current Licence Disc Image",
                        Description =
                            "Add a clear image of the current licence disc for this prototype journey.",
                        Status = "Required",
                        StatusTone = "warning",
                        IsReady = false
                    }
                ]
            };

            return View(model);
        }


        public IActionResult Track(
            string reference = "Q4U-260814-018",
            bool newRequest = false)
        {
            if (newRequest)
            {
                var newModel = new RenewalTrackViewModel
                {
                    Reference = reference,
                    VehicleName = "2020 BMW 320i Sport Line",
                    Registration = "KP 19 FX GP",
                    SubmittedDate = "16 August 2026 · 16:58",
                    CurrentStatus = "Submitted",
                    StatusTone = "info",
                    Amount = 1284.50m,
                    PaymentStatus = "Prototype payment confirmed",
                    ProgressPercent = 18,
                    IsNewRequest = true,

                    Timeline =
                    [
                        new()
                        {
                            Title = "Request submitted",
                            Description =
                                "Your renewal request has been received by Q4U.",
                            Timestamp = "16 Aug 2026 · 16:58",
                            State = "complete"
                        },

                        new()
                        {
                            Title = "Document review",
                            Description =
                                "Q4U will review the information and supporting documents.",
                            Timestamp = "Next step",
                            State = "active"
                        },

                        new()
                        {
                            Title = "Renewal processing",
                            Description =
                                "The renewal will move into processing once the information is confirmed.",
                            Timestamp = "Pending",
                            State = "pending"
                        },

                        new()
                        {
                            Title = "Completion",
                            Description =
                                "You will be notified when the renewal process is complete.",
                            Timestamp = "Pending",
                            State = "pending"
                        }
                    ]
                };

                return View(newModel);
            }


            var model = new RenewalTrackViewModel
            {
                Reference = reference,
                VehicleName = "2020 BMW 320i Sport Line",
                Registration = "KP 19 FX GP",
                SubmittedDate = "14 August 2026 · 09:18",
                CurrentStatus = "Documents received",
                StatusTone = "info",
                Amount = 1284.50m,
                PaymentStatus = "Payment confirmed",
                ProgressPercent = 42,
                IsNewRequest = false,

                Timeline =
                [
                    new()
                    {
                        Title = "Request submitted",
                        Description =
                            "Your licence renewal request was created successfully.",
                        Timestamp = "14 Aug · 09:18",
                        State = "complete"
                    },

                    new()
                    {
                        Title = "Documents received",
                        Description =
                            "Required prototype documentation has been received.",
                        Timestamp = "14 Aug · 11:03",
                        State = "complete"
                    },

                    new()
                    {
                        Title = "Q4U review",
                        Description =
                            "The Q4U team is reviewing the vehicle information.",
                        Timestamp = "In progress",
                        State = "active"
                    },

                    new()
                    {
                        Title = "Renewal processing",
                        Description =
                            "The renewal will move into processing after review.",
                        Timestamp = "Pending",
                        State = "pending"
                    },

                    new()
                    {
                        Title = "Ready / completed",
                        Description =
                            "You will receive an SMS and portal notification when complete.",
                        Timestamp = "Pending",
                        State = "pending"
                    }
                ]
            };

            return View(model);
        }


        private static List<RenewalVehicleViewModel> BuildVehicles()
        {
            return
            [
                new()
                {
                    Id = "bmw",
                    VehicleName = "2020 BMW 320i Sport Line",
                    Registration = "KP 19 FX",
                    Province = "GP",
                    ExpiryDate = new DateOnly(2026, 8, 22),
                    DaysRemaining = 6,
                    Status = "Urgent",
                    StatusTone = "danger",
                    ReadyDocumentCount = 4,
                    RequiredDocumentCount = 5,
                    PrototypeEstimate = 1284.50m
                },

                new()
                {
                    Id = "polo",
                    VehicleName = "2021 Volkswagen Polo 1.0 TSI",
                    Registration = "JT 42 ZN",
                    Province = "GP",
                    ExpiryDate = new DateOnly(2026, 9, 28),
                    DaysRemaining = 43,
                    Status = "Due soon",
                    StatusTone = "warning",
                    ReadyDocumentCount = 4,
                    RequiredDocumentCount = 5,
                    PrototypeEstimate = 1262.00m
                },

                new()
                {
                    Id = "hilux",
                    VehicleName = "2022 Toyota Hilux 2.4 GD-6",
                    Registration = "LM 81 TK",
                    Province = "GP",
                    ExpiryDate = new DateOnly(2027, 2, 14),
                    DaysRemaining = 182,
                    Status = "Up to date",
                    StatusTone = "success",
                    ReadyDocumentCount = 5,
                    RequiredDocumentCount = 5,
                    PrototypeEstimate = 1315.00m
                }
            ];
        }
    }
}