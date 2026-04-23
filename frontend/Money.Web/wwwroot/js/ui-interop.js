window.moneyUi = {
    scrollIntoView(elementId) {
        const el = document.getElementById(elementId);
        if (el && typeof el.scrollIntoView === 'function') {
            el.scrollIntoView({block: 'nearest', inline: 'nearest'});
        }
    }
};
