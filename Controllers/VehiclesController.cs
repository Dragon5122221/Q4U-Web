using Microsoft.AspNetCore.Mvc;
using Q4UPrototypeWeb.Models;

namespace Q4UPrototypeWeb.Controllers
{
    public class VehiclesController : Controller
    {
        public IActionResult Index()
        {
            var model = new VehicleLibraryViewModel
            {
                Vehicles = BuildVehicles()
            };

            return View(model);
        }


        public IActionResult Details(string id = "polo")
        {
            var vehicles = BuildVehicles();

            var vehicle =
                vehicles.FirstOrDefault(v =>
                    v.Id.Equals(
                        id,
                        StringComparison.OrdinalIgnoreCase))
                ?? vehicles.First();

            var model = BuildVehicleDetails(vehicle);

            return View(model);
        }


        public IActionResult Add()
        {
            return View();
        }


        private static List<VehicleSummaryViewModel> BuildVehicles()
        {
            return
            [
                new()
                {
                    Id = "polo",
                    Year = 2021,
                    Make = "Volkswagen",
                    Model = "Polo 1.0 TSI Comfortline",
                    Registration = "JT 42 ZN",
                    Province = "GP",
                    Colour = "Pure White",
                    VinLastSix = "672914",
                    LicenceExpiry = new DateOnly(2026, 9, 28),
                    DaysRemaining = 43,
                    RenewalStatus = "Renewal due soon",
                    StatusTone = "warning",
                    DocumentCount = 4,
                    ServiceCount = 2,
                    OutstandingBalance = 0m
                },

                new()
                {
                    Id = "hilux",
                    Year = 2022,
                    Make = "Toyota",
                    Model = "Hilux 2.4 GD-6 Raider",
                    Registration = "LM 81 TK",
                    Province = "GP",
                    Colour = "Graphite Grey",
                    VinLastSix = "418327",
                    LicenceExpiry = new DateOnly(2027, 2, 14),
                    DaysRemaining = 182,
                    RenewalStatus = "Up to date",
                    StatusTone = "success",
                    DocumentCount = 5,
                    ServiceCount = 3,
                    OutstandingBalance = 0m
                },

                new()
                {
                    Id = "bmw",
                    Year = 2020,
                    Make = "BMW",
                    Model = "320i Sport Line",
                    Registration = "KP 19 FX",
                    Province = "GP",
                    Colour = "Mineral Grey",
                    VinLastSix = "930641",
                    LicenceExpiry = new DateOnly(2026, 8, 22),
                    DaysRemaining = 6,
                    RenewalStatus = "Urgent renewal",
                    StatusTone = "danger",
                    DocumentCount = 2,
                    ServiceCount = 1,
                    OutstandingBalance = 1284.50m
                }
            ];
        }


        private static VehicleDetailsViewModel BuildVehicleDetails(
            VehicleSummaryViewModel vehicle)
        {
            if (vehicle.Id == "hilux")
            {
                return new VehicleDetailsViewModel
                {
                    Vehicle = vehicle,
                    EngineNumber = "2GD-FTV-7429183",
                    FullVin = "AHTBB3CD906418327",
                    BodyType = "Double Cab",
                    FuelType = "Diesel",
                    Transmission = "6-Speed Manual",
                    LicenceDiscNumber = "GP-LD-228419",
                    RegistrationDate = "18 March 2022",
                    Odometer = "68,420 km",

                    Documents =
                    [
                        new()
                        {
                            Name = "Vehicle Registration Certificate",
                            Category = "Registration",
                            UploadedDate = "18 Mar 2022",
                            Status = "Verified"
                        },
                        new()
                        {
                            Name = "Owner ID Copy",
                            Category = "Identity",
                            UploadedDate = "03 Aug 2026",
                            Status = "Verified"
                        },
                        new()
                        {
                            Name = "Proof of Address",
                            Category = "Customer",
                            UploadedDate = "02 Aug 2026",
                            Status = "Verified"
                        }
                    ],

                    ServiceHistory =
                    [
                        new()
                        {
                            Reference = "Q4U-260722-006",
                            Service = "Traffic Fine Assistance",
                            Date = "24 Jul 2026",
                            Amount = "R350.00",
                            Status = "Completed"
                        },
                        new()
                        {
                            Reference = "Q4U-250208-031",
                            Service = "Licence Renewal",
                            Date = "08 Feb 2025",
                            Amount = "R1,106.00",
                            Status = "Completed"
                        }
                    ],

                    Activity =
                    [
                        new()
                        {
                            Title = "Vehicle record reviewed",
                            Description = "Vehicle information confirmed as current.",
                            Date = "14 Aug 2026",
                            Tone = "success"
                        },
                        new()
                        {
                            Title = "Proof of address verified",
                            Description = "Customer document marked as verified.",
                            Date = "02 Aug 2026",
                            Tone = "success"
                        }
                    ]
                };
            }


            if (vehicle.Id == "bmw")
            {
                return new VehicleDetailsViewModel
                {
                    Vehicle = vehicle,
                    EngineNumber = "B48B20M0-390641",
                    FullVin = "WBA5R12070FK930641",
                    BodyType = "Sedan",
                    FuelType = "Petrol",
                    Transmission = "8-Speed Automatic",
                    LicenceDiscNumber = "GP-LD-771945",
                    RegistrationDate = "11 November 2020",
                    Odometer = "91,860 km",

                    Documents =
                    [
                        new()
                        {
                            Name = "Vehicle Registration Certificate",
                            Category = "Registration",
                            UploadedDate = "11 Nov 2020",
                            Status = "Verified"
                        },
                        new()
                        {
                            Name = "Licence Renewal Notice",
                            Category = "Renewal",
                            UploadedDate = "14 Aug 2026",
                            Status = "Review",
                            StatusTone = "warning"
                        }
                    ],

                    ServiceHistory =
                    [
                        new()
                        {
                            Reference = "Q4U-260814-018",
                            Service = "Licence Renewal",
                            Date = "14 Aug 2026",
                            Amount = "R1,284.50",
                            Status = "In progress",
                            StatusTone = "info"
                        }
                    ],

                    Activity =
                    [
                        new()
                        {
                            Title = "Renewal notice uploaded",
                            Description = "A new licence renewal notice was added.",
                            Date = "14 Aug 2026",
                            Tone = "warning"
                        },
                        new()
                        {
                            Title = "Urgent reminder generated",
                            Description = "Licence expiry is now less than seven days away.",
                            Date = "16 Aug 2026",
                            Tone = "danger"
                        }
                    ]
                };
            }


            return new VehicleDetailsViewModel
            {
                Vehicle = vehicle,
                EngineNumber = "DKLA-521672914",
                FullVin = "WVWZZZAWZMU672914",
                BodyType = "Hatchback",
                FuelType = "Petrol",
                Transmission = "5-Speed Manual",
                LicenceDiscNumber = "GP-LD-648215",
                RegistrationDate = "07 October 2021",
                Odometer = "54,280 km",

                Documents =
                [
                    new()
                    {
                        Name = "Vehicle Registration Certificate",
                        Category = "Registration",
                        UploadedDate = "04 Aug 2026",
                        Status = "Verified"
                    },
                    new()
                    {
                        Name = "Owner ID Copy",
                        Category = "Identity",
                        UploadedDate = "03 Aug 2026",
                        Status = "Verified"
                    },
                    new()
                    {
                        Name = "Proof of Address",
                        Category = "Customer",
                        UploadedDate = "02 Aug 2026",
                        Status = "Verified"
                    },
                    new()
                    {
                        Name = "Previous Licence Disc",
                        Category = "Licence",
                        UploadedDate = "01 Aug 2026",
                        Status = "Verified"
                    }
                ],

                ServiceHistory =
                [
                    new()
                    {
                        Reference = "Q4U-260810-011",
                        Service = "Number Plates",
                        Date = "10 Aug 2026",
                        Amount = "R420.00",
                        Status = "Ready for collection"
                    },
                    new()
                    {
                        Reference = "Q4U-250919-024",
                        Service = "Licence Renewal",
                        Date = "19 Sep 2025",
                        Amount = "R1,078.00",
                        Status = "Completed"
                    }
                ],

                Activity =
                [
                    new()
                    {
                        Title = "Number plates ready",
                        Description = "Replacement plates are ready for collection.",
                        Date = "15 Aug 2026",
                        Tone = "success"
                    },
                    new()
                    {
                        Title = "Renewal reminder scheduled",
                        Description = "Licence renewal reminder enabled for September.",
                        Date = "12 Aug 2026",
                        Tone = "info"
                    },
                    new()
                    {
                        Title = "Registration certificate verified",
                        Description = "The uploaded document passed prototype review.",
                        Date = "04 Aug 2026",
                        Tone = "success"
                    }
                ]
            };
        }
    }
}