// Chart.js (UMD) must be loaded before this file — see App.razor.
(function () {
    const charts = {};

    function destroyAll() {
        Object.keys(charts).forEach((id) => {
            try {
                charts[id].destroy();
            } catch { /* ignore */ }
            delete charts[id];
        });
    }

    function scrollToFigure(anchorId) {
        const el = document.getElementById(anchorId);
        if (!el) return;
        try {
            el.scrollIntoView({ behavior: "smooth", block: "start" });
        } catch {
            el.scrollIntoView();
        }
    }

    function scrollTabIntoView(id) {
        const el = document.getElementById(id);
        if (!el) return;
        try {
            el.scrollIntoView({ behavior: "smooth", block: "nearest", inline: "nearest" });
        } catch {
            try { el.scrollIntoView(); } catch { /* ignore */ }
        }
    }

    function closeRing(points) {
        if (!points || points.length === 0) return [];
        const out = points.slice();
        const a = out[0];
        const b = out[out.length - 1];
        if (a.x !== b.x || a.y !== b.y) out.push({ x: a.x, y: a.y });
        return out;
    }

    function buildScatterChart(canvasId, title, xLabel, yLabel, polygon, passPoints, failPoints, naPoints, mappingContext) {
        const el = document.getElementById(canvasId);
        if (!el) return;
        if (charts[canvasId]) {
            charts[canvasId].destroy();
            delete charts[canvasId];
        }

        const ring = closeRing(polygon || []);
        const boundary = ring.map((p) => ({ x: p.x, y: p.y }));

        const datasets = [
            {
                type: "line",
                label: "Boundary",
                data: boundary,
                borderColor: "rgba(60,60,60,0.95)",
                borderWidth: 2,
                fill: false,
                pointRadius: 0,
                tension: 0
            },
            {
                type: "scatter",
                label: "Pass",
                data: passPoints || [],
                backgroundColor: "rgba(34,170,68,0.9)",
                pointRadius: 4
            },
            {
                type: "scatter",
                label: "Fail",
                data: failPoints || [],
                backgroundColor: "rgba(204,34,34,0.9)",
                pointRadius: 4
            },
            {
                type: "scatter",
                label: "N/A",
                data: naPoints || [],
                backgroundColor: "rgba(120,120,120,0.75)",
                pointRadius: 3
            }
        ];

        const sub = typeof mappingContext === "string" && mappingContext.trim().length > 0 ? mappingContext.trim() : "";

        charts[canvasId] = new Chart(el, {
            data: { datasets },
            type: "scatter",
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: "nearest", intersect: true },
                plugins: {
                    legend: { display: true, position: "bottom" },
                    title: { display: true, text: title, font: { size: 12, weight: "600" }, padding: { bottom: sub ? 2 : 6 } },
                    subtitle: {
                        display: !!sub,
                        text: sub,
                        color: "#495057",
                        font: { size: 10 },
                        padding: { top: 0, bottom: 8 }
                    },
                    tooltip: {
                        displayColors: false,
                        callbacks: {
                            title: function () {
                                return null;
                            },
                            label: function (ctx) {
                                if (ctx.dataset.label === "Boundary") return "";
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
                        title: { display: true, text: xLabel }
                    },
                    y: {
                        type: "linear",
                        title: { display: true, text: yLabel }
                    }
                }
            }
        });
    }

    window.cpvCharts = {
        renderAll: function (spec) {
            destroyAll();
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
                    p.mappingContext
                );
                buildScatterChart(
                    p.engCanvasId,
                    `Engagement: ${g}`,
                    "ap (mm)",
                    "ae (mm)",
                    p.engagement.polygon,
                    p.engagement.pass,
                    p.engagement.fail,
                    p.engagement.na,
                    p.mappingContext
                );
            });
        },
        destroyAll: destroyAll,
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
