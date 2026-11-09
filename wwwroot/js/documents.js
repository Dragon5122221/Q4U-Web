"use strict";

const q4uDocumentSessionKey =
    "q4uPrototypeUploadedDocument";


document.addEventListener("DOMContentLoaded", () => {

    initialiseDocumentLibrary();
    initialiseDocumentPreview();
    initialiseDocumentUpload();
    injectSessionDocument();

});


/* ============================================================
   DOCUMENT LIBRARY
   ============================================================ */

function initialiseDocumentLibrary() {

    const list =
        document.getElementById(
            "documentLibraryList"
        );

    if (!list) {
        return;
    }


    const search =
        document.getElementById(
            "documentSearch"
        );

    const category =
        document.getElementById(
            "documentCategoryFilter"
        );

    const vehicle =
        document.getElementById(
            "documentVehicleFilter"
        );

    const status =
        document.getElementById(
            "documentStatusFilter"
        );

    const emptyState =
        document.getElementById(
            "documentEmptyState"
        );


    const applyFilters = () => {

        const records =
            Array.from(
                list.querySelectorAll(
                    "[data-document-record]"
                )
            );

        const query =
            search?.value
                .trim()
                .toLowerCase() ?? "";

        let visible = 0;


        records.forEach(record => {

            const matchesSearch =
                !query ||
                (
                    record.dataset.search ??
                    record.textContent.toLowerCase()
                )
                    .includes(query);


            const matchesCategory =
                category?.value === "all" ||
                record.dataset.category ===
                category?.value;


            const matchesVehicle =
                vehicle?.value === "all" ||
                record.dataset.vehicle ===
                vehicle?.value;


            const matchesStatus =
                status?.value === "all" ||
                record.dataset.status ===
                status?.value;


            const show =
                matchesSearch &&
                matchesCategory &&
                matchesVehicle &&
                matchesStatus;


            record.style.display =
                show
                    ? ""
                    : "none";


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

    category?.addEventListener(
        "change",
        applyFilters
    );

    vehicle?.addEventListener(
        "change",
        applyFilters
    );

    status?.addEventListener(
        "change",
        applyFilters
    );


    document
        .querySelectorAll(
            "[data-document-view]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    document
                        .querySelectorAll(
                            "[data-document-view]"
                        )
                        .forEach(item =>
                            item.classList.remove(
                                "active"
                            )
                        );

                    button.classList.add(
                        "active"
                    );

                    list.classList.toggle(
                        "grid-mode",
                        button.dataset.documentView ===
                        "grid"
                    );
                }
            );
        });


    document
        .getElementById(
            "resetDocumentFilters"
        )
        ?.addEventListener(
            "click",
            () => {

                if (search) {
                    search.value = "";
                }

                if (category) {
                    category.value = "all";
                }

                if (vehicle) {
                    vehicle.value = "all";
                }

                if (status) {
                    status.value = "all";
                }

                applyFilters();
            }
        );
}


/* ============================================================
   PREVIEW
   ============================================================ */

let selectedDocumentRecord = null;


function initialiseDocumentPreview() {

    document.addEventListener(
        "click",
        event => {

            const previewButton =
                event.target.closest(
                    "[data-preview-document]"
                );


            if (previewButton) {

                const record =
                    previewButton.closest(
                        "[data-document-record]"
                    );

                if (record) {
                    openDocumentPreview(record);
                }

                return;
            }


            const replaceButton =
                event.target.closest(
                    "[data-replace-document]"
                );

            if (replaceButton) {

                selectedDocumentRecord =
                    replaceButton.closest(
                        "[data-document-record]"
                    );

                document
                    .getElementById(
                        "replaceDocumentInput"
                    )
                    ?.click();

                return;
            }


            const deleteButton =
                event.target.closest(
                    "[data-delete-document]"
                );

            if (deleteButton) {

                selectedDocumentRecord =
                    deleteButton.closest(
                        "[data-document-record]"
                    );

                openDeleteDocumentModal(
                    selectedDocumentRecord
                );
            }


            const deleteSession =
                event.target.closest(
                    "[data-delete-session-document]"
                );

            if (deleteSession) {

                selectedDocumentRecord =
                    deleteSession.closest(
                        "[data-document-record]"
                    );

                openDeleteDocumentModal(
                    selectedDocumentRecord
                );
            }
        }
    );


    document
        .getElementById(
            "previewReplaceDocument"
        )
        ?.addEventListener(
            "click",
            () => {

                document
                    .getElementById(
                        "replaceDocumentInput"
                    )
                    ?.click();
            }
        );


    document
        .getElementById(
            "replaceDocumentInput"
        )
        ?.addEventListener(
            "change",
            event => {

                const file =
                    event.target.files?.[0];

                if (!file) {
                    return;
                }


                if (selectedDocumentRecord) {

                    selectedDocumentRecord.dataset
                        .fileType =
                        getFileType(file);

                    selectedDocumentRecord.dataset
                        .fileSize =
                        formatFileSize(file.size);


                    const icon =
                        selectedDocumentRecord.querySelector(
                            ".q4u-document-file-icon"
                        );

                    if (icon) {
                        icon.textContent =
                            getFileType(file);
                    }
                }


                setText(
                    "previewDocumentType",
                    getFileType(file)
                );

                setText(
                    "previewDocumentSize",
                    formatFileSize(file.size)
                );


                showPortalToast(
                    "Document replaced",
                    `${file.name} has been selected as the prototype replacement file.`
                );


                event.target.value = "";
            }
        );


    document
        .querySelector(
            "[data-download-document]"
        )
        ?.addEventListener(
            "click",
            () => {

                showPortalToast(
                    "Prototype download",
                    "A production system could download the original file here."
                );
            }
        );


    document
        .getElementById(
            "confirmDeleteDocument"
        )
        ?.addEventListener(
            "click",
            completeDocumentDeletion
        );
}


function openDocumentPreview(record) {

    selectedDocumentRecord =
        record;


    setText(
        "previewDocumentName",
        record.dataset.name
    );

    setText(
        "previewPageDocumentName",
        record.dataset.name
    );

    setText(
        "previewDocumentCategory",
        record.dataset.category ??
        "DOCUMENT PREVIEW"
    );

    setText(
        "previewDocumentType",
        record.dataset.fileType
    );

    setText(
        "previewDocumentSize",
        record.dataset.fileSize
    );

    setText(
        "previewDocumentAssociation",
        record.dataset.association
    );

    setText(
        "previewPageAssociation",
        record.dataset.association
    );

    setText(
        "previewDocumentUploaded",
        record.dataset.uploaded
    );

    setText(
        "previewDocumentUploader",
        record.dataset.uploader
    );

    setText(
        "previewDocumentDescription",
        record.dataset.description
    );

    setText(
        "previewDocumentStatus",
        record.dataset.documentStatus
    );


    const icon =
        document.getElementById(
            "previewStatusIcon"
        );


    if (icon) {

        const tone =
            record.dataset.documentTone;


        if (tone === "warning") {
            icon.textContent = "!";
        }
        else if (tone === "info") {
            icon.textContent = "i";
        }
        else {
            icon.textContent = "✓";
        }
    }


    bootstrap.Modal
        .getOrCreateInstance(
            document.getElementById(
                "documentPreviewModal"
            )
        )
        .show();
}


function openDeleteDocumentModal(record) {

    setText(
        "deleteDocumentName",
        record.dataset.name ??
        record.querySelector("strong")
            ?.textContent.trim()
    );

    setText(
        "deleteDocumentAssociation",
        record.dataset.association ??
        "Prototype document"
    );


    bootstrap.Modal
        .getOrCreateInstance(
            document.getElementById(
                "deleteDocumentModal"
            )
        )
        .show();
}


function completeDocumentDeletion() {

    if (!selectedDocumentRecord) {
        return;
    }


    const isSessionDocument =
        selectedDocumentRecord.classList
            .contains(
                "session-document"
            );


    if (isSessionDocument) {

        sessionStorage.removeItem(
            q4uDocumentSessionKey
        );

        selectedDocumentRecord.remove();


        const count =
            document.getElementById(
                "documentTotalCount"
            );

        if (count) {

            count.textContent =
                Math.max(
                    0,
                    Number(count.textContent) - 1
                );
        }
    }


    const modal =
        document.getElementById(
            "deleteDocumentModal"
        );

    bootstrap.Modal
        .getInstance(modal)
        ?.hide();


    showPortalToast(
        isSessionDocument
            ? "Document removed"
            : "Prototype removal",

        isSessionDocument
            ? "The session document has been removed."
            : "A production system would remove this document after confirmation."
    );


    selectedDocumentRecord = null;
}


/* ============================================================
   SESSION DOCUMENT
   ============================================================ */

function injectSessionDocument() {

    const list =
        document.getElementById(
            "documentLibraryList"
        );

    const template =
        document.getElementById(
            "sessionDocumentTemplate"
        );

    const stored =
        sessionStorage.getItem(
            q4uDocumentSessionKey
        );


    if (
        !list ||
        !template ||
        !stored ||
        list.querySelector(
            ".session-document"
        )
    ) {
        return;
    }


    let documentData;


    try {
        documentData =
            JSON.parse(stored);
    }
    catch {
        return;
    }


    const fragment =
        template.content.cloneNode(true);

    const record =
        fragment.querySelector(
            "[data-document-record]"
        );


    record.dataset.category =
        documentData.category;

    record.dataset.vehicle =
        documentData.associationType ===
            "customer"
            ? "customer"
            : documentData.vehicleId;

    record.dataset.search =
        `${documentData.name} ${documentData.category} ${documentData.associationText} ${documentData.filename}`
            .toLowerCase();

    record.dataset.name =
        documentData.name;

    record.dataset.fileType =
        documentData.fileType;

    record.dataset.fileSize =
        documentData.fileSize;

    record.dataset.uploaded =
        "16 August 2026";

    record.dataset.uploader =
        "Ayesha Khan";

    record.dataset.association =
        documentData.associationText;

    record.dataset.description =
        "Document added during this prototype browser session.";

    record.dataset.documentStatus =
        "Stored";

    record.dataset.documentTone =
        "info";

    record.dataset.renewal =
        String(
            documentData.renewalRelevant
        );


    setNodeText(
        fragment.querySelector(
            "[data-session-file-type]"
        ),
        documentData.fileType
    );

    setNodeText(
        fragment.querySelector(
            "[data-session-name]"
        ),
        documentData.name
    );

    setNodeText(
        fragment.querySelector(
            "[data-session-category]"
        ),
        `${documentData.category} · ${documentData.fileSize}`
    );

    setNodeText(
        fragment.querySelector(
            "[data-session-association]"
        ),
        documentData.associationName
    );

    setNodeText(
        fragment.querySelector(
            "[data-session-registration]"
        ),
        documentData.associationSubtext
    );


    list.prepend(fragment);


    const count =
        document.getElementById(
            "documentTotalCount"
        );

    if (count) {

        count.textContent =
            String(
                Number(count.textContent) + 1
            );
    }
}


/* ============================================================
   UPLOAD WIZARD
   ============================================================ */

function initialiseDocumentUpload() {

    const steps =
        document.querySelectorAll(
            "[data-document-upload-step]"
        );


    if (!steps.length) {
        return;
    }


    let currentStep = 1;
    let selectedFile = null;


    const input =
        document.getElementById(
            "documentUploadInput"
        );

    const dropzone =
        document.getElementById(
            "documentDropzone"
        );

    const continueButton =
        document.getElementById(
            "documentUploadStepOneContinue"
        );


    const showStep = step => {

        currentStep = step;


        steps.forEach(panel => {

            panel.classList.toggle(
                "d-none",
                Number(
                    panel.dataset
                        .documentUploadStep
                ) !== step
            );
        });


        document
            .querySelectorAll(
                "[data-document-upload-indicator]"
            )
            .forEach(indicator => {

                const number =
                    Number(
                        indicator.dataset
                            .documentUploadIndicator
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


        if (step === 3) {
            populateDocumentReview(
                selectedFile
            );
        }


        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    };


    document
        .getElementById(
            "selectDocumentFile"
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

            if (file) {

                selectedFile = file;

                displaySelectedDocument(
                    file
                );

                if (continueButton) {
                    continueButton.disabled = false;
                }
            }
        }
    );


    if (dropzone) {

        [
            "dragenter",
            "dragover"
        ]
            .forEach(eventName => {

                dropzone.addEventListener(
                    eventName,
                    event => {

                        event.preventDefault();

                        dropzone.classList.add(
                            "dragging"
                        );
                    }
                );
            });


        [
            "dragleave",
            "drop"
        ]
            .forEach(eventName => {

                dropzone.addEventListener(
                    eventName,
                    event => {

                        event.preventDefault();

                        dropzone.classList.remove(
                            "dragging"
                        );
                    }
                );
            });


        dropzone.addEventListener(
            "drop",
            event => {

                const file =
                    event.dataTransfer
                        ?.files?.[0];

                if (!file) {
                    return;
                }


                if (!isSupportedDocument(file)) {

                    showPortalToast(
                        "Unsupported file",
                        "Please use PDF, JPG, JPEG or PNG."
                    );

                    return;
                }


                selectedFile = file;

                displaySelectedDocument(
                    file
                );


                if (continueButton) {
                    continueButton.disabled = false;
                }
            }
        );
    }


    document
        .getElementById(
            "removeUploadDocument"
        )
        ?.addEventListener(
            "click",
            () => {

                selectedFile = null;

                if (input) {
                    input.value = "";
                }


                document
                    .getElementById(
                        "documentSelectedFile"
                    )
                    ?.classList.add(
                        "d-none"
                    );


                if (continueButton) {
                    continueButton.disabled = true;
                }
            }
        );


    document
        .querySelectorAll(
            "[data-document-upload-next]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    if (
                        currentStep === 1 &&
                        !selectedFile
                    ) {
                        return;
                    }


                    if (currentStep < 3) {
                        showStep(
                            currentStep + 1
                        );
                    }
                }
            );
        });


    document
        .querySelectorAll(
            "[data-document-upload-previous]"
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


    initialiseDocumentAssociation();


    document
        .getElementById(
            "completeDocumentUpload"
        )
        ?.addEventListener(
            "click",
            () => {

                completeDocumentUpload(
                    selectedFile
                );
            }
        );


    document
        .getElementById(
            "uploadAnotherDocument"
        )
        ?.addEventListener(
            "click",
            () => {

                window.location.reload();
            }
        );
}


/* ============================================================
   ASSOCIATION
   ============================================================ */

function initialiseDocumentAssociation() {

    const vehiclePanel =
        document.getElementById(
            "vehicleDocumentAssociation"
        );


    document
        .querySelectorAll(
            'input[name="documentAssociation"]'
        )
        .forEach(input => {

            input.addEventListener(
                "change",
                () => {

                    vehiclePanel?.classList.toggle(
                        "d-none",
                        input.value ===
                        "customer"
                    );
                }
            );
        });
}


/* ============================================================
   FILE DISPLAY
   ============================================================ */

function displaySelectedDocument(file) {

    setText(
        "selectedDocumentType",
        getFileType(file)
    );

    setText(
        "selectedDocumentName",
        file.name
    );

    setText(
        "selectedDocumentMeta",
        `${formatFileSize(file.size)} · Ready`
    );


    document
        .getElementById(
            "documentSelectedFile"
        )
        ?.classList.remove(
            "d-none"
        );


    const titleInput =
        document.getElementById(
            "uploadDocumentTitle"
        );


    if (
        titleInput &&
        titleInput.value ===
        "Vehicle Registration Certificate"
    ) {

        const suggested =
            file.name
                .replace(
                    /\.[^/.]+$/,
                    ""
                )
                .replace(
                    /[_-]+/g,
                    " "
                );


        if (suggested.length > 2) {
            titleInput.value = suggested;
        }
    }
}


/* ============================================================
   REVIEW
   ============================================================ */

function populateDocumentReview(file) {

    if (!file) {
        return;
    }


    const title =
        document.getElementById(
            "uploadDocumentTitle"
        )?.value.trim() ||
        file.name;


    const category =
        document.getElementById(
            "uploadDocumentCategory"
        )?.value ??
        "Other";


    const association =
        document.querySelector(
            'input[name="documentAssociation"]:checked'
        )?.value ??
        "vehicle";


    const renewal =
        document.getElementById(
            "uploadDocumentRenewalRelevant"
        )?.checked ??
        false;


    let associationName =
        "Customer profile";

    let registration =
        "Ayesha Khan";


    if (association === "vehicle") {

        const vehicle =
            document.getElementById(
                "uploadDocumentVehicle"
            );

        const option =
            vehicle?.options[
            vehicle.selectedIndex
            ];


        if (option) {

            associationName =
                option.dataset.name;

            registration =
                `${option.dataset.registration} ${option.dataset.province}`;
        }
    }


    setText(
        "reviewDocumentFileType",
        getFileType(file)
    );

    setText(
        "reviewDocumentName",
        title
    );

    setText(
        "reviewDocumentFilename",
        file.name
    );

    setText(
        "reviewDocumentCategory",
        category
    );

    setText(
        "reviewDocumentAssociation",
        associationName
    );

    setText(
        "reviewDocumentRegistration",
        registration
    );

    setText(
        "reviewDocumentRenewal",
        renewal
            ? "Yes"
            : "No"
    );
}


/* ============================================================
   COMPLETE UPLOAD
   ============================================================ */

function completeDocumentUpload(file) {

    if (!file) {
        return;
    }


    const title =
        document.getElementById(
            "uploadDocumentTitle"
        )?.value.trim() ||
        file.name;


    const category =
        document.getElementById(
            "uploadDocumentCategory"
        )?.value ??
        "Other";


    const associationType =
        document.querySelector(
            'input[name="documentAssociation"]:checked'
        )?.value ??
        "vehicle";


    const renewalRelevant =
        document.getElementById(
            "uploadDocumentRenewalRelevant"
        )?.checked ??
        false;


    let vehicleId = "";
    let associationName =
        "Customer profile";

    let associationSubtext =
        "Ayesha Khan";

    let associationText =
        "Customer profile";


    if (
        associationType === "vehicle"
    ) {

        const selector =
            document.getElementById(
                "uploadDocumentVehicle"
            );

        const option =
            selector?.options[
            selector.selectedIndex
            ];


        if (option) {

            vehicleId =
                option.value;

            associationName =
                option.dataset.name;

            associationSubtext =
                `${option.dataset.registration} ${option.dataset.province}`;

            associationText =
                `${associationName} · ${associationSubtext}`;
        }
    }


    const documentData = {

        name:
            title,

        filename:
            file.name,

        fileType:
            getFileType(file),

        fileSize:
            formatFileSize(
                file.size
            ),

        category:
            category,

        associationType:
            associationType,

        vehicleId:
            vehicleId,

        associationName:
            associationName,

        associationSubtext:
            associationSubtext,

        associationText:
            associationText,

        renewalRelevant:
            renewalRelevant
    };


    sessionStorage.setItem(
        q4uDocumentSessionKey,
        JSON.stringify(
            documentData
        )
    );


    setText(
        "successDocumentFileType",
        documentData.fileType
    );

    setText(
        "successDocumentName",
        documentData.name
    );

    setText(
        "successDocumentAssociation",
        documentData.associationText
    );


    document
        .getElementById(
            "documentUploadReview"
        )
        ?.classList.add(
            "d-none"
        );


    document
        .getElementById(
            "documentUploadSuccess"
        )
        ?.classList.remove(
            "d-none"
        );


    showPortalToast(
        "Document added",
        `${documentData.name} has been added to the prototype library.`
    );
}


/* ============================================================
   HELPERS
   ============================================================ */

function isSupportedDocument(file) {

    const extension =
        getFileType(file);

    return [
        "PDF",
        "JPG",
        "JPEG",
        "PNG"
    ].includes(extension);
}


function getFileType(file) {

    const name =
        file?.name ?? "";

    const extension =
        name.split(".")
            .pop()
            ?.toUpperCase();

    return extension ||
        "FILE";
}


function formatFileSize(bytes) {

    if (!bytes) {
        return "0 KB";
    }


    if (bytes < 1024 * 1024) {

        return `${Math.max(
            1,
            Math.round(bytes / 1024)
        )} KB`;
    }


    return `${(
        bytes /
        (1024 * 1024)
    ).toFixed(1)} MB`;
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