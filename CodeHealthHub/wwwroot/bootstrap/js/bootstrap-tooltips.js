window.enableTooltips = () => {
    document
        .querySelectorAll('[data-bs-toggle="tooltip"]')
        .forEach(el => {
            if (!bootstrap.Tooltip.getInstance(el)) {
                new bootstrap.Tooltip(el);
            }
        });
};
