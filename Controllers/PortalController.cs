using Microsoft.AspNetCore.Mvc;
using Q4UPrototypeWeb.Models;

namespace Q4UPrototypeWeb.Controllers
{
    public class PortalController : Controller
    {
        public IActionResult Dashboard()
        {
            var model = BuildDashboard();

            return View(model);
        }


        public IActionResult Vehicles()
        {
            return RedirectToAction(
                "Index",
                "Vehicles");
        }


        public IActionResult Renewals()
        {
            return RedirectToAction(
                "Index",
                "Renewals");
        }


        public IActionResult Documents()
        {
            return RedirectToAction(
                "Index",
                "Documents");
        }


        public IActionResult Requests()
        {
            return RedirectToAction(
                "Index",
                "ServiceRequests");
        }


        public IActionResult Payments()
        {
            return RedirectToAction(
                "Index",
                "Payments");
        }

        public IActionResult Notifications()
        {
            return RedirectToAction(
                "Index",
                "Notifications");
        }


        public IActionResult Settings()
        {
            return RedirectToAction(
                "Index",
                "Settings");
        }


        private static PortalDashboardViewModel BuildDashboard()
        {
            return new PortalDashboardViewModel
            {
                FirstName = "Ayesha",
                FullName = "Ayesha Khan",
                CustomerNumber = "Q4U-10482",
                EmailAddress = "ayesha.khan@example.co.za",
                UnreadNotificationCount = 3,

                Vehicles =
                [
                    new()
                    {
                        Id = "polo",
                        Year = 2021,
                        Make = "Volkswagen",
                        Model = "Polo 1.0 TSI",
                        Registration = "JT 42 ZN",
                        Province = "GP",
                        LicenceExpiry = new DateOnly(2026, 9, 28),
                        DaysRemaining = 43,
                        RenewalStatus = "Action required",
                        StatusTone = "warning",
                        DocumentCount = 4,
                        OutstandingBalance = 0,
                        CompletedServices = 2
                    },

                    new()
                    {
                        Id = "hilux",
                        Year = 2022,
                        Make = "Toyota",
                        Model = "Hilux 2.4 GD-6",
                        Registration = "LM 81 TK",
                        Province = "GP",
                        LicenceExpiry = new DateOnly(2027, 2, 14),
                        DaysRemaining = 182,
                        RenewalStatus = "Up to date",
                        StatusTone = "success",
                        DocumentCount = 5,
                        OutstandingBalance = 0,
                        CompletedServices = 3
                    },

                    new()
                    {
                        Id = "bmw",
                        Year = 2020,
                        Make = "BMW",
                        Model = "320i",
                        Registration = "KP 19 FX",
                        Province = "GP",
                        LicenceExpiry = new DateOnly(2026, 8, 22),
                        DaysRemaining = 6,
                        RenewalStatus = "Urgent",
                        StatusTone = "danger",
                        DocumentCount = 2,
                        OutstandingBalance = 1284.50m,
                        CompletedServices = 1
                    }
                ],

                RecentRequests =
                [
                    new()
                    {
                        Reference = "Q4U-260814-018",
                        ServiceName = "Licence Renewal",
                        Registration = "KP 19 FX GP",
                        SubmittedDate = "14 Aug 2026",
                        Status = "Documents received",
                        StatusTone = "info"
                    },

                    new()
                    {
                        Reference = "Q4U-260810-011",
                        ServiceName = "Number Plates",
                        Registration = "JT 42 ZN GP",
                        SubmittedDate = "10 Aug 2026",
                        Status = "Ready for collection",
                        StatusTone = "success"
                    },

                    new()
                    {
                        Reference = "Q4U-260722-006",
                        ServiceName = "Traffic Fine Assistance",
                        Registration = "LM 81 TK GP",
                        SubmittedDate = "22 Jul 2026",
                        Status = "Completed",
                        StatusTone = "success"
                    }
                ],

                RecentDocuments =
                [
                    new()
                    {
                        Name = "Licence Renewal Notice",
                        Category = "Renewal",
                        Registration = "KP 19 FX GP",
                        UploadedDate = "14 Aug 2026",
                        Status = "Review",
                        StatusTone = "warning"
                    },

                    new()
                    {
                        Name = "Vehicle Registration Certificate",
                        Category = "Registration",
                        Registration = "JT 42 ZN GP",
                        UploadedDate = "04 Aug 2026",
                        Status = "Verified",
                        StatusTone = "success"
                    },

                    new()
                    {
                        Name = "Proof of Address",
                        Category = "Customer",
                        Registration = "General",
                        UploadedDate = "02 Aug 2026",
                        Status = "Verified",
                        StatusTone = "success"
                    }
                ]
            };
        }
    }
}