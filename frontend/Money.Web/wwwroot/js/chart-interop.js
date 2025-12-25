window.moneyChart = {
    instances: {},
    resolveCssVariables: obj => {
        if (!obj) return;

        const style = getComputedStyle(document.body);

        const process = o => {
            for (const key in o) {
                if (typeof o[key] === 'string' && o[key].startsWith('var(--')) {
                    const varName = o[key].substring(4, o[key].length - 1);
                    const color = style.getPropertyValue(varName).trim();
                    if (color) {
                        o[key] = color;
                    }
                } else if (typeof o[key] === 'object' && o[key] !== null) {
                    process(o[key]);
                }
            }
        };

        process(obj);
    },
    create: (elementId, config) => {
        const element = document.getElementById(elementId);
        if (!element) return;

        const ctx = element.getContext('2d');
        window.moneyChart.resolveCssVariables(config);
        window.moneyChart.instances[elementId] = new Chart(ctx, config);
    },
    update: (elementId, config) => {
        const chart = window.moneyChart.instances[elementId];
        if (chart) {
            window.moneyChart.resolveCssVariables(config);

            chart.data = config.data;

            if (config.options) {
                chart.options = config.options;
            }

            chart.update();
        }
    },
    destroy: elementId => {
        const chart = window.moneyChart.instances[elementId];
        if (chart) {
            chart.destroy();
            delete window.moneyChart.instances[elementId];
        }
    }
};
