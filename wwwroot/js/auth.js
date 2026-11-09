"use strict";

document.addEventListener("DOMContentLoaded", () => {

    initialisePasswordToggles();
    initialiseDemoLogin();
    initialiseAuthenticationForms();
    initialisePasswordReset();

});


function initialisePasswordToggles() {

    document
        .querySelectorAll("[data-password-toggle]")
        .forEach(button => {

            button.addEventListener("click", () => {

                const selector =
                    button.dataset.passwordToggle;

                const input =
                    document.querySelector(selector);

                if (!input) {
                    return;
                }

                const showing =
                    input.type === "text";

                input.type =
                    showing
                        ? "password"
                        : "text";

                button.textContent =
                    showing
                        ? "Show"
                        : "Hide";
            });

        });
}


function initialiseDemoLogin() {

    const button =
        document.getElementById("useDemoLogin");

    if (!button) {
        return;
    }

    button.addEventListener("click", () => {

        const email =
            document.getElementById("loginEmail");

        const password =
            document.getElementById("loginPassword");

        if (email) {
            email.value =
                "ayesha.khan@example.co.za";
        }

        if (password) {
            password.value =
                "Q4U2026!";
        }

        email?.focus();
    });
}


function initialiseAuthenticationForms() {

    const forms = [
        document.getElementById("q4uLoginForm"),
        document.getElementById("q4uRegisterForm")
    ].filter(Boolean);

    forms.forEach(form => {

        form.addEventListener("submit", event => {

            event.preventDefault();
            event.stopPropagation();

            if (!form.checkValidity()) {

                form.classList.add("was-validated");

                return;
            }

            form.classList.add("was-validated");

            const submit =
                form.querySelector(".q4u-auth-submit");

            submit?.classList.add("is-loading");

            const redirect =
                form.dataset.redirect;

            window.setTimeout(() => {

                if (redirect) {
                    window.location.assign(redirect);
                }

            }, 750);
        });
    });
}


function initialisePasswordReset() {

    const sendButton =
        document.getElementById("sendResetLink");

    const formContent =
        document.getElementById(
            "forgotPasswordFormContent"
        );

    const successContent =
        document.getElementById(
            "forgotPasswordSuccess"
        );

    if (
        !sendButton ||
        !formContent ||
        !successContent
    ) {
        return;
    }

    sendButton.addEventListener("click", () => {

        const email =
            document.getElementById("forgotEmail");

        if (
            !email ||
            !email.value.trim()
        ) {
            email?.focus();
            return;
        }

        formContent.classList.add("d-none");
        successContent.classList.remove("d-none");
    });
}