using Microsoft.AspNetCore.Mvc;
using Q4UPrototypeWeb.Models;

namespace Q4UPrototypeWeb.Controllers
{
    public class PaymentsController : Controller
    {
        public IActionResult Index()
        {
            var model = new PaymentOverviewViewModel
            {
                Charges = BuildCharges()
            };

            return View(model);
        }


        public IActionResult Pay(string id = "bmw-renewal")
        {
            var charge =
                BuildCharges()
                    .FirstOrDefault(c =>
                        c.Id.Equals(
                            id,
                            StringComparison.OrdinalIgnoreCase))
                ?? BuildCharges().First();


            if (!charge.IsOutstanding)
            {
                return RedirectToAction(
                    "Receipt",
                    new
                    {
                        reference = charge.Reference
                    });
            }


            var model = new PaymentStartViewModel
            {
                Charge = charge,

                ServiceAmount =
                    charge.Id == "bmw-renewal"
                        ? 934.50m
                        : charge.Amount,

                HandlingAmount =
                    charge.Id == "bmw-renewal"
                        ? 350.00m
                        : 0m
            };

            return View(model);
        }


        public IActionResult Receipt(
            string reference = "Q4U-PAY-260810-011",
            bool newPayment = false)
        {
            if (newPayment)
            {
                return View(
                    new PaymentReceiptViewModel
                    {
                        PaymentReference = reference,
                        ServiceReference = "Q4U-260814-018",
                        Description = "BMW Licence Renewal",
                        VehicleName = "2020 BMW 320i Sport Line",
                        Registration = "KP 19 FX GP",
                        Amount = 1284.50m,
                        PaymentMethod = "Card",
                        PaymentDate = "16 August 2026 · 17:50",
                        Status = "Paid",
                        IsNewPayment = true
                    });
            }


            return View(
                new PaymentReceiptViewModel
                {
                    PaymentReference = reference,
                    ServiceReference = "Q4U-260810-011",
                    Description = "Number Plates",
                    VehicleName = "2021 Volkswagen Polo 1.0 TSI",
                    Registration = "JT 42 ZN GP",
                    Amount = 420.00m,
                    PaymentMethod = "Card",
                    PaymentDate = "10 August 2026 · 10:12",
                    Status = "Paid"
                });
        }


        private static List<PaymentChargeViewModel> BuildCharges()
        {
            return
            [
                new()
                {
                    Id = "bmw-renewal",
                    Reference = "Q4U-260814-018",
                    Description = "Licence Renewal",
                    Category = "Renewal",
                    VehicleName = "2020 BMW 320i Sport Line",
                    Registration = "KP 19 FX GP",
                    Amount = 1284.50m,
                    Date = "14 Aug 2026",
                    Status = "Payment due",
                    StatusTone = "warning",
                    SourceType = "renewal",
                    SourceReference = "Q4U-260814-018"
                },

                new()
                {
                    Id = "polo-plates",
                    Reference = "Q4U-260810-011",
                    Description = "Number Plates",
                    Category = "Service",
                    VehicleName = "2021 Volkswagen Polo 1.0 TSI",
                    Registration = "JT 42 ZN GP",
                    Amount = 420.00m,
                    Date = "10 Aug 2026",
                    Status = "Paid",
                    StatusTone = "success",
                    SourceType = "service",
                    SourceReference = "Q4U-260810-011"
                },

                new()
                {
                    Id = "hilux-roadworthy",
                    Reference = "Q4U-260812-015",
                    Description = "Roadworthy Assistance",
                    Category = "Service",
                    VehicleName = "2022 Toyota Hilux 2.4 GD-6",
                    Registration = "LM 81 TK GP",
                    Amount = 650.00m,
                    Date = "12 Aug 2026",
                    Status = "Paid",
                    StatusTone = "success",
                    SourceType = "service",
                    SourceReference = "Q4U-260812-015"
                },

                new()
                {
                    Id = "hilux-fines",
                    Reference = "Q4U-260722-006",
                    Description = "Traffic Fine Assistance",
                    Category = "Service",
                    VehicleName = "2022 Toyota Hilux 2.4 GD-6",
                    Registration = "LM 81 TK GP",
                    Amount = 350.00m,
                    Date = "22 Jul 2026",
                    Status = "Paid",
                    StatusTone = "success",
                    SourceType = "service",
                    SourceReference = "Q4U-260722-006"
                },

                new()
                {
                    Id = "polo-renewal-2025",
                    Reference = "Q4U-250919-024",
                    Description = "Licence Renewal",
                    Category = "Renewal",
                    VehicleName = "2021 Volkswagen Polo 1.0 TSI",
                    Registration = "JT 42 ZN GP",
                    Amount = 1078.00m,
                    Date = "19 Sep 2025",
                    Status = "Paid",
                    StatusTone = "success",
                    SourceType = "renewal",
                    SourceReference = "Q4U-250919-024"
                }
            ];
        }
    }
}