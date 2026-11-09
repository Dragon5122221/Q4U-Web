using Microsoft.AspNetCore.Mvc;
using Q4UPrototypeWeb.Models;

namespace Q4UPrototypeWeb.Controllers
{
    public class NotificationsController : Controller
    {
        public IActionResult Index()
        {
            var model = new NotificationCentreViewModel
            {
                Notifications =
                [
                    new()
                    {
                        Id = "bmw-expiry",
                        Type = "renewal",
                        Icon = "R",
                        Eyebrow = "LICENCE REMINDER",
                        Title = "BMW licence expires in 6 days",
                        Message =
                            "Your 2020 BMW 320i · KP 19 FX GP is approaching its licence expiry date.",
                        Timestamp = "Today · 08:15",
                        Priority = "urgent",
                        IsRead = false,
                        ActionText = "Prepare Renewal",
                        Controller = "Renewals",
                        Action = "Start",
                        RouteId = "bmw"
                    },

                    new()
                    {
                        Id = "bmw-documents",
                        Type = "renewal",
                        Icon = "R",
                        Eyebrow = "RENEWAL UPDATE",
                        Title = "Documents received",
                        Message =
                            "Supporting documents for renewal Q4U-260814-018 have been received and are ready for review.",
                        Timestamp = "Today · 11:03",
                        Priority = "normal",
                        IsRead = false,
                        ActionText = "Track Renewal",
                        Controller = "Renewals",
                        Action = "Track",
                        RouteReference = "Q4U-260814-018"
                    },

                    new()
                    {
                        Id = "polo-plates-ready",
                        Type = "service",
                        Icon = "NP",
                        Eyebrow = "SERVICE UPDATE",
                        Title = "Number plates ready for collection",
                        Message =
                            "Your Volkswagen Polo number plate request Q4U-260810-011 is ready for collection.",
                        Timestamp = "Yesterday · 15:16",
                        Priority = "normal",
                        IsRead = false,
                        ActionText = "View Request",
                        Controller = "ServiceRequests",
                        Action = "Track",
                        RouteReference = "Q4U-260810-011"
                    },

                    new()
                    {
                        Id = "hilux-roadworthy",
                        Type = "service",
                        Icon = "✓",
                        Eyebrow = "APPOINTMENT UPDATE",
                        Title = "Roadworthy appointment confirmed",
                        Message =
                            "The preferred appointment for your Toyota Hilux roadworthy assistance request has been confirmed.",
                        Timestamp = "14 Aug · 10:15",
                        Priority = "normal",
                        IsRead = true,
                        ActionText = "View Request",
                        Controller = "ServiceRequests",
                        Action = "Track",
                        RouteReference = "Q4U-260812-015"
                    },

                    new()
                    {
                        Id = "bmw-disc-review",
                        Type = "document",
                        Icon = "DOC",
                        Eyebrow = "DOCUMENT UPDATE",
                        Title = "Licence disc image needs review",
                        Message =
                            "The current licence disc image linked to your BMW is awaiting prototype review.",
                        Timestamp = "Today · 09:42",
                        Priority = "attention",
                        IsRead = false,
                        ActionText = "View Documents",
                        Controller = "Documents",
                        Action = "Index"
                    },

                    new()
                    {
                        Id = "bmw-payment",
                        Type = "payment",
                        Icon = "R",
                        Eyebrow = "PAYMENT",
                        Title = "Payment required for BMW renewal",
                        Message =
                            "A prototype amount of R1,284.50 is outstanding against renewal Q4U-260814-018.",
                        Timestamp = "14 Aug · 09:21",
                        Priority = "attention",
                        IsRead = false,
                        ActionText = "View Payment",
                        Controller = "Payments",
                        Action = "Pay",
                        RouteId = "bmw-renewal"
                    },

                    new()
                    {
                        Id = "proof-address-verified",
                        Type = "document",
                        Icon = "DOC",
                        Eyebrow = "DOCUMENT VERIFIED",
                        Title = "Proof of address verified",
                        Message =
                            "Your customer proof of address is marked as verified in the prototype document library.",
                        Timestamp = "02 Aug · 13:10",
                        Priority = "normal",
                        IsRead = true,
                        ActionText = "View Documents",
                        Controller = "Documents",
                        Action = "Index"
                    },

                    new()
                    {
                        Id = "polo-payment",
                        Type = "payment",
                        Icon = "R",
                        Eyebrow = "PAYMENT RECEIPT",
                        Title = "Payment receipt available",
                        Message =
                            "A receipt is available for your Volkswagen Polo number plate service.",
                        Timestamp = "10 Aug · 10:12",
                        Priority = "normal",
                        IsRead = true,
                        ActionText = "View Receipt",
                        Controller = "Payments",
                        Action = "Receipt",
                        RouteReference = "Q4U-260810-011"
                    }
                ]
            };

            return View(model);
        }
    }
}