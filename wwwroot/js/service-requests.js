"use strict";

const q4uServiceRequestStorageKey =
    "q4uPrototypeServiceRequest";


const serviceConfiguration = {

    plates: {
        name: "Number Plates",
        icon: "NP",
        description:
            "Request replacement or new number plates for the selected vehicle.",
        estimate: "From R420"
    },

    personalised: {
        name: "Personalised Plates",
        icon: "★",
        description:
            "Capture a preferred personalised registration and request assistance.",
        estimate: "From R850"
    },

    roadworthy: {
        name: "Roadworthy Assistance",
        icon: "✓",
        description:
            "Request assistance preparing and coordinating a roadworthy process.",
        estimate: "From R650"
    },

    fines: {
        name: "Traffic Fine Assistance",
        icon: "!",
        description:
            "Capture traffic-fine information and request Q4U administrative assistance.",
        estimate: "From R350"
    },

    fleet: {
        name: "Fleet Services",
        icon: "F",
        description:
            "Request a Q4U consultation for multi-vehicle administration.",
        estimate: "Custom"
    }
};


document.addEventListener(
    "DOMContentLoaded",
    () => {

        initialiseServiceCatalogue();
        injectSessionServiceRequest();

        initialiseServiceWizard();

        initialiseServiceTracking();
    }
);


/* ============================================================
   CATALOGUE
   ============================================================ */

function initialiseServiceCatalogue() {

    const search =
        document.getElementById(
            "serviceCatalogueSearch"
        );

    const cards =
        Array.from(
            document.querySelectorAll(
                "[data-service-catalogue-card]"
            )
        );

    const empty =
        document.getElementById(
            "serviceCatalogueEmpty"
        );


    search?.addEventListener(
        "input",
        () => {

            const query =
                search.value
                    .trim()
                    .toLowerCase();

            let visible = 0;


            cards.forEach(card => {

                const show =
                    !query ||
                    card.dataset.search
                        ?.includes(query);

                card.style.display =
                    show
                        ? ""
                        : "none";

                if (show) {
                    visible++;
                }
            });


            empty?.classList.toggle(
                "d-none",
                visible > 0
            );
        }
    );


    const requestFilter =
        document.getElementById(
            "serviceRequestStatusFilter"
        );


    requestFilter?.addEventListener(
        "change",
        () => {

            document
                .querySelectorAll(
                    "[data-service-request]"
                )
                .forEach(request => {

                    request.style.display =
                        requestFilter.value ===
                            "all" ||
                            request.dataset.status ===
                            requestFilter.value
                            ? ""
                            : "none";
                });
        }
    );
}


/* ============================================================
   SESSION REQUEST
   ============================================================ */

function injectSessionServiceRequest() {

    const stored =
        sessionStorage.getItem(
            q4uServiceRequestStorageKey
        );

    const list =
        document.getElementById(
            "serviceRequestList"
        );

    const template =
        document.getElementById(
            "sessionServiceRequestTemplate"
        );


    if (
        !stored ||
        !list ||
        !template ||
        list.querySelector(
            ".session-service-request"
        )
    ) {
        return;
    }


    let request;


    try {
        request =
            JSON.parse(stored);
    }
    catch {
        return;
    }


    const fragment =
        template.content.cloneNode(
            true
        );


    setNodeText(
        fragment.querySelector(
            "[data-session-service-reference]"
        ),
        request.reference
    );


    setNodeText(
        fragment.querySelector(
            "[data-session-service-name]"
        ),
        request.serviceName
    );


    setNodeText(
        fragment.querySelector(
            "[data-session-service-vehicle]"
        ),
        `${request.vehicleName} · ${request.registration} ${request.province}`
    );


    const link =
        fragment.querySelector(
            "[data-session-service-link]"
        );


    if (link) {

        link.href =
            `/ServiceRequests/Track?reference=${encodeURIComponent(request.reference)}&newRequest=true`;
    }


    list.prepend(
        fragment
    );


    const count =
        document.getElementById(
            "serviceActiveCount"
        );


    if (count) {

        count.textContent =
            String(
                Number(
                    count.textContent || "0"
                ) + 1
            );
    }
}


/* ============================================================
   WIZARD
   ============================================================ */

function initialiseServiceWizard() {

    const steps =
        document.querySelectorAll(
            "[data-service-step]"
        );


    if (!steps.length) {
        return;
    }


    let currentStep = 1;

    let selectedService =
        window.q4uInitialService ||
        "plates";


    if (!serviceConfiguration[
        selectedService
    ]) {
        selectedService = "plates";
    }


    const showStep = step => {

        currentStep = step;


        steps.forEach(panel => {

            panel.classList.toggle(
                "d-none",
                Number(
                    panel.dataset.serviceStep
                ) !== step
            );
        });


        const indicators =
            document.querySelectorAll(
                "[data-service-indicator]"
            );


        indicators.forEach(
            indicator => {

                const number =
                    Number(
                        indicator.dataset
                            .serviceIndicator
                    );


                indicator.classList.toggle(
                    "active",
                    number === step
                );


                indicator.classList.toggle(
                    "complete",
                    number < step
                );


                const marker =
                    indicator.querySelector(
                        ":scope > span"
                    );


                if (marker) {

                    marker.textContent =
                        number < step
                            ? "✓"
                            : number;
                }
            }
        );


        document
            .querySelectorAll(
                ".q4u-service-wizard-progress > i"
            )
            .forEach(
                (line, index) => {

                    line.classList.toggle(
                        "complete",
                        index + 1 < step
                    );
                }
            );


        if (step === 4) {
            populateServiceReview(
                selectedService
            );
        }


        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    };


    initialiseServiceChoices(
        value => {

            selectedService = value;

            updateServiceUI(
                selectedService
            );
        }
    );


    initialiseServiceVehicle();
    initialisePersonalisedPlate();
    initialiseServiceSupportingFile();


    document
        .querySelectorAll(
            "[data-service-next]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    if (currentStep < 4) {

                        showStep(
                            currentStep + 1
                        );
                    }
                }
            );
        });


    document
        .querySelectorAll(
            "[data-service-previous]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    if (currentStep > 1) {

                        showStep(
                            currentStep - 1
                        );
                    }
                }
            );
        });


    const consent =
        document.getElementById(
            "serviceRequestConsent"
        );

    const submit =
        document.getElementById(
            "submitServiceRequest"
        );


    consent?.addEventListener(
        "change",
        () => {

            if (submit) {

                submit.disabled =
                    !consent.checked;
            }
        }
    );


    submit?.addEventListener(
        "click",
        () => {

            completeServiceRequest(
                selectedService
            );
        }
    );


    document
        .getElementById(
            "copyServiceReference"
        )
        ?.addEventListener(
            "click",
            async () => {

                try {

                    await navigator
                        .clipboard
                        .writeText(
                            "Q4U-260816-029"
                        );


                    showPortalToast(
                        "Reference copied",
                        "Q4U-260816-029 copied to clipboard."
                    );
                }
                catch {

                    showPortalToast(
                        "Service reference",
                        "Q4U-260816-029"
                    );
                }
            }
        );


    updateServiceUI(
        selectedService
    );

    updateVehicleDisplay();
}


/* ============================================================
   SERVICE CHOICE
   ============================================================ */

function initialiseServiceChoices(
    onSelected) {

    document
        .querySelectorAll(
            "[data-service-choice]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    document
                        .querySelectorAll(
                            "[data-service-choice]"
                        )
                        .forEach(item =>
                            item.classList.remove(
                                "selected"
                            )
                        );


                    button.classList.add(
                        "selected"
                    );


                    onSelected?.(
                        button.dataset
                            .serviceChoice
                    );
                }
            );
        });
}


function updateServiceUI(serviceId) {

    const config =
        serviceConfiguration[
        serviceId
        ];


    if (!config) {
        return;
    }


    setText(
        "selectedServiceName",
        config.name
    );

    setText(
        "selectedServiceDescription",
        config.description
    );

    setText(
        "selectedServiceEstimate",
        config.estimate
    );


    setText(
        "serviceSideName",
        config.name
    );

    setText(
        "serviceSideEstimate",
        config.estimate
    );


    document
        .querySelectorAll(
            "[data-service-panel]"
        )
        .forEach(panel => {

            panel.classList.toggle(
                "d-none",
                panel.dataset.servicePanel !==
                serviceId
            );
        });
}


/* ============================================================
   VEHICLE
   ============================================================ */

function initialiseServiceVehicle() {

    document
        .getElementById(
            "serviceVehicleSelector"
        )
        ?.addEventListener(
            "change",
            updateVehicleDisplay
        );
}


function updateVehicleDisplay() {

    const selector =
        document.getElementById(
            "serviceVehicleSelector"
        );


    const option =
        selector?.options[
        selector.selectedIndex
        ];


    if (!option) {
        return;
    }


    setText(
        "serviceVehicleName",
        option.dataset.name
    );

    setText(
        "serviceVehicleColour",
        option.dataset.colour
    );

    setText(
        "serviceVehicleRegistration",
        option.dataset.registration
    );

    setText(
        "serviceVehicleProvince",
        option.dataset.province
    );


    setText(
        "serviceSideVehicle",
        shortVehicleName(
            option.dataset.name
        )
    );


    setText(
        "serviceSideRegistration",
        `${option.dataset.registration} ${option.dataset.province}`
    );
}


/* ============================================================
   PERSONALISED PLATE
   ============================================================ */

function initialisePersonalisedPlate() {

    const input =
        document.getElementById(
            "personalisedPlateText"
        );


    input?.addEventListener(
        "input",
        () => {

            setText(
                "personalisedPlatePreview",
                input.value
                    .trim()
                    .toUpperCase() ||
                "YOUR PLATE"
            );


            document
                .getElementById(
                    "personalisedPlateResult"
                )
                ?.classList.add(
                    "d-none"
                );
        }
    );


    document
        .getElementById(
            "checkPersonalisedPlate"
        )
        ?.addEventListener(
            "click",
            () => {

                const text =
                    input?.value
                        .trim()
                        .toUpperCase();


                if (!text) {

                    input?.focus();

                    showPortalToast(
                        "Plate text required",
                        "Enter a preferred personalised plate first."
                    );

                    return;
                }


                document
                    .getElementById(
                        "personalisedPlateResult"
                    )
                    ?.classList.remove(
                        "d-none"
                    );


                showPortalToast(
                    "Prototype result",
                    `${text} is shown as available for this demonstration.`
                );
            }
        );
}


/* ============================================================
   SUPPORTING FILE
   ============================================================ */

function initialiseServiceSupportingFile() {

    const input =
        document.getElementById(
            "serviceSupportingFileInput"
        );


    document
        .getElementById(
            "selectServiceSupportingFile"
        )
        ?.addEventListener(
            "click",
            () => input?.click()
        );


    input?.addEventListener(
        "change",
        () => {

            const file =
                input.files?.[0];


            if (!file) {
                return;
            }


            setText(
                "serviceSelectedFileType",
                getFileType(file)
            );


            setText(
                "serviceSelectedFileName",
                file.name
            );


            document
                .getElementById(
                    "serviceSelectedFile"
                )
                ?.classList.remove(
                    "d-none"
                );


            showPortalToast(
                "Supporting file selected",
                `${file.name} has been selected locally.`
            );
        }
    );


    document
        .getElementById(
            "removeServiceSupportingFile"
        )
        ?.addEventListener(
            "click",
            () => {

                if (input) {
                    input.value = "";
                }


                document
                    .getElementById(
                        "serviceSelectedFile"
                    )
                    ?.classList.add(
                        "d-none"
                    );
            }
        );
}


/* ============================================================
   REVIEW
   ============================================================ */

function populateServiceReview(
    serviceId) {

    const config =
        serviceConfiguration[
        serviceId
        ];


    const selector =
        document.getElementById(
            "serviceVehicleSelector"
        );


    const vehicle =
        selector?.options[
        selector.selectedIndex
        ];


    if (!config || !vehicle) {
        return;
    }


    setText(
        "serviceReviewIcon",
        config.icon
    );


    setText(
        "serviceReviewName",
        config.name
    );


    setText(
        "serviceReviewEstimate",
        config.estimate
    );


    setText(
        "serviceReviewEstimateSecondary",
        config.estimate
    );


    setText(
        "serviceReviewDescription",
        buildServiceDetailSummary(
            serviceId
        )
    );


    setText(
        "serviceReviewVehicle",
        vehicle.dataset.name
    );


    setText(
        "serviceReviewColour",
        vehicle.dataset.colour
    );


    setText(
        "serviceReviewRegistration",
        vehicle.dataset.registration
    );


    setText(
        "serviceReviewProvince",
        vehicle.dataset.province
    );


    const contact =
        document.querySelector(
            'input[name="serviceContactPreference"]:checked'
        );


    const fulfilment =
        document.querySelector(
            'input[name="serviceFulfilment"]:checked'
        );


    const file =
        document.getElementById(
            "serviceSupportingFileInput"
        )?.files?.[0];


    setText(
        "serviceReviewContact",
        contact?.value ??
        "SMS"
    );


    setText(
        "serviceReviewFulfilment",
        fulfilment?.value ??
        "Collect at Q4U Mayfair"
    );


    setText(
        "serviceReviewFile",
        file?.name ??
        "None"
    );
}


function buildServiceDetailSummary(
    serviceId) {

    if (serviceId === "plates") {

        const reason =
            document.getElementById(
                "plateRequestReason"
            )?.value ??
            "Replacement plate";


        const quantity =
            document.getElementById(
                "plateQuantity"
            )?.selectedOptions?.[0]
                ?.textContent
                ?.trim() ??
            "Front and rear";


        return `${reason} · ${quantity}`;
    }


    if (serviceId === "personalised") {

        const text =
            document.getElementById(
                "personalisedPlateText"
            )?.value
                .trim()
                .toUpperCase() ??
            "Personalised plate";


        return `Preferred registration: ${text}`;
    }


    if (serviceId === "roadworthy") {

        const date =
            document.getElementById(
                "roadworthyDate"
            )?.value ??
            "Date to confirm";


        const period =
            document.getElementById(
                "roadworthyTime"
            )?.value ??
            "Preferred period";


        return `${date} · ${period}`;
    }


    if (serviceId === "fines") {

        return `Reference: ${document.getElementById(
            "fineReference"
        )?.value ||
            "Not supplied"
            }`;
    }


    if (serviceId === "fleet") {

        return `${document.getElementById(
            "fleetVehicleCount"
        )?.value ??
            "Fleet consultation"
            }`;
    }


    return "";
}


/* ============================================================
   COMPLETE REQUEST
   ============================================================ */

function completeServiceRequest(
    serviceId) {

    const config =
        serviceConfiguration[
        serviceId
        ];


    const selector =
        document.getElementById(
            "serviceVehicleSelector"
        );


    const vehicle =
        selector?.options[
        selector.selectedIndex
        ];


    const contact =
        document.querySelector(
            'input[name="serviceContactPreference"]:checked'
        )?.value ??
        "SMS";


    if (!config || !vehicle) {
        return;
    }


    const request = {

        reference:
            "Q4U-260816-029",

        serviceId:
            serviceId,

        serviceName:
            config.name,

        vehicleId:
            vehicle.value,

        vehicleName:
            vehicle.dataset.name,

        registration:
            vehicle.dataset.registration,

        province:
            vehicle.dataset.province,

        contactPreference:
            contact,

        estimate:
            config.estimate,

        submitted:
            new Date().toISOString()
    };


    sessionStorage.setItem(
        q4uServiceRequestStorageKey,
        JSON.stringify(
            request
        )
    );


    document
        .getElementById(
            "serviceRequestReview"
        )
        ?.classList.add(
            "d-none"
        );


    document
        .getElementById(
            "serviceRequestSuccess"
        )
        ?.classList.remove(
            "d-none"
        );


    setText(
        "serviceSuccessMessage",
        `Q4U: Your ${config.name} request Q4U-260816-029 has been received.`
    );


    showPortalToast(
        "Service request submitted",
        `${config.name} request Q4U-260816-029 has been created.`
    );
}


/* ============================================================
   TRACKING
   ============================================================ */

function initialiseServiceTracking() {

    document
        .querySelectorAll(
            "[data-service-notification]"
        )
        .forEach(toggle => {

            toggle.addEventListener(
                "change",
                () => {

                    const type =
                        toggle.dataset
                            .serviceNotification;


                    showPortalToast(
                        `${type} updates ${toggle.checked ? "enabled" : "disabled"}`,
                        `${type} notifications have been updated for this prototype request.`
                    );
                }
            );
        });


    document
        .getElementById(
            "confirmCancelService"
        )
        ?.addEventListener(
            "click",
            () => {

                const modal =
                    document.getElementById(
                        "cancelServiceModal"
                    );


                bootstrap.Modal
                    .getInstance(
                        modal
                    )
                    ?.hide();


                setText(
                    "serviceTrackCurrentStatus",
                    "Cancellation requested"
                );


                const badge =
                    document.getElementById(
                        "serviceTrackStatusBadge"
                    );


                if (badge) {

                    badge.textContent =
                        "Action requested";

                    badge.classList.remove(
                        "success",
                        "info"
                    );

                    badge.classList.add(
                        "warning"
                    );
                }


                showPortalToast(
                    "Cancellation requested",
                    "The prototype request now shows a cancellation state."
                );
            }
        );
}


/* ============================================================
   HELPERS
   ============================================================ */

function shortVehicleName(name) {

    if (!name) {
        return "";
    }


    return name
        .replace(
            /^\d{4}\s/,
            ""
        );
}


function getFileType(file) {

    return file?.name
        ?.split(".")
        ?.pop()
        ?.toUpperCase() ??
        "FILE";
}


function setText(id, value) {

    const element =
        document.getElementById(id);


    if (element) {

        element.textContent =
            value ?? "";
    }
}


function setNodeText(
    element,
    value) {

    if (element) {

        element.textContent =
            value ?? "";
    }
}