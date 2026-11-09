"use strict";

document.addEventListener("DOMContentLoaded", () => {

    initialisePortalSidebar();
    initialiseNotifications();
    initialiseDashboardVehicleSelector();
    initialisePrototypeActions();
    initialiseSectionFiltering();
    initialiseGreeting();

});


/* ============================================================
   SIDEBAR
   ============================================================ */

function initialisePortalSidebar() {

    const sidebar =
        document.getElementById("portalSidebar");

    const overlay =
        document.getElementById("portalSidebarOverlay");

    const openButton =
        document.getElementById("openPortalSidebar");

    const closeButton =
        document.getElementById("closePortalSidebar");

    if (!sidebar || !overlay) {
        return;
    }

    const openSidebar = () => {

        sidebar.classList.add("open");
        overlay.classList.add("show");

        document.body.style.overflow = "hidden";
    };

    const closeSidebar = () => {

        sidebar.classList.remove("open");
        overlay.classList.remove("show");

        document.body.style.overflow = "";
    };

    openButton?.addEventListener(
        "click",
        openSidebar
    );

    closeButton?.addEventListener(
        "click",
        closeSidebar
    );

    overlay.addEventListener(
        "click",
        closeSidebar
    );

    window.addEventListener(
        "resize",
        () => {

            if (window.innerWidth >= 992) {
                closeSidebar();
            }
        }
    );
}


/* ============================================================
   NOTIFICATIONS
   ============================================================ */

function initialiseNotifications() {

    const notifications =
        document.querySelectorAll(
            "[data-notification]"
        );

    const markAllButton =
        document.getElementById(
            "markAllNotificationsRead"
        );

    notifications.forEach(notification => {

        notification.addEventListener(
            "click",
            () => {

                notification.classList.remove(
                    "unread"
                );

                notification
                    .querySelector("em")
                    ?.remove();

                updateNotificationCount();
            }
        );
    });

    markAllButton?.addEventListener(
        "click",
        () => {

            notifications.forEach(
                notification => {

                    notification.classList.remove(
                        "unread"
                    );

                    notification
                        .querySelector("em")
                        ?.remove();
                }
            );

            updateNotificationCount();

            showPortalToast(
                "Notifications updated",
                "All notifications have been marked as read."
            );
        }
    );

    updateNotificationCount();
}


function updateNotificationCount() {

    const unreadCount =
        document.querySelectorAll(
            "[data-notification].unread"
        ).length;

    const badge =
        document.getElementById(
            "notificationBadge"
        );

    const unreadText =
        document.getElementById(
            "notificationUnreadText"
        );

    if (badge) {

        badge.textContent = unreadCount;

        badge.style.display =
            unreadCount === 0
                ? "none"
                : "flex";
    }

    if (unreadText) {
        unreadText.textContent = unreadCount;
    }
}


/* ============================================================
   DASHBOARD VEHICLE
   ============================================================ */

function initialiseDashboardVehicleSelector() {

    const selector =
        document.getElementById(
            "dashboardVehicleSelect"
        );

    if (!selector) {
        return;
    }

    const updateVehicle = () => {

        const option =
            selector.options[
            selector.selectedIndex
            ];

        if (!option) {
            return;
        }

        setText(
            "dashboardVehicleModel",
            option.dataset.model
        );

        setText(
            "dashboardVehicleRegistration",
            option.dataset.registration
        );

        setText(
            "dashboardVehicleProvince",
            option.dataset.province
        );

        setText(
            "dashboardVehicleExpiry",
            option.dataset.expiry
        );

        setText(
            "dashboardVehicleDays",
            option.dataset.days
        );

        setText(
            "dashboardVehicleDocuments",
            option.dataset.documents
        );

        setText(
            "dashboardVehicleBalance",
            option.dataset.balance
        );

        setText(
            "dashboardVehicleServices",
            option.dataset.services
        );

        setText(
            "renewalModalVehicle",
            option.dataset.model
        );

        setText(
            "renewalModalRegistration",
            `${option.dataset.registration} ${option.dataset.province}`
        );

        const status =
            document.getElementById(
                "dashboardVehicleStatus"
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

        updateRenewalMessage(
            Number(option.dataset.days)
        );
    };

    selector.addEventListener(
        "change",
        updateVehicle
    );

    updateVehicle();
}


function updateRenewalMessage(days) {

    let heading =
        "Renewal approaching";

    let message =
        "Your vehicle's licence renewal window is approaching. Start preparing now to avoid unnecessary delays.";

    if (days <= 7) {

        heading =
            "Renewal requires urgent attention";

        message =
            "This licence expires very soon. We recommend starting the renewal process as soon as possible.";
    }
    else if (days > 90) {

        heading =
            "You're up to date";

        message =
            "No immediate renewal action is required. Q4U will remind you when the renewal window gets closer.";
    }

    setText(
        "dashboardRenewalHeading",
        heading
    );

    setText(
        "dashboardRenewalMessage",
        message
    );
}


function setText(id, value) {

    const element =
        document.getElementById(id);

    if (element) {
        element.textContent = value ?? "";
    }
}


/* ============================================================
   PROTOTYPE ACTIONS
   ============================================================ */

function initialisePrototypeActions() {

    document
        .querySelectorAll(
            "[data-prototype-action]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    showPortalToast(
                        "Prototype action",
                        button.dataset.message ??
                        "Your action was completed."
                    );
                }
            );
        });
}


/* ============================================================
   FILTER / SEARCH
   ============================================================ */

function initialiseSectionFiltering() {

    const search =
        document.getElementById(
            "sectionSearch"
        );

    const filter =
        document.getElementById(
            "sectionStatusFilter"
        );

    const items =
        Array.from(
            document.querySelectorAll(
                "[data-section-item]"
            )
        );

    const emptyState =
        document.getElementById(
            "sectionEmptyState"
        );

    if (!items.length) {
        return;
    }

    const applyFilters = () => {

        const searchValue =
            search?.value
                .trim()
                .toLowerCase() ?? "";

        const statusValue =
            filter?.value ?? "all";

        let visibleCount = 0;

        items.forEach(item => {

            const matchesSearch =
                !searchValue ||
                item.dataset.search
                    ?.includes(searchValue);

            const matchesStatus =
                statusValue === "all" ||
                item.dataset.status ===
                statusValue;

            const visible =
                matchesSearch &&
                matchesStatus;

            item.style.display =
                visible
                    ? "grid"
                    : "none";

            if (visible) {
                visibleCount++;
            }
        });

        emptyState?.classList.toggle(
            "d-none",
            visibleCount > 0
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
}


/* ============================================================
   GREETING
   ============================================================ */

function initialiseGreeting() {

    const greeting =
        document.getElementById(
            "dashboardGreeting"
        );

    if (!greeting) {
        return;
    }

    const hour =
        new Date().getHours();

    if (hour < 12) {
        greeting.textContent =
            "GOOD MORNING";
    }
    else if (hour < 18) {
        greeting.textContent =
            "GOOD AFTERNOON";
    }
    else {
        greeting.textContent =
            "GOOD EVENING";
    }
}


/* ============================================================
   GLOBAL PORTAL TOAST
   ============================================================ */

function showPortalToast(title, message) {

    const element =
        document.getElementById(
            "portalToast"
        );

    if (!element) {
        return;
    }

    setText(
        "portalToastTitle",
        title
    );

    setText(
        "portalToastMessage",
        message
    );

    const toast =
        bootstrap.Toast.getOrCreateInstance(
            element,
            {
                delay: 3200
            }
        );

    toast.show();
}