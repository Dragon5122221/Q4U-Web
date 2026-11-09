"use strict";

const q4uSettingsStorageKey =
    "q4uPrototypeSettings";


document.addEventListener(
    "DOMContentLoaded",
    () => {

        initialiseSettingsTabs();
        initialiseProfileEditing();
        initialiseSettingsPreferences();
        initialiseSecuritySettings();
        initialiseAccountActions();
        restoreSettingsState();
    }
);


/* ============================================================
   TABS
   ============================================================ */

function initialiseSettingsTabs() {

    const buttons =
        document.querySelectorAll(
            "[data-settings-tab]"
        );

    const panels =
        document.querySelectorAll(
            "[data-settings-panel]"
        );


    if (!buttons.length) {
        return;
    }


    buttons.forEach(button => {

        button.addEventListener(
            "click",
            () => {

                const tab =
                    button.dataset.settingsTab;


                buttons.forEach(item =>
                    item.classList.remove(
                        "active"
                    )
                );


                button.classList.add(
                    "active"
                );


                panels.forEach(panel => {

                    panel.classList.toggle(
                        "d-none",
                        panel.dataset.settingsPanel !==
                        tab
                    );
                });
            }
        );
    });
}


/* ============================================================
   PROFILE
   ============================================================ */

function initialiseProfileEditing() {

    document
        .getElementById(
            "editProfileButton"
        )
        ?.addEventListener(
            "click",
            () => {

                bootstrap.Modal
                    .getOrCreateInstance(
                        document.getElementById(
                            "editProfileModal"
                        )
                    )
                    .show();
            }
        );


    document
        .getElementById(
            "saveProfileChanges"
        )
        ?.addEventListener(
            "click",
            saveProfileChanges
        );
}


function saveProfileChanges() {

    const firstName =
        valueOf(
            "settingsFirstName"
        );

    const lastName =
        valueOf(
            "settingsLastName"
        );

    const email =
        valueOf(
            "settingsEmail"
        );

    const mobile =
        valueOf(
            "settingsMobile"
        );

    const address =
        valueOf(
            "settingsAddress"
        );

    const suburb =
        valueOf(
            "settingsSuburb"
        );

    const city =
        valueOf(
            "settingsCity"
        );

    const province =
        valueOf(
            "settingsProvince"
        );

    const postalCode =
        valueOf(
            "settingsPostalCode"
        );


    if (
        !firstName ||
        !lastName ||
        !email ||
        !mobile
    ) {

        showPortalToast(
            "Information required",
            "Please complete your name, email and mobile number."
        );

        return;
    }


    const fullName =
        `${firstName} ${lastName}`;


    setText(
        "settingsProfileName",
        fullName
    );

    setText(
        "settingsSideName",
        fullName
    );

    setText(
        "settingsEmailDisplay",
        email
    );

    setText(
        "settingsMobileDisplay",
        mobile
    );


    const addressDisplay =
        document.getElementById(
            "settingsAddressDisplay"
        );


    if (addressDisplay) {

        addressDisplay.innerHTML =
            `${escapeHtml(address)}<br>` +
            `${escapeHtml(suburb)}, ${escapeHtml(city)}<br>` +
            `${escapeHtml(province)}, ${escapeHtml(postalCode)}`;
    }


    const state =
        getSettingsState();


    state.profile = {
        firstName,
        lastName,
        email,
        mobile,
        address,
        suburb,
        city,
        province,
        postalCode
    };


    saveSettingsState(
        state
    );


    bootstrap.Modal
        .getInstance(
            document.getElementById(
                "editProfileModal"
            )
        )
        ?.hide();


    showPortalToast(
        "Profile updated",
        "Your prototype customer information has been updated."
    );
}


/* ============================================================
   NOTIFICATIONS
   ============================================================ */

function initialiseSettingsPreferences() {

    document
        .getElementById(
            "saveNotificationPreferences"
        )
        ?.addEventListener(
            "click",
            () => {

                const preferences = {};


                document
                    .querySelectorAll(
                        "[data-settings-preference]"
                    )
                    .forEach(toggle => {

                        preferences[
                            toggle.dataset.settingsPreference
                        ] =
                            toggle.checked;
                    });


                const reminders = {};


                document
                    .querySelectorAll(
                        "[data-reminder-period]"
                    )
                    .forEach(toggle => {

                        reminders[
                            toggle.dataset.reminderPeriod
                        ] =
                            toggle.checked;
                    });


                const state =
                    getSettingsState();


                state.notifications = {
                    preferences,
                    reminders
                };


                saveSettingsState(
                    state
                );


                showPortalToast(
                    "Preferences saved",
                    "Your prototype notification preferences have been updated."
                );
            }
        );
}


/* ============================================================
   SECURITY
   ============================================================ */

function initialiseSecuritySettings() {

    const newPassword =
        document.getElementById(
            "newPassword"
        );

    const confirmPassword =
        document.getElementById(
            "confirmNewPassword"
        );

    const updateButton =
        document.getElementById(
            "savePasswordChange"
        );


    const validatePassword = () => {

        const value =
            newPassword?.value ??
            "";


        const rules = {

            length:
                value.length >= 8,

            upper:
                /[A-Z]/.test(
                    value
                ),

            number:
                /\d/.test(
                    value
                )
        };


        Object.entries(rules)
            .forEach(
                ([rule, valid]) => {

                    document
                        .querySelector(
                            `[data-password-rule="${rule}"]`
                        )
                        ?.classList.toggle(
                            "valid",
                            valid
                        );
                }
            );


        const allValid =
            Object.values(rules)
                .every(Boolean);


        const matches =
            value.length > 0 &&
            value ===
            confirmPassword?.value;


        if (updateButton) {

            updateButton.disabled =
                !(allValid && matches);
        }
    };


    newPassword?.addEventListener(
        "input",
        validatePassword
    );


    confirmPassword?.addEventListener(
        "input",
        validatePassword
    );


    updateButton?.addEventListener(
        "click",
        () => {

            const modal =
                document.getElementById(
                    "changePasswordModal"
                );


            if (newPassword) {
                newPassword.value = "";
            }


            if (confirmPassword) {
                confirmPassword.value = "";
            }


            validatePassword();


            bootstrap.Modal
                .getInstance(modal)
                ?.hide();


            showPortalToast(
                "Password updated",
                "The prototype password change was completed successfully."
            );
        }
    );


    document
        .getElementById(
            "twoFactorToggle"
        )
        ?.addEventListener(
            "change",
            event => {

                const enabled =
                    event.target.checked;


                const state =
                    getSettingsState();


                state.twoFactor =
                    enabled;


                saveSettingsState(
                    state
                );


                showPortalToast(
                    enabled
                        ? "Two-step verification enabled"
                        : "Two-step verification disabled",

                    enabled
                        ? "An additional prototype sign-in step is now enabled."
                        : "The additional prototype sign-in step has been disabled."
                );
            }
        );
}


/* ============================================================
   ACCOUNT
   ============================================================ */

function initialiseAccountActions() {

    document
        .getElementById(
            "exportAccountSummary"
        )
        ?.addEventListener(
            "click",
            () => {

                showPortalToast(
                    "Account summary prepared",
                    "A production system could export the customer's account information here."
                );
            }
        );


    document
        .getElementById(
            "confirmDeleteAccount"
        )
        ?.addEventListener(
            "click",
            () => {

                bootstrap.Modal
                    .getInstance(
                        document.getElementById(
                            "deleteAccountModal"
                        )
                    )
                    ?.hide();


                showPortalToast(
                    "Prototype removal requested",
                    "No customer information has actually been deleted."
                );
            }
        );
}


/* ============================================================
   RESTORE STATE
   ============================================================ */

function restoreSettingsState() {

    const state =
        getSettingsState();


    if (state.profile) {

        const profile =
            state.profile;


        setValue(
            "settingsFirstName",
            profile.firstName
        );

        setValue(
            "settingsLastName",
            profile.lastName
        );

        setValue(
            "settingsEmail",
            profile.email
        );

        setValue(
            "settingsMobile",
            profile.mobile
        );

        setValue(
            "settingsAddress",
            profile.address
        );

        setValue(
            "settingsSuburb",
            profile.suburb
        );

        setValue(
            "settingsCity",
            profile.city
        );

        setValue(
            "settingsProvince",
            profile.province
        );

        setValue(
            "settingsPostalCode",
            profile.postalCode
        );


        const fullName =
            `${profile.firstName} ${profile.lastName}`;


        setText(
            "settingsProfileName",
            fullName
        );

        setText(
            "settingsSideName",
            fullName
        );

        setText(
            "settingsEmailDisplay",
            profile.email
        );

        setText(
            "settingsMobileDisplay",
            profile.mobile
        );


        const address =
            document.getElementById(
                "settingsAddressDisplay"
            );


        if (address) {

            address.innerHTML =
                `${escapeHtml(profile.address)}<br>` +
                `${escapeHtml(profile.suburb)}, ${escapeHtml(profile.city)}<br>` +
                `${escapeHtml(profile.province)}, ${escapeHtml(profile.postalCode)}`;
        }
    }


    if (state.notifications) {

        const preferences =
            state.notifications
                .preferences ??
            {};


        document
            .querySelectorAll(
                "[data-settings-preference]"
            )
            .forEach(toggle => {

                const key =
                    toggle.dataset
                        .settingsPreference;


                if (
                    typeof preferences[key] ===
                    "boolean"
                ) {

                    toggle.checked =
                        preferences[key];
                }
            });


        const reminders =
            state.notifications
                .reminders ??
            {};


        document
            .querySelectorAll(
                "[data-reminder-period]"
            )
            .forEach(toggle => {

                const key =
                    toggle.dataset
                        .reminderPeriod;


                if (
                    typeof reminders[key] ===
                    "boolean"
                ) {

                    toggle.checked =
                        reminders[key];
                }
            });
    }


    const twoFactor =
        document.getElementById(
            "twoFactorToggle"
        );


    if (
        twoFactor &&
        typeof state.twoFactor ===
        "boolean"
    ) {

        twoFactor.checked =
            state.twoFactor;
    }
}


/* ============================================================
   STORAGE
   ============================================================ */

function getSettingsState() {

    const raw =
        sessionStorage.getItem(
            q4uSettingsStorageKey
        );


    if (!raw) {
        return {};
    }


    try {

        return JSON.parse(
            raw
        );
    }
    catch {

        return {};
    }
}


function saveSettingsState(
    state) {

    sessionStorage.setItem(
        q4uSettingsStorageKey,
        JSON.stringify(
            state
        )
    );
}


/* ============================================================
   HELPERS
   ============================================================ */

function valueOf(id) {

    return document
        .getElementById(id)
        ?.value
        ?.trim() ??
        "";
}


function setValue(
    id,
    value) {

    const element =
        document.getElementById(id);


    if (element) {

        element.value =
            value ?? "";
    }
}


function setText(
    id,
    value) {

    const element =
        document.getElementById(id);


    if (element) {

        element.textContent =
            value ?? "";
    }
}


function escapeHtml(value) {

    return String(
        value ?? ""
    )
        .replaceAll(
            "&",
            "&amp;"
        )
        .replaceAll(
            "<",
            "&lt;"
        )
        .replaceAll(
            ">",
            "&gt;"
        )
        .replaceAll(
            '"',
            "&quot;"
        )
        .replaceAll(
            "'",
            "&#039;"
        );
}