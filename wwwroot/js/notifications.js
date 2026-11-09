"use strict";


const q4uNotificationStateKey =
    "q4uPrototypeNotificationState";


document.addEventListener(
    "DOMContentLoaded",
    () => {

        injectCrossModuleNotifications();
        restoreNotificationState();
        initialiseNotificationCentre();
        initialiseNotificationPreferences();
        refreshNotificationCounts();
    }
);


/* ============================================================
   CROSS-MODULE NOTIFICATIONS
   ============================================================ */

function injectCrossModuleNotifications() {

    const list =
        document.getElementById(
            "notificationList"
        );

    const template =
        document.getElementById(
            "sessionNotificationTemplate"
        );


    if (!list || !template) {
        return;
    }


    const items = [];


    const renewal =
        readSessionJson(
            "q4uPrototypeRenewalRequest"
        );


    if (renewal) {

        items.push({
            id: "session-renewal",
            type: "renewal",
            icon: "R",
            eyebrow: "RENEWAL SUBMITTED",
            title:
                `${renewal.vehicleName} renewal submitted`,
            message:
                `${renewal.reference} has entered the Q4U prototype renewal workflow.`,
            link:
                `/Renewals/Track?reference=${encodeURIComponent(renewal.reference)}&newRequest=true`
        });
    }


    const documentData =
        readSessionJson(
            "q4uPrototypeUploadedDocument"
        );


    if (documentData) {

        items.push({
            id: "session-document",
            type: "document",
            icon: "DOC",
            eyebrow: "DOCUMENT ADDED",
            title:
                `${documentData.name} added`,
            message:
                `${documentData.name} is now visible in your prototype document library.`,
            link:
                "/Documents"
        });
    }


    const service =
        readSessionJson(
            "q4uPrototypeServiceRequest"
        );


    if (service) {

        items.push({
            id: "session-service",
            type: "service",
            icon: "Q4U",
            eyebrow: "SERVICE REQUEST",
            title:
                `${service.serviceName} request received`,
            message:
                `${service.reference} has entered the Q4U service workflow.`,
            link:
                `/ServiceRequests/Track?reference=${encodeURIComponent(service.reference)}&newRequest=true`
        });
    }


    const payment =
        readSessionJson(
            "q4uPrototypePayment"
        );


    if (payment) {

        items.push({
            id: "session-payment",
            type: "payment",
            icon: "R",
            eyebrow: "PAYMENT COMPLETE",
            title:
                "Payment receipt available",
            message:
                `${payment.paymentReference} has been recorded for ${payment.service}.`,
            link:
                `/Payments/Receipt?reference=${encodeURIComponent(payment.paymentReference)}&newPayment=true`
        });
    }


    items
        .reverse()
        .forEach(item => {

            if (
                list.querySelector(
                    `[data-id="${item.id}"]`
                )
            ) {
                return;
            }


            const fragment =
                template.content
                    .cloneNode(true);


            const record =
                fragment.querySelector(
                    "[data-notification-record]"
                );


            if (!record) {
                return;
            }


            record.dataset.id =
                item.id;

            record.dataset.type =
                item.type;

            record.dataset.search =
                `${item.eyebrow} ${item.title} ${item.message}`
                    .toLowerCase();


            const icon =
                fragment.querySelector(
                    "[data-session-notification-icon]"
                );


            if (icon) {

                icon.textContent =
                    item.icon;

                icon.classList.add(
                    item.type
                );
            }


            setNodeText(
                fragment.querySelector(
                    "[data-session-notification-eyebrow]"
                ),
                item.eyebrow
            );


            setNodeText(
                fragment.querySelector(
                    "[data-session-notification-title]"
                ),
                item.title
            );


            setNodeText(
                fragment.querySelector(
                    "[data-session-notification-message]"
                ),
                item.message
            );


            const link =
                fragment.querySelector(
                    "[data-session-notification-link]"
                );


            if (link) {

                link.href =
                    item.link;

                link.setAttribute(
                    "data-notification-open",
                    ""
                );
            }


            list.prepend(
                fragment
            );
        });
}


/* ============================================================
   CENTRE
   ============================================================ */

function initialiseNotificationCentre() {

    const list =
        document.getElementById(
            "notificationList"
        );


    if (!list) {
        return;
    }


    let activeFilter =
        "all";


    const search =
        document.getElementById(
            "notificationSearch"
        );


    const applyFilters = () => {

        const query =
            search?.value
                .trim()
                .toLowerCase() ?? "";


        let visible = 0;


        list
            .querySelectorAll(
                "[data-notification-record]"
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


                let matchesFilter =
                    activeFilter === "all";


                if (
                    activeFilter === "unread"
                ) {

                    matchesFilter =
                        record.dataset.read !==
                        "true";
                }
                else if (
                    activeFilter !== "all"
                ) {

                    matchesFilter =
                        record.dataset.type ===
                        activeFilter;
                }


                const show =
                    matchesSearch &&
                    matchesFilter;


                record.style.display =
                    show
                        ? "grid"
                        : "none";


                if (show) {
                    visible++;
                }
            });


        document
            .getElementById(
                "notificationEmptyState"
            )
            ?.classList.toggle(
                "d-none",
                visible > 0
            );
    };


    document
        .querySelectorAll(
            "[data-notification-filter]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    activeFilter =
                        button.dataset
                            .notificationFilter;


                    document
                        .querySelectorAll(
                            "[data-notification-filter]"
                        )
                        .forEach(item =>
                            item.classList
                                .remove(
                                    "active"
                                )
                        );


                    button.classList.add(
                        "active"
                    );


                    applyFilters();
                }
            );
        });


    search?.addEventListener(
        "input",
        applyFilters
    );


    document
        .getElementById(
            "resetNotificationFilters"
        )
        ?.addEventListener(
            "click",
            () => {

                activeFilter =
                    "all";


                if (search) {
                    search.value = "";
                }


                document
                    .querySelectorAll(
                        "[data-notification-filter]"
                    )
                    .forEach(button => {

                        button.classList.toggle(
                            "active",
                            button.dataset
                                .notificationFilter ===
                            "all"
                        );
                    });


                applyFilters();
            }
        );


    document.addEventListener(
        "click",
        event => {

            const readButton =
                event.target.closest(
                    "[data-notification-read-toggle]"
                );


            if (readButton) {

                const record =
                    readButton.closest(
                        "[data-notification-record]"
                    );


                if (record) {

                    setNotificationReadState(
                        record,
                        record.dataset.read !==
                        "true"
                    );
                }

                return;
            }


            const dismiss =
                event.target.closest(
                    "[data-dismiss-notification]"
                );


            if (dismiss) {

                const record =
                    dismiss.closest(
                        "[data-notification-record]"
                    );


                if (record) {

                    dismissNotification(
                        record
                    );
                }

                return;
            }


            const openLink =
                event.target.closest(
                    "[data-notification-open]"
                );


            if (openLink) {

                const record =
                    openLink.closest(
                        "[data-notification-record]"
                    );


                if (
                    record &&
                    record.dataset.read !==
                    "true"
                ) {

                    setNotificationReadState(
                        record,
                        true
                    );
                }
            }
        }
    );


    document
        .getElementById(
            "markAllNotificationsRead"
        )
        ?.addEventListener(
            "click",
            () => {

                list
                    .querySelectorAll(
                        "[data-notification-record]"
                    )
                    .forEach(record => {

                        setNotificationReadState(
                            record,
                            true,
                            false
                        );
                    });


                persistNotificationState();
                refreshNotificationCounts();
                applyFilters();


                showPortalToast(
                    "Notifications updated",
                    "All notifications have been marked as read."
                );
            }
        );


    window.q4uApplyNotificationFilters =
        applyFilters;
}


/* ============================================================
   READ / UNREAD
   ============================================================ */

function setNotificationReadState(
    record,
    isRead,
    persist = true) {

    record.dataset.read =
        String(isRead);


    record.classList.toggle(
        "read",
        isRead
    );


    record.classList.toggle(
        "unread",
        !isRead
    );


    record
        .querySelectorAll(
            "[data-notification-read-toggle]"
        )
        .forEach(button => {

            button.textContent =
                isRead
                    ? "Mark unread"
                    : "Mark read";
        });


    if (persist) {

        persistNotificationState();
        refreshNotificationCounts();


        window
            .q4uApplyNotificationFilters
            ?.();
    }
}


/* ============================================================
   DISMISS
   ============================================================ */

function dismissNotification(
    record) {

    const title =
        record.querySelector("h3")
            ?.textContent
            .trim() ??
        "Notification";


    record.classList.add(
        "q4u-notification-removing"
    );


    setTimeout(
        () => {

            record.remove();

            persistNotificationState();
            refreshNotificationCounts();


            window
                .q4uApplyNotificationFilters
                ?.();
        },
        160
    );


    showPortalToast(
        "Notification dismissed",
        `${title} was removed from this prototype session.`
    );
}


/* ============================================================
   PREFERENCES
   ============================================================ */

function initialiseNotificationPreferences() {

    document
        .querySelectorAll(
            "[data-notification-preference]"
        )
        .forEach(toggle => {

            toggle.addEventListener(
                "change",
                () => {

                    const type =
                        toggle.dataset
                            .notificationPreference;


                    showPortalToast(
                        `${type} notifications ${toggle.checked ? "enabled" : "disabled"}`,
                        `${type} delivery preference has been updated for this prototype.`
                    );
                }
            );
        });
}


/* ============================================================
   COUNTS
   ============================================================ */

function refreshNotificationCounts() {

    const records =
        Array.from(
            document.querySelectorAll(
                "[data-notification-record]"
            )
        );


    const unread =
        records.filter(record =>
            record.dataset.read !==
            "true"
        ).length;


    setText(
        "notificationUnreadCount",
        unread
    );


    setText(
        "notificationUnreadTabCount",
        unread
    );


    setText(
        "notificationAllCount",
        records.length
    );
}


/* ============================================================
   STATE PERSISTENCE
   ============================================================ */

function persistNotificationState() {

    const state = {};


    document
        .querySelectorAll(
            "[data-notification-record]"
        )
        .forEach(record => {

            const id =
                record.dataset.id;


            if (!id) {
                return;
            }


            state[id] = {
                read:
                    record.dataset.read ===
                    "true",

                dismissed:
                    false
            };
        });


    const previous =
        readSessionJson(
            q4uNotificationStateKey
        ) ?? {};


    Object.keys(previous)
        .forEach(id => {

            const stillExists =
                document.querySelector(
                    `[data-notification-record][data-id="${id}"]`
                );


            if (!stillExists) {

                state[id] = {
                    read:
                        previous[id]?.read ??
                        false,

                    dismissed:
                        true
                };
            }
        });


    sessionStorage.setItem(
        q4uNotificationStateKey,
        JSON.stringify(state)
    );
}


function restoreNotificationState() {

    const state =
        readSessionJson(
            q4uNotificationStateKey
        );


    if (!state) {
        return;
    }


    Object.entries(state)
        .forEach(
            ([id, item]) => {

                const record =
                    document.querySelector(
                        `[data-notification-record][data-id="${id}"]`
                    );


                if (!record) {
                    return;
                }


                if (item.dismissed) {

                    record.remove();

                    return;
                }


                setNotificationReadState(
                    record,
                    item.read,
                    false
                );
            }
        );
}


/* ============================================================
   HELPERS
   ============================================================ */

function readSessionJson(key) {

    const value =
        sessionStorage.getItem(
            key
        );


    if (!value) {
        return null;
    }


    try {

        return JSON.parse(
            value
        );
    }
    catch {

        return null;
    }
}


function setText(
    id,
    value) {

    const element =
        document.getElementById(
            id
        );


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