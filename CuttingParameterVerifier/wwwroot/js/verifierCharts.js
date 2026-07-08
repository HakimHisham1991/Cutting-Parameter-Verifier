// Chart.js (UMD) must be loaded before this file — see App.razor.
(function () {
    /** Match ~75% UI scale (see wwwroot/app.css html { font-size: 75% }). */
    const CPV_UI_SCALE = 0.75;

    const PASS_REGION_FILL = "rgba(34,170,68,0.12)";
    const PASS_REGION_STROKE = "rgba(34,120,50,0.30)";

    const charts = {};
    const previewCharts = {};

    function destroyRegistry(registry) {
        Object.keys(registry).forEach((id) => {
            try {
                registry[id].destroy();
            } catch { /* ignore */ }
            delete registry[id];
        });
    }

    function destroyAll() {
        destroyRegistry(charts);
    }

    function destroyPreview() {
        destroyRegistry(previewCharts);
    }

    function renderPanels(spec, registry) {
        const obj = typeof spec === "string" ? JSON.parse(spec) : spec;
        if (!obj || !obj.panels) return;
        obj.panels.forEach((p) => {
            const g = p.graphNumber;
            buildScatterChart(
                p.cutCanvasId,
                `Cutting: ${g}`,
                "Vc (m/min)",
                "Fz (mm)",
                p.cutting.polygon,
                p.cutting.pass,
                p.cutting.fail,
                p.cutting.na,
                p.mappingContext,
                { fillPassRegion: true, registry }
            );
            if (p.engagementMode === "diameterScaled") {
                buildScatterChart(
                    p.engAeCanvasId,
                    `Engagement ae vs Ø: ${g}`,
                    "Ø (mm)",
                    "ae (mm)",
                    p.engagementAeVsDiameter.polygon,
                    p.engagementAeVsDiameter.pass,
                    p.engagementAeVsDiameter.fail,
                    p.engagementAeVsDiameter.na,
                    p.mappingContext,
                    {
                        inequalityLines: p.engagementAeVsDiameter.inequalityLines,
                        fillPassRegion: true,
                        registry
                    }
                );
                buildScatterChart(
                    p.engApCanvasId,
                    `Engagement ap vs Ø: ${g}`,
                    "Ø (mm)",
                    "ap (mm)",
                    p.engagementApVsDiameter.polygon,
                    p.engagementApVsDiameter.pass,
                    p.engagementApVsDiameter.fail,
                    p.engagementApVsDiameter.na,
                    p.mappingContext,
                    {
                        inequalityLines: p.engagementApVsDiameter.inequalityLines,
                        fillPassRegion: true,
                        registry
                    }
                );
            } else {
                buildScatterChart(
                    p.engCanvasId,
                    `Engagement: ${g}`,
                    "ap (mm)",
                    "ae (mm)",
                    p.engagement.polygon,
                    p.engagement.pass,
                    p.engagement.fail,
                    p.engagement.na,
                    p.mappingContext,
                    { fillPassRegion: true, registry }
                );
            }
        });
    }
    const passRegionFillPlugin = {
        id: "cpvPassRegionFill",
        beforeDatasetsDraw(chart, _args, options) {
            const poly = options && options.polygon;
            if (!poly || poly.length < 3) return;
            const xS = chart.scales.x;
            const yS = chart.scales.y;
            if (!xS || !yS) return;

            const ctx = chart.ctx;
            ctx.save();
            ctx.beginPath();
            poly.forEach((p, i) => {
                const px = xS.getPixelForValue(p.x);
                const py = yS.getPixelForValue(p.y);
                if (i === 0) ctx.moveTo(px, py);
                else ctx.lineTo(px, py);
            });
            ctx.closePath();
            ctx.fillStyle = options.color || PASS_REGION_FILL;
            ctx.fill();
            if (options.stroke !== false) {
                ctx.strokeStyle = options.strokeColor || PASS_REGION_STROKE;
                ctx.lineWidth = Math.max(1, 1 * CPV_UI_SCALE);
                ctx.stroke();
            }
            ctx.restore();
        }
    };

    if (typeof Chart !== "undefined" && Chart.register && !Chart.registry.plugins.get("cpvPassRegionFill")) {
        Chart.register(passRegionFillPlugin);
    }

    function galleryScrollPanel() {
        return document.querySelector(".graph-gallery .card-body.graph-scroll");
    }

    /** Scroll vertically inside the chart card only — never scroll the document (avoids jumping away from the table). */
    function scrollIntoContainerVert(el, container, smooth) {
        if (!el || !container) return;
        const pad = 8;
        const cr = container.getBoundingClientRect();
        const er = el.getBoundingClientRect();
        let dy = 0;
        if (er.top < cr.top + pad) dy -= Math.ceil(cr.top + pad - er.top);
        else if (er.bottom > cr.bottom - pad) dy += Math.ceil(er.bottom - (cr.bottom - pad));
        if (dy === 0) return;
        const target = container.scrollTop + dy;
        try {
            if (smooth && typeof container.scrollTo === "function") {
                container.scrollTo({ top: target, behavior: "smooth" });
            } else {
                container.scrollTop = target;
            }
        } catch {
            container.scrollTop = target;
        }
    }

    /** Scroll horizontally inside the tab strip only. */
    function scrollIntoContainerHoriz(el, container, smooth) {
        if (!el || !container) return;
        const pad = 8;
        const cr = container.getBoundingClientRect();
        const er = el.getBoundingClientRect();
        let dx = 0;
        if (er.left < cr.left + pad) dx -= Math.ceil(cr.left + pad - er.left);
        else if (er.right > cr.right - pad) dx += Math.ceil(er.right - (cr.right - pad));
        if (dx === 0) return;
        const target = container.scrollLeft + dx;
        try {
            if (smooth && typeof container.scrollTo === "function") {
                container.scrollTo({ left: target, behavior: "smooth" });
            } else {
                container.scrollLeft = target;
            }
        } catch {
            container.scrollLeft = target;
        }
    }

    function scrollToFigure(anchorId) {
        const panel = galleryScrollPanel();
        const el = document.getElementById(anchorId);
        if (!panel || !el || !panel.contains(el)) return;
        scrollIntoContainerVert(el, panel, true);
    }

    function scrollTabIntoView(id) {
        const el = document.getElementById(id);
        if (!el) return;
        const tabs = el.closest("ul.nav-tabs");
        if (tabs) {
            scrollIntoContainerHoriz(el, tabs, true);
            return;
        }
        const panel = galleryScrollPanel();
        if (panel && panel.contains(el)) scrollIntoContainerVert(el, panel, true);
    }

    function closeRing(points) {
        if (!points || points.length === 0) return [];
        const out = points.slice();
        const a = out[0];
        const b = out[out.length - 1];
        if (a.x !== b.x || a.y !== b.y) out.push({ x: a.x, y: a.y });
        return out;
    }

    function uniquePolygonRing(points) {
        if (!points || points.length === 0) return [];
        const ring = closeRing(points);
        if (ring.length > 1) {
            const a = ring[0];
            const b = ring[ring.length - 1];
            if (a.x === b.x && a.y === b.y) ring.pop();
        }
        return ring;
    }

    function buildScatterChart(canvasId, title, xLabel, yLabel, polygon, passPoints, failPoints, naPoints, mappingContext, chartOptions) {
        const el = document.getElementById(canvasId);
        if (!el) return;
        const opts = chartOptions || {};
        const registry = opts.registry || charts;
        if (registry[canvasId]) {
            registry[canvasId].destroy();
            delete registry[canvasId];
        }

        const inequalityLines = opts.inequalityLines || [];
        const fillPassRegion = !!opts.fillPassRegion;
        const passRegionPoly = uniquePolygonRing(polygon || []);
        const boundary = closeRing(polygon || []).map((p) => ({ x: p.x, y: p.y }));

        const datasets = [];

        if (fillPassRegion && passRegionPoly.length >= 3) {
            datasets.push({
                type: "line",
                label: "Pass region",
                data: [],
                borderColor: PASS_REGION_STROKE,
                backgroundColor: PASS_REGION_FILL,
                borderWidth: 0,
                fill: false,
                pointRadius: 0,
                order: 4
            });
        }

        if (boundary.length > 0 && inequalityLines.length === 0) {
            datasets.push({
                type: "line",
                label: "Boundary",
                data: boundary,
                borderColor: "rgba(60,60,60,0.95)",
                borderWidth: Math.max(1, 2 * CPV_UI_SCALE),
                fill: false,
                pointRadius: 0,
                tension: 0,
                order: 3
            });
        }

        inequalityLines.forEach((line, idx) => {
            const pts = (line.points || []).map((p) => ({ x: p.x, y: p.y }));
            if (pts.length < 2) return;
            datasets.push({
                type: "line",
                label: line.label || `Bound ${idx + 1}`,
                data: pts,
                borderColor: "rgba(60,60,60,0.75)",
                borderWidth: Math.max(1, 1.5 * CPV_UI_SCALE),
                borderDash: idx % 2 === 0 ? [5 * CPV_UI_SCALE, 4 * CPV_UI_SCALE] : [],
                fill: false,
                pointRadius: 0,
                tension: 0,
                order: 2
            });
        });

        datasets.push(
            {
                type: "scatter",
                label: "Pass",
                data: passPoints || [],
                backgroundColor: "rgba(34,170,68,0.9)",
                pointRadius: Math.max(2, 4 * CPV_UI_SCALE),
                order: 0
            },
            {
                type: "scatter",
                label: "Fail",
                data: failPoints || [],
                backgroundColor: "rgba(204,34,34,0.9)",
                pointRadius: Math.max(2, 4 * CPV_UI_SCALE),
                order: 0
            },
            {
                type: "scatter",
                label: "N/A",
                data: naPoints || [],
                backgroundColor: "rgba(120,120,120,0.75)",
                pointRadius: Math.max(2, 3 * CPV_UI_SCALE),
                order: 0
            }
        );

        const sub = typeof mappingContext === "string" && mappingContext.trim().length > 0 ? mappingContext.trim() : "";

        registry[canvasId] = new Chart(el, {
            data: { datasets },
            type: "scatter",
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: "nearest", intersect: true },
                plugins: {
                    cpvPassRegionFill: fillPassRegion && passRegionPoly.length >= 3
                        ? {
                            polygon: passRegionPoly,
                            color: PASS_REGION_FILL,
                            strokeColor: PASS_REGION_STROKE,
                            stroke: inequalityLines.length > 0
                        }
                        : { polygon: [] },
                    legend: {
                        display: true,
                        position: "bottom",
                        labels: {
                            font: { size: 12 * CPV_UI_SCALE },
                            generateLabels(chart) {
                                const items = Chart.defaults.plugins.legend.labels.generateLabels(chart);
                                return items.map((item) => {
                                    const ds = chart.data.datasets[item.datasetIndex];
                                    if (ds && ds.label === "Pass region") {
                                        item.fillStyle = PASS_REGION_FILL;
                                        item.strokeStyle = PASS_REGION_FILL;
                                        item.lineWidth = 0;
                                    }
                                    return item;
                                });
                            }
                        }
                    },
                    title: {
                        display: true,
                        text: title,
                        font: { size: 12 * CPV_UI_SCALE, weight: "600" },
                        padding: { bottom: sub ? 2 * CPV_UI_SCALE : 6 * CPV_UI_SCALE }
                    },
                    subtitle: {
                        display: !!sub,
                        text: sub,
                        color: "#495057",
                        font: { size: 10 * CPV_UI_SCALE },
                        padding: { top: 0, bottom: 8 * CPV_UI_SCALE }
                    },
                    tooltip: {
                        displayColors: false,
                        bodyFont: { size: 11 * CPV_UI_SCALE },
                        titleFont: { size: 11 * CPV_UI_SCALE },
                        callbacks: {
                            title: function () {
                                return null;
                            },
                            label: function (ctx) {
                                const lbl = ctx.dataset.label || "";
                                if (lbl === "Pass region" || lbl === "Boundary") return "";
                                const raw = ctx.raw;
                                if (raw && typeof raw.tooltip === "string") return raw.tooltip;
                                const lx = ctx.dataset.label || "";
                                const x = raw && typeof raw.x === "number" ? raw.x : "";
                                const y = raw && typeof raw.y === "number" ? raw.y : "";
                                return `${lx}: (${x}, ${y})`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        type: "linear",
                        title: { display: true, text: xLabel, font: { size: 12 * CPV_UI_SCALE } },
                        ticks: { font: { size: 12 * CPV_UI_SCALE } }
                    },
                    y: {
                        type: "linear",
                        title: { display: true, text: yLabel, font: { size: 12 * CPV_UI_SCALE } },
                        ticks: { font: { size: 12 * CPV_UI_SCALE } }
                    }
                }
            }
        });
    }

    window.cpvCharts = {
        renderAll: function (spec) {
            destroyAll();
            renderPanels(spec, charts);
        },
        renderPreview: function (spec) {
            destroyPreview();
            renderPanels(spec, previewCharts);
        },
        destroyAll: destroyAll,
        destroyPreview: destroyPreview,
        scrollToFigure: scrollToFigure,
        scrollTabIntoView: scrollTabIntoView
    };

    window.cpvFiles = {
        downloadBase64: function (filename, base64) {
            const bytes = Uint8Array.from(atob(base64), (c) => c.charCodeAt(0));
            const blob = new Blob([bytes], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
            const url = URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = filename;
            a.click();
            URL.revokeObjectURL(url);
        }
    };
})();
