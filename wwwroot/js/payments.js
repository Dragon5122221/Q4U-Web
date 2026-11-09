"use strict";

const q4uPaymentStorageKey =
    "q4uPrototypePayment";


document.addEventListener(
    "DOMContentLoaded",
    () => {

        initialisePaymentLibrary();
        initialisePaymentFlow();
        initialiseReceiptActions();
        applySessionPayment();
    }
);


/* ============================================================
   PAYMENT OVERVIEW
   ============================================================ */

function initialisePaymentLibrary() {

    const list =
        document.getElementById(
            "paymentTransactionList"
        );


    if (!list) {
        return;
    }


    const search =
        document.getElementById(
            "paymentSearch"
        );

    const status =
        document.getElementById(
            "paymentStatusFilter"
        );

    const category =
        document.getElementById(
            "paymentCategoryFilter"
        );

    const empty =
        document.getElementById(
            "paymentEmptyState"
        );


    const applyFilters = () => {

        const query =
            search?.value
                .trim()
                .toLowerCase() ?? "";


        let visible = 0;


        list
            .querySelectorAll(
                "[data-payment-record]"
            )
            .forEach(record => {

                const matchesSearch =
                    !query ||
                    (
                        record.dataset.search ??
                        record.textContent
                            .toLowerCase()
                    )
                        .includes(query);


                const matchesStatus =
                    status?.value === "all" ||
                    record.dataset.status ===
                    status?.value;


                const matchesCategory =
                    category?.value === "all" ||
                    record.dataset.category ===
                    category?.value;


                const show =
                    matchesSearch &&
                    matchesStatus &&
                    matchesCategory;


                record.style.display =
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
    };


    search?.addEventListener(
        "input",
        applyFilters
    );

    status?.addEventListener(
        "change",
        applyFilters
    );

    category?.addEventListener(
        "change",
        applyFilters
    );


    document
        .getElementById(
            "resetPaymentFilters"
        )
        ?.addEventListener(
            "click",
            () => {

                if (search) {
                    search.value = "";
                }

                if (status) {
                    status.value = "all";
                }

                if (category) {
                    category.value = "all";
                }

                applyFilters();
            }
        );
}


/* ============================================================
   PAYMENT FLOW
   ============================================================ */

function initialisePaymentFlow() {

    const paymentMethods =
        document.querySelectorAll(
            'input[name="prototypePaymentMethod"]'
        );


    if (!paymentMethods.length) {
        return;
    }


    const updateMethod = () => {

        const selected =
            document.querySelector(
                'input[name="prototypePaymentMethod"]:checked'
            )?.value ??
            "Card";


        document
            .querySelectorAll(
                "[data-payment-method-panel]"
            )
            .forEach(panel => {

                panel.classList.toggle(
                    "d-none",
                    panel.dataset.paymentMethodPanel !==
                    selected
                );
            });
    };


    paymentMethods.forEach(input => {

        input.addEventListener(
            "change",
            updateMethod
        );
    });


    updateMethod();


    const consent =
        document.getElementById(
            "paymentPrototypeConsent"
        );

    const completeButton =
        document.getElementById(
            "completePrototypePayment"
        );


    consent?.addEventListener(
        "change",
        () => {

            if (completeButton) {

                completeButton.disabled =
                    !consent.checked;
            }
        }
    );


    document
        .getElementById(
            "copyPaymentReference"
        )
        ?.addEventListener(
            "click",
            async () => {

                try {

                    await navigator.clipboard
                        .writeText(
                            "Q4U-260814-018"
                        );


                    showPortalToast(
                        "Reference copied",
                        "Q4U-260814-018 copied to clipboard."
                    );
                }
                catch {

                    showPortalToast(
                        "Payment reference",
                        "Q4U-260814-018"
                    );
                }
            }
        );


    completeButton?.addEventListener(
        "click",
        completePrototypePayment
    );
}


function completePrototypePayment() {

    const method =
        document.querySelector(
            'input[name="prototypePaymentMethod"]:checked'
        )?.value ??
        "Card";


    const payment = {

        paymentReference:
            "Q4U-PAY-260816-041",

        serviceReference:
            "Q4U-260814-018",

        service:
            "Licence Renewal",

        vehicle:
            "2020 BMW 320i Sport Line",

        registration:
            "KP 19 FX GP",

        amount:
            1284.50,

        method:
            method,

        date:
            "16 August 2026"
    };


    sessionStorage.setItem(
        q4uPaymentStorageKey,
        JSON.stringify(payment)
    );


    document
        .querySelectorAll(
            ".q4u-payment-charge-card, .q4u-payment-method-card, .q4u-payment-confirmation-card"
        )
        .forEach(element => {

            element.classList.add(
                "d-none"
            );
        });


    document
        .getElementById(
            "paymentSuccess"
        )
        ?.classList.remove(
            "d-none"
        );


    showPortalToast(
        "Payment complete",
        `${payment.paymentReference} has been recorded for this prototype session.`
    );
}


/* ============================================================
   SESSION PAYMENT STATE
   ============================================================ */

function applySessionPayment() {

    const stored =
        sessionStorage.getItem(
            q4uPaymentStorageKey
        );


    if (!stored) {
        return;
    }


    let payment;


    try {
        payment =
            JSON.parse(stored);
    }
    catch {
        return;
    }


    const list =
        document.getElementById(
            "paymentTransactionList"
        );


    if (!list) {
        return;
    }


    const existing =
        list.querySelector(
            '[data-id="bmw-renewal"]'
        );


    existing?.remove();


    const template =
        document.getElementById(
            "sessionPaymentTemplate"
        );


    if (
        template &&
        !list.querySelector(
            ".q4u-session-payment"
        )
    ) {

        const fragment =
            template.content.cloneNode(
                true
            );


        const record =
            fragment.querySelector(
                "[data-payment-record]"
            );


        if (record) {

            record.dataset.search =
                `${payment.serviceReference} ${payment.service} ${payment.vehicle} ${payment.registration}`
                    .toLowerCase();
        }


        list.prepend(
            fragment
        );
    }


    document
        .getElementById(
            "outstandingPaymentBanner"
        )
        ?.remove();


    setText(
        "paymentOutstandingTotal",
        "R0.00"
    );


    setText(
        "paymentOutstandingCount",
        "No payments due"
    );


    const paidTotal =
        document.getElementById(
            "paymentPaidTotal"
        );


    if (paidTotal) {

        const basePaid =
            2498.00;

        paidTotal.textContent =
            `R${(
                basePaid +
                Number(payment.amount)
            ).toLocaleString(
                "en-ZA",
                {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }
            )}`;
    }


    setText(
        "paymentReceiptCount",
        "5"
    );
}


/* ============================================================
   RECEIPT
   ============================================================ */

function initialiseReceiptActions() {

    document
        .getElementById(
            "printPrototypeReceipt"
        )
        ?.addEventListener(
            "click",
            () => {

                window.print();
            }
        );
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