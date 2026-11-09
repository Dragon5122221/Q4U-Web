"use strict";

document.addEventListener("DOMContentLoaded", () => {

    initialiseVehicleLibrary();
    initialiseVehicleDetailTabs();
    initialiseVehicleDetailActions();
    initialiseAddVehicleWizard();
    initialiseSessionVehicleBadge();

});


/* ============================================================
   SESSION VEHICLE
   ============================================================ */

const q4uSessionVehicleKey =
    "q4uPrototypeAddedVehicle";


function getSessionVehicle() {

    const stored =
        sessionStorage.getItem(
            q4uSessionVehicleKey
        );

    if (!stored) {
        return null;
    }

    try {
        return JSON.parse(stored);
    }
    catch {
        return null;
    }
}


function initialiseSessionVehicleBadge() {

    const vehicle =
        getSessionVehicle();

    if (!vehicle) {
        return;
    }

    document
        .querySelectorAll(
            '.q4u-portal-nav a[href*="/Portal/Vehicles"] em'
        )
        .forEach(badge => {
            badge.textContent = "4";
        });
}


/* ============================================================
   VEHICLE LIBRARY
   ============================================================ */

function initialiseVehicleLibrary() {

    const grid =
        document.getElementById(
            "vehicleLibraryGrid"
        );

    if (!grid) {
        return;
    }

    injectSessionVehicle(grid);

    const search =
        document.getElementById(
            "vehicleSearch"
        );

    const filter =
        document.getElementById(
            "vehicleStatusFilter"
        );

    const sort =
        document.getElementById(
            "vehicleSort"
        );

    const emptyState =
        document.getElementById(
            "vehicleEmptyState"
        );

    const resetButton =
        document.getElementById(
            "resetVehicleFilters"
        );

    const getCards = () =>
        Array.from(
            grid.querySelectorAll(
                "[data-vehicle-card]"
            )
        );


    const applyFilters = () => {

        const query =
            search?.value
                .trim()
                .toLowerCase() ?? "";

        const status =
            filter?.value ?? "all";

        let visible = 0;

        getCards().forEach(card => {

            const searchText =
                card.dataset.search ??
                card.textContent.toLowerCase();

            const matchesSearch =
                !query ||
                searchText.includes(query);

            const matchesStatus =
                status === "all" ||
                card.dataset.status === status;

            const show =
                matchesSearch &&
                matchesStatus;

            card.style.display =
                show ? "" : "none";

            if (show) {
                visible++;
            }
        });

        emptyState?.classList.toggle(
            "d-none",
            visible > 0
        );
    };


    search?.addEventListener(
        "input",
        applyFilters
    );

    filter?.addEventListener(
        "change",
        applyFilters
    );


    sort?.addEventListener(
        "change",
        () => {

            const cards =
                getCards();

            if (sort.value === "expiry") {

                cards.sort(
                    (a, b) =>
                        new Date(a.dataset.expiry) -
                        new Date(b.dataset.expiry)
                );
            }
            else if (sort.value === "name") {

                cards.sort(
                    (a, b) =>
                        a.textContent
                            .trim()
                            .localeCompare(
                                b.textContent.trim()
                            )
                );
            }
            else {

                const order = {
                    bmw: 1,
                    polo: 2,
                    hilux: 3,
                    "session-added": 4
                };

                cards.sort(
                    (a, b) =>
                        (order[a.dataset.id] ?? 99) -
                        (order[b.dataset.id] ?? 99)
                );
            }

            cards.forEach(card =>
                grid.appendChild(card)
            );

            applyFilters();
        }
    );


    document
        .querySelectorAll(
            "[data-vehicle-view]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    document
                        .querySelectorAll(
                            "[data-vehicle-view]"
                        )
                        .forEach(item =>
                            item.classList.remove(
                                "active"
                            )
                        );

                    button.classList.add(
                        "active"
                    );

                    grid.classList.toggle(
                        "list-mode",
                        button.dataset.vehicleView ===
                        "list"
                    );
                }
            );
        });


    resetButton?.addEventListener(
        "click",
        () => {

            if (search) {
                search.value = "";
            }

            if (filter) {
                filter.value = "all";
            }

            applyFilters();
        }
    );
}


function injectSessionVehicle(grid) {

    const vehicle =
        getSessionVehicle();

    const template =
        document.getElementById(
            "sessionVehicleTemplate"
        );

    if (
        !vehicle ||
        !template ||
        grid.querySelector(
            '[data-id="session-added"]'
        )
    ) {
        return;
    }

    const fragment =
        template.content.cloneNode(true);

    const card =
        fragment.querySelector(
            "[data-vehicle-card]"
        );

    card.dataset.search =
        `${vehicle.year} ${vehicle.make} ${vehicle.model} ${vehicle.registration} ${vehicle.province} ${vehicle.colour}`
            .toLowerCase();

    setElementText(
        fragment.querySelector(
            "[data-added-icon]"
        ),
        vehicle.make?.charAt(0) ?? "V"
    );

    setElementText(
        fragment.querySelector(
            "[data-added-subtitle]"
        ),
        `${vehicle.year} · ${vehicle.colour}`
    );

    setElementText(
        fragment.querySelector(
            "[data-added-name]"
        ),
        `${vehicle.make} ${vehicle.model}`
    );

    setElementText(
        fragment.querySelector(
            "[data-added-registration]"
        ),
        vehicle.registration
    );

    setElementText(
        fragment.querySelector(
            "[data-added-province]"
        ),
        vehicle.province
    );

    setElementText(
        fragment.querySelector(
            "[data-added-documents]"
        ),
        vehicle.hasDocument ? "1" : "0"
    );

    fragment
        .querySelector(
            "[data-added-preview]"
        )
        ?.addEventListener(
            "click",
            () => {

                showPortalToast(
                    "Vehicle selected",
                    `${vehicle.make} ${vehicle.model} is a prototype session vehicle.`
                );
            }
        );

    grid.appendChild(fragment);

    const count =
        document.getElementById(
            "vehicleLibraryCount"
        );

    if (count) {
        count.textContent = "4";
    }
}


/* ============================================================
   DETAIL TABS
   ============================================================ */

function initialiseVehicleDetailTabs() {

    const buttons =
        document.querySelectorAll(
            "[data-vehicle-tab]"
        );

    const panels =
        document.querySelectorAll(
            "[data-vehicle-tab-panel]"
        );

    if (!buttons.length) {
        return;
    }

    buttons.forEach(button => {

        button.addEventListener(
            "click",
            () => {

                const target =
                    button.dataset.vehicleTab;

                buttons.forEach(item =>
                    item.classList.toggle(
                        "active",
                        item === button
                    )
                );

                panels.forEach(panel => {

                    panel.classList.toggle(
                        "d-none",
                        panel.dataset.vehicleTabPanel !==
                        target
                    );
                });
            }
        );
    });
}


/* ============================================================
   DETAIL ACTIONS
   ============================================================ */

function initialiseVehicleDetailActions() {

    const editForm =
        document.getElementById(
            "editVehicleForm"
        );

    editForm?.addEventListener(
        "submit",
        event => {

            event.preventDefault();

            const nickname =
                document.getElementById(
                    "editVehicleNickname"
                )?.value.trim();

            const odometer =
                document.getElementById(
                    "editVehicleOdometer"
                )?.value.trim();

            if (nickname) {
                setTextById(
                    "vehicleDetailName",
                    nickname
                );
            }

            if (odometer) {
                setTextById(
                    "vehicleDetailOdometer",
                    odometer
                );
            }

            const modalElement =
                document.getElementById(
                    "editVehicleModal"
                );

            bootstrap.Modal
                .getInstance(modalElement)
                ?.hide();

            showPortalToast(
                "Vehicle updated",
                "Your prototype vehicle details have been updated."
            );
        }
    );


    const removeButton =
        document.getElementById(
            "confirmRemoveVehicle"
        );

    removeButton?.addEventListener(
        "click",
        () => {

            const modal =
                document.getElementById(
                    "removeVehicleModal"
                );

            bootstrap.Modal
                .getInstance(modal)
                ?.hide();

            showPortalToast(
                "Prototype removal complete",
                "The vehicle would now be removed from the customer profile."
            );

            window.setTimeout(
                () => {
                    window.location.href =
                        "/Vehicles";
                },
                900
            );
        }
    );


    const reminderToggle =
        document.getElementById(
            "vehicleReminderToggle"
        );

    reminderToggle?.addEventListener(
        "change",
        () => {

            showPortalToast(
                reminderToggle.checked
                    ? "Reminders enabled"
                    : "Reminders disabled",

                reminderToggle.checked
                    ? "Renewal reminders are active for this vehicle."
                    : "Renewal reminders have been disabled in the prototype."
            );
        }
    );
}


/* ============================================================
   ADD VEHICLE WIZARD
   ============================================================ */

function initialiseAddVehicleWizard() {

    const steps =
        document.querySelectorAll(
            "[data-wizard-step]"
        );

    if (!steps.length) {
        return;
    }

    let currentStep = 1;
    let lookupComplete = false;


    const showStep = step => {

        currentStep = step;

        steps.forEach(panel => {

            panel.classList.toggle(
                "d-none",
                Number(
                    panel.dataset.wizardStep
                ) !== step
            );
        });


        document
            .querySelectorAll(
                "[data-wizard-indicator]"
            )
            .forEach(indicator => {

                const number =
                    Number(
                        indicator.dataset
                            .wizardIndicator
                    );

                indicator.classList.toggle(
                    "active",
                    number === step
                );

                indicator.classList.toggle(
                    "complete",
                    number < step
                );

                const numberElement =
                    indicator.querySelector(
                        ":scope > span"
                    );

                if (numberElement) {

                    numberElement.textContent =
                        number < step
                            ? "✓"
                            : number;
                }
            });


        if (step === 4) {
            populateReview();
        }

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    };


    const lookupButton =
        document.getElementById(
            "lookupVehicleButton"
        );

    const lookupResult =
        document.getElementById(
            "vehicleLookupResult"
        );

    const continueButton =
        document.getElementById(
            "stepOneContinue"
        );


    lookupButton?.addEventListener(
        "click",
        () => {

            const registration =
                document
                    .getElementById(
                        "addVehicleRegistration"
                    )
                    ?.value.trim()
                    .toUpperCase();

            const province =
                document
                    .getElementById(
                        "addVehicleProvince"
                    )
                    ?.value;

            if (!registration) {

                showPortalToast(
                    "Registration required",
                    "Enter a registration number before searching."
                );

                return;
            }

            lookupButton.classList.add(
                "loading"
            );

            lookupButton.disabled = true;


            window.setTimeout(
                () => {

                    lookupButton.classList.remove(
                        "loading"
                    );

                    lookupButton.disabled = false;

                    lookupResult?.classList.remove(
                        "d-none"
                    );

                    setTextById(
                        "lookupPlateDisplay",
                        registration
                    );

                    setTextById(
                        "lookupProvinceDisplay",
                        province
                    );

                    lookupComplete = true;

                    if (continueButton) {
                        continueButton.disabled = false;
                    }

                    showPortalToast(
                        "Vehicle found",
                        "A matching prototype vehicle record was found."
                    );
                },
                650
            );
        }
    );


    document
        .querySelectorAll(
            "[data-next-step]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    if (
                        currentStep === 1 &&
                        !lookupComplete
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
            "[data-previous-step]"
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


    initialiseVehicleFileSelection();


    document
        .getElementById(
            "completeAddVehicle"
        )
        ?.addEventListener(
            "click",
            completeVehicleAddition
        );
}


/* ============================================================
   FILE SELECTION
   ============================================================ */

function initialiseVehicleFileSelection() {

    const fileInput =
        document.getElementById(
            "addVehicleFileInput"
        );

    const chooseButton =
        document.getElementById(
            "chooseVehicleFile"
        );

    const fileDisplay =
        document.getElementById(
            "selectedVehicleFile"
        );

    chooseButton?.addEventListener(
        "click",
        () => fileInput?.click()
    );


    fileInput?.addEventListener(
        "change",
        () => {

            const file =
                fileInput.files?.[0];

            if (!file) {
                return;
            }

            setTextById(
                "selectedVehicleFileName",
                file.name
            );

            fileDisplay?.classList.remove(
                "d-none"
            );
        }
    );


    document
        .getElementById(
            "removeSelectedVehicleFile"
        )
        ?.addEventListener(
            "click",
            () => {

                if (fileInput) {
                    fileInput.value = "";
                }

                fileDisplay?.classList.add(
                    "d-none"
                );
            }
        );
}


/* ============================================================
   REVIEW / COMPLETE
   ============================================================ */

function populateReview() {

    const year =
        document.getElementById(
            "newVehicleYear"
        )?.value ?? "2019";

    const make =
        document.getElementById(
            "newVehicleMake"
        )?.value ?? "Ford";

    const model =
        document.getElementById(
            "newVehicleModel"
        )?.value ?? "Ranger";

    const colour =
        document.getElementById(
            "newVehicleColour"
        )?.value ?? "";

    const body =
        document.getElementById(
            "newVehicleBody"
        )?.value ?? "";

    const registration =
        document.getElementById(
            "addVehicleRegistration"
        )?.value.toUpperCase() ?? "";

    const province =
        document.getElementById(
            "addVehicleProvince"
        )?.value ?? "GP";

    const file =
        document.getElementById(
            "addVehicleFileInput"
        )?.files?.[0];


    setTextById(
        "reviewVehicleName",
        `${year} ${make} ${model}`
    );

    setTextById(
        "reviewVehicleColour",
        `${colour} · ${body}`
    );

    setTextById(
        "reviewVehicleRegistration",
        registration
    );

    setTextById(
        "reviewVehicleProvince",
        province
    );

    setTextById(
        "reviewDocumentStatus",
        file
            ? file.name
            : "Add later"
    );

    setTextById(
        "reviewSmsStatus",
        document.getElementById(
            "addVehicleSms"
        )?.checked
            ? "Enabled"
            : "Disabled"
    );

    setTextById(
        "reviewPortalStatus",
        document.getElementById(
            "addVehiclePortalReminder"
        )?.checked
            ? "Enabled"
            : "Disabled"
    );
}


function completeVehicleAddition() {

    const vehicle = {

        year:
            document.getElementById(
                "newVehicleYear"
            )?.value ?? "2019",

        make:
            document.getElementById(
                "newVehicleMake"
            )?.value ?? "Ford",

        model:
            document.getElementById(
                "newVehicleModel"
            )?.value ?? "Ranger",

        colour:
            document.getElementById(
                "newVehicleColour"
            )?.value ?? "White",

        registration:
            document
                .getElementById(
                    "addVehicleRegistration"
                )
                ?.value
                .trim()
                .toUpperCase() ?? "",

        province:
            document.getElementById(
                "addVehicleProvince"
            )?.value ?? "GP",

        hasDocument:
            Boolean(
                document.getElementById(
                    "addVehicleFileInput"
                )?.files?.length
            )
    };


    sessionStorage.setItem(
        q4uSessionVehicleKey,
        JSON.stringify(vehicle)
    );


    document
        .getElementById(
            "addVehicleReview"
        )
        ?.classList.add(
            "d-none"
        );


    document
        .getElementById(
            "addVehicleSuccess"
        )
        ?.classList.remove(
            "d-none"
        );


    setTextById(
        "successVehiclePlate",
        vehicle.registration
    );

    setTextById(
        "successVehicleProvince",
        vehicle.province
    );


    initialiseSessionVehicleBadge();


    showPortalToast(
        "Vehicle added",
        `${vehicle.make} ${vehicle.model} has been added to your prototype profile.`
    );
}


/* ============================================================
   HELPERS
   ============================================================ */

function setTextById(id, value) {

    const element =
        document.getElementById(id);

    if (element) {
        element.textContent = value;
    }
}


function setElementText(element, value) {

    if (element) {
        element.textContent = value;
    }
}