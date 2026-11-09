"use strict";

const q4uRenewalStorageKey =
    "q4uPrototypeRenewalRequest";


document.addEventListener("DOMContentLoaded", () => {

    initialiseRenewalOverview();
    initialiseRenewalWizard();
    initialiseRenewalTracking();
    injectSessionRenewal();

});


/* ============================================================
   OVERVIEW
   ============================================================ */

function initialiseRenewalOverview() {

    const filter =
        document.getElementById(
            "renewalStatusFilter"
        );

    const vehicles =
        Array.from(
            document.querySelectorAll(
                "[data-renewal-vehicle]"
            )
        );

    if (!filter || !vehicles.length) {
        return;
    }


    filter.addEventListener(
        "change",
        () => {

            vehicles.forEach(vehicle => {

                const visible =
                    filter.value === "all" ||
                    vehicle.dataset.status ===
                    filter.value;

                vehicle.style.display =
                    visible
                        ? "grid"
                        : "none";
            });
        }
    );
}


/* ============================================================
   SESSION RENEWAL
   ============================================================ */

function injectSessionRenewal() {

    const list =
        document.getElementById(
            "activeRenewalList"
        );

    const template =
        document.getElementById(
            "sessionRenewalTemplate"
        );

    const stored =
        sessionStorage.getItem(
            q4uRenewalStorageKey
        );

    if (
        !list ||
        !template ||
        !stored ||
        list.querySelector(
            ".session-renewal"
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
        template.content.cloneNode(true);


    setNodeText(
        fragment.querySelector(
            "[data-session-renewal-reference]"
        ),
        request.reference
    );

    setNodeText(
        fragment.querySelector(
            "[data-session-renewal-name]"
        ),
        request.vehicleName
    );

    setNodeText(
        fragment.querySelector(
            "[data-session-renewal-description]"
        ),
        `${request.registration} ${request.province} · Submitted today`
    );


    const link =
        fragment.querySelector(
            "[data-session-renewal-link]"
        );

    if (link) {

        link.href =
            `/Renewals/Track?reference=${encodeURIComponent(request.reference)}&newRequest=true`;
    }


    list.appendChild(fragment);


    const count =
        document.getElementById(
            "renewalActiveCount"
        );

    if (count) {

        count.textContent =
            String(
                Number(count.textContent || "0") +
                1
            );
    }
}


/* ============================================================
   WIZARD
   ============================================================ */

function initialiseRenewalWizard() {

    const steps =
        document.querySelectorAll(
            "[data-renewal-step]"
        );

    if (!steps.length) {
        return;
    }


    let currentStep = 1;
    let documentReady = false;


    const showStep = step => {

        currentStep = step;


        steps.forEach(panel => {

            panel.classList.toggle(
                "d-none",
                Number(
                    panel.dataset.renewalStep
                ) !== step
            );
        });


        const indicators =
            document.querySelectorAll(
                "[data-renewal-indicator]"
            );


        indicators.forEach(indicator => {

            const number =
                Number(
                    indicator.dataset
                        .renewalIndicator
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
        });


        const connectorLines =
            document.querySelectorAll(
                ".q4u-renewal-wizard-progress > i"
            );

        connectorLines.forEach(
            (line, index) => {

                line.classList.toggle(
                    "complete",
                    index + 1 < step
                );
            }
        );


        if (step === 4) {
            populateRenewalReview();
        }


        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    };


    initialiseRenewalVehicleSelector();
    initialiseRenewalDocumentUpload(
        () => {
            documentReady = true;
        },
        () => {
            documentReady = false;
        }
    );

    initialiseRenewalPaymentOptions();


    document
        .querySelectorAll(
            "[data-renewal-next]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    if (
                        currentStep === 2 &&
                        !documentReady
                    ) {
                        return;
                    }

                    if (currentStep < 4) {
                        showStep(currentStep + 1);
                    }
                }
            );
        });


    document
        .querySelectorAll(
            "[data-renewal-previous]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    if (currentStep > 1) {
                        showStep(currentStep - 1);
                    }
                }
            );
        });


    const consent =
        document.getElementById(
            "renewalConsent"
        );

    const submit =
        document.getElementById(
            "submitRenewalRequest"
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
        completeRenewalRequest
    );


    document
        .getElementById(
            "copyRenewalReference"
        )
        ?.addEventListener(
            "click",
            async () => {

                try {

                    await navigator.clipboard.writeText(
                        "Q4U-260816-021"
                    );

                    showPortalToast(
                        "Reference copied",
                        "Q4U-260816-021 copied to clipboard."
                    );
                }
                catch {

                    showPortalToast(
                        "Reference",
                        "Q4U-260816-021"
                    );
                }
            }
        );
}


/* ============================================================
   VEHICLE SELECTOR
   ============================================================ */

function initialiseRenewalVehicleSelector() {

    const selector =
        document.getElementById(
            "renewalVehicleSelector"
        );

    if (!selector) {
        return;
    }


    const update = () => {

        const option =
            selector.options[
            selector.selectedIndex
            ];

        if (!option) {
            return;
        }


        setText(
            "renewalSelectedName",
            option.dataset.name
        );

        setText(
            "renewalSelectedRegistration",
            option.dataset.registration
        );

        setText(
            "renewalSelectedProvince",
            option.dataset.province
        );

        setText(
            "renewalSelectedExpiry",
            option.dataset.expiry
        );

        setText(
            "renewalSelectedDays",
            `${option.dataset.days} days`
        );

        setText(
            "renewalSelectedEstimate",
            formatCurrencyNumber(
                option.dataset.estimate
            )
        );


        const status =
            document.getElementById(
                "renewalSelectedStatus"
            );

        if (status) {

            status.textContent =
                option.dataset.status;

            status.classList.remove(
                "success",
                "warning",
                "danger",
                "info"
            );

            status.classList.add(
                option.dataset.tone
            );
        }


        const daysElement =
            document.getElementById(
                "renewalSelectedDays"
            );

        daysElement?.classList.remove(
            "success",
            "warning",
            "danger"
        );

        daysElement?.classList.add(
            option.dataset.tone
        );


        updateEligibility(
            option.dataset.tone
        );


        updateQuoteFromVehicle(
            Number(
                option.dataset.estimate
            )
        );
    };


    selector.addEventListener(
        "change",
        update
    );

    update();
}


function updateEligibility(tone) {

    const container =
        document.getElementById(
            "renewalEligibilityMessage"
        );

    const heading =
        document.getElementById(
            "renewalEligibilityHeading"
        );

    const copy =
        document.getElementById(
            "renewalEligibilityCopy"
        );


    if (!container) {
        return;
    }


    container.classList.remove(
        "success",
        "warning",
        "danger"
    );

    container.classList.add(tone);


    if (tone === "danger") {

        heading.textContent =
            "Urgent renewal recommended";

        copy.textContent =
            "This vehicle expires very soon. Preparing the renewal should be prioritised.";
    }
    else if (tone === "warning") {

        heading.textContent =
            "Renewal preparation is available";

        copy.textContent =
            "This vehicle is approaching its renewal window.";
    }
    else {

        heading.textContent =
            "No immediate renewal required";

        copy.textContent =
            "This vehicle is currently up to date. You may still review the renewal preparation journey.";
    }
}


function updateQuoteFromVehicle(totalEstimate) {

    const handlingFee = 350;

    const vehicleFee =
        Math.max(
            0,
            totalEstimate - handlingFee
        );


    setText(
        "renewalVehicleFee",
        formatCurrencyNumber(vehicleFee)
    );

    setText(
        "renewalTotal",
        formatCurrencyNumber(totalEstimate)
    );
}


/* ============================================================
   DOCUMENT UPLOAD
   ============================================================ */

function initialiseRenewalDocumentUpload(
    onReady,
    onRemoved) {

    const input =
        document.getElementById(
            "renewalDocumentInput"
        );

    const addButton =
        document.querySelector(
            "[data-add-renewal-document]"
        );

    const fileCard =
        document.getElementById(
            "renewalDocumentFile"
        );

    const continueButton =
        document.getElementById(
            "renewalDocumentsContinue"
        );

    const missingDocument =
        document.querySelector(
            '[data-document-id="disc"]'
        );


    if (
        !input ||
        !addButton
    ) {
        return;
    }


    addButton.addEventListener(
        "click",
        () => input.click()
    );


    input.addEventListener(
        "change",
        () => {

            const file =
                input.files?.[0];

            if (!file) {
                return;
            }


            setText(
                "renewalDocumentFileName",
                file.name
            );


            fileCard?.classList.remove(
                "d-none"
            );


            if (missingDocument) {

                missingDocument.dataset
                    .documentReady = "true";

                const icon =
                    missingDocument.querySelector(
                        ".q4u-renewal-document-icon"
                    );

                icon?.classList.remove(
                    "warning"
                );

                icon?.classList.add(
                    "success"
                );

                if (icon) {
                    icon.textContent = "✓";
                }


                const status =
                    missingDocument.querySelector(
                        "[data-document-status]"
                    );

                status?.classList.remove(
                    "warning"
                );

                status?.classList.add(
                    "success"
                );

                if (status) {
                    status.textContent =
                        "Added";
                }


                addButton.style.display =
                    "none";
            }


            if (continueButton) {
                continueButton.disabled = false;
            }


            setText(
                "renewalDocumentSummaryHeading",
                "5 of 5 preparation items ready"
            );

            setText(
                "renewalDocumentSummaryCopy",
                "Your prototype renewal checklist is ready to continue."
            );


            onReady?.();


            showPortalToast(
                "Document added",
                "The missing preparation item is now ready."
            );
        }
    );


    document
        .getElementById(
            "removeRenewalDocument"
        )
        ?.addEventListener(
            "click",
            () => {

                input.value = "";

                fileCard?.classList.add(
                    "d-none"
                );


                if (missingDocument) {

                    missingDocument.dataset
                        .documentReady = "false";


                    const icon =
                        missingDocument.querySelector(
                            ".q4u-renewal-document-icon"
                        );

                    icon?.classList.remove(
                        "success"
                    );

                    icon?.classList.add(
                        "warning"
                    );

                    if (icon) {
                        icon.textContent = "!";
                    }


                    const status =
                        missingDocument.querySelector(
                            "[data-document-status]"
                        );

                    status?.classList.remove(
                        "success"
                    );

                    status?.classList.add(
                        "warning"
                    );

                    if (status) {
                        status.textContent =
                            "Required";
                    }


                    addButton.style.display =
                        "";
                }


                if (continueButton) {
                    continueButton.disabled = true;
                }


                setText(
                    "renewalDocumentSummaryHeading",
                    "4 of 5 preparation items ready"
                );

                setText(
                    "renewalDocumentSummaryCopy",
                    "Add the missing licence disc image before continuing."
                );


                onRemoved?.();
            }
        );
}


/* ============================================================
   PAYMENT
   ============================================================ */

function initialiseRenewalPaymentOptions() {

    const paymentInputs =
        document.querySelectorAll(
            'input[name="paymentMethod"]'
        );

    const cardFields =
        document.getElementById(
            "renewalCardFields"
        );


    paymentInputs.forEach(input => {

        input.addEventListener(
            "change",
            () => {

                cardFields?.classList.toggle(
                    "d-none",
                    input.value !== "Card"
                );
            }
        );
    });
}


/* ============================================================
   REVIEW
   ============================================================ */

function populateRenewalReview() {

    const selector =
        document.getElementById(
            "renewalVehicleSelector"
        );

    const option =
        selector?.options[
        selector.selectedIndex
        ];


    if (!option) {
        return;
    }


    setText(
        "renewalReviewVehicle",
        option.dataset.name
    );

    setText(
        "renewalReviewExpiry",
        `Expires ${option.dataset.expiry}`
    );

    setText(
        "renewalReviewRegistration",
        option.dataset.registration
    );

    setText(
        "renewalReviewProvince",
        option.dataset.province
    );

    setText(
        "renewalReviewTotal",
        formatCurrencyNumber(
            option.dataset.estimate
        )
    );


    const collection =
        document.querySelector(
            'input[name="collectionPreference"]:checked'
        );

    const payment =
        document.querySelector(
            'input[name="paymentMethod"]:checked'
        );


    setText(
        "renewalReviewCollection",
        collection?.value ??
        "Collect at Q4U Mayfair"
    );

    setText(
        "renewalReviewPayment",
        payment?.value ??
        "Card"
    );
}


/* ============================================================
   SUBMIT
   ============================================================ */

function completeRenewalRequest() {

    const selector =
        document.getElementById(
            "renewalVehicleSelector"
        );

    const option =
        selector?.options[
        selector.selectedIndex
        ];


    if (!option) {
        return;
    }


    const request = {

        reference:
            "Q4U-260816-021",

        vehicleId:
            option.value,

        vehicleName:
            option.dataset.name,

        registration:
            option.dataset.registration,

        province:
            option.dataset.province,

        amount:
            option.dataset.estimate,

        submitted:
            new Date().toISOString()
    };


    sessionStorage.setItem(
        q4uRenewalStorageKey,
        JSON.stringify(request)
    );


    document
        .getElementById(
            "renewalReviewContent"
        )
        ?.classList.add(
            "d-none"
        );


    document
        .getElementById(
            "renewalSuccess"
        )
        ?.classList.remove(
            "d-none"
        );


    showPortalToast(
        "Renewal submitted",
        `${request.reference} has been created in the prototype.`
    );
}


/* ============================================================
   TRACKING
   ============================================================ */

function initialiseRenewalTracking() {

    document
        .querySelectorAll(
            "[data-track-notification]"
        )
        .forEach(toggle => {

            toggle.addEventListener(
                "change",
                () => {

                    const type =
                        toggle.dataset
                            .trackNotification;

                    showPortalToast(
                        `${type} updates ${toggle.checked ? "enabled" : "disabled"}`,
                        `${type} renewal notifications have been updated for this prototype request.`
                    );
                }
            );
        });
}


/* ============================================================
   HELPERS
   ============================================================ */

function setText(id, value) {

    const element =
        document.getElementById(id);

    if (element) {
        element.textContent =
            value ?? "";
    }
}


function setNodeText(element, value) {

    if (element) {
        element.textContent =
            value ?? "";
    }
}


function formatCurrencyNumber(value) {

    return Number(value ?? 0)
        .toLocaleString(
            "en-ZA",
            {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }
        );
}