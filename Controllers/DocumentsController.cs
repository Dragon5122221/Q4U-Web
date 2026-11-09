using Microsoft.AspNetCore.Mvc;
using Q4UPrototypeWeb.Models;

namespace Q4UPrototypeWeb.Controllers
{
    public class DocumentsController : Controller
    {
        public IActionResult Index()
        {
            var model = new DocumentLibraryViewModel
            {
                Documents =
                [
                    new()
                    {
                        Id = "polo-registration",
                        Name = "Vehicle Registration Certificate",
                        Category = "Registration",
                        FileType = "PDF",
                        FileSize = "1.2 MB",
                        UploadedDate = "04 August 2026",
                        Status = "Verified",
                        StatusTone = "success",
                        VehicleId = "polo",
                        VehicleName = "2021 Volkswagen Polo 1.0 TSI",
                        Registration = "JT 42 ZN GP",
                        Description =
                            "Vehicle registration certificate currently associated with the Volkswagen Polo.",
                        RenewalRelevant = true
                    },

                    new()
                    {
                        Id = "proof-address",
                        Name = "Proof of Address",
                        Category = "Customer",
                        FileType = "PDF",
                        FileSize = "684 KB",
                        UploadedDate = "02 August 2026",
                        Status = "Verified",
                        StatusTone = "success",
                        Description =
                            "Customer proof of address stored at profile level.",
                        RenewalRelevant = true
                    },

                    new()
                    {
                        Id = "owner-id",
                        Name = "Owner ID Copy",
                        Category = "Identity",
                        FileType = "PDF",
                        FileSize = "912 KB",
                        UploadedDate = "03 August 2026",
                        Status = "Verified",
                        StatusTone = "success",
                        Description =
                            "Customer identity document used for vehicle administration preparation.",
                        RenewalRelevant = true
                    },

                    new()
                    {
                        Id = "bmw-renewal-notice",
                        Name = "Licence Renewal Notice",
                        Category = "Renewal",
                        FileType = "PDF",
                        FileSize = "428 KB",
                        UploadedDate = "14 August 2026",
                        Status = "Review",
                        StatusTone = "warning",
                        VehicleId = "bmw",
                        VehicleName = "2020 BMW 320i",
                        Registration = "KP 19 FX GP",
                        Description =
                            "Renewal notice uploaded for the upcoming BMW licence renewal.",
                        RenewalRelevant = true
                    },

                    new()
                    {
                        Id = "bmw-registration",
                        Name = "Vehicle Registration Certificate",
                        Category = "Registration",
                        FileType = "PDF",
                        FileSize = "1.0 MB",
                        UploadedDate = "11 November 2020",
                        Status = "Verified",
                        StatusTone = "success",
                        VehicleId = "bmw",
                        VehicleName = "2020 BMW 320i",
                        Registration = "KP 19 FX GP",
                        Description =
                            "Vehicle registration certificate for the BMW.",
                        RenewalRelevant = true
                    },

                    new()
                    {
                        Id = "hilux-registration",
                        Name = "Vehicle Registration Certificate",
                        Category = "Registration",
                        FileType = "PDF",
                        FileSize = "1.4 MB",
                        UploadedDate = "18 March 2022",
                        Status = "Verified",
                        StatusTone = "success",
                        VehicleId = "hilux",
                        VehicleName = "2022 Toyota Hilux 2.4 GD-6",
                        Registration = "LM 81 TK GP",
                        Description =
                            "Vehicle registration certificate for the Toyota Hilux.",
                        RenewalRelevant = true
                    },

                    new()
                    {
                        Id = "polo-old-disc",
                        Name = "Previous Licence Disc",
                        Category = "Licence",
                        FileType = "JPG",
                        FileSize = "2.1 MB",
                        UploadedDate = "01 August 2026",
                        Status = "Verified",
                        StatusTone = "success",
                        VehicleId = "polo",
                        VehicleName = "2021 Volkswagen Polo 1.0 TSI",
                        Registration = "JT 42 ZN GP",
                        Description =
                            "Image of the previous licence disc.",
                        RenewalRelevant = true
                    },

                    new()
                    {
                        Id = "insurance-copy",
                        Name = "Insurance Confirmation",
                        Category = "Other",
                        FileType = "PDF",
                        FileSize = "734 KB",
                        UploadedDate = "29 July 2026",
                        Status = "Stored",
                        StatusTone = "info",
                        VehicleId = "hilux",
                        VehicleName = "2022 Toyota Hilux 2.4 GD-6",
                        Registration = "LM 81 TK GP",
                        Description =
                            "Supporting insurance information retained with the vehicle profile."
                    },

                    new()
                    {
                        Id = "bmw-disc-photo",
                        Name = "Current Licence Disc Image",
                        Category = "Licence",
                        FileType = "JPG",
                        FileSize = "1.8 MB",
                        UploadedDate = "16 August 2026",
                        Status = "Needs review",
                        StatusTone = "warning",
                        VehicleId = "bmw",
                        VehicleName = "2020 BMW 320i",
                        Registration = "KP 19 FX GP",
                        Description =
                            "Current licence disc image awaiting prototype review.",
                        RenewalRelevant = true
                    }
                ]
            };

            return View(model);
        }


        public IActionResult Upload()
        {
            var model = new DocumentUploadViewModel
            {
                Vehicles =
                [
                    new()
                    {
                        Id = "polo",
                        Name = "2021 Volkswagen Polo 1.0 TSI",
                        Registration = "JT 42 ZN",
                        Province = "GP"
                    },

                    new()
                    {
                        Id = "hilux",
                        Name = "2022 Toyota Hilux 2.4 GD-6",
                        Registration = "LM 81 TK",
                        Province = "GP"
                    },

                    new()
                    {
                        Id = "bmw",
                        Name = "2020 BMW 320i",
                        Registration = "KP 19 FX",
                        Province = "GP"
                    }
                ]
            };

            return View(model);
        }
    }
}