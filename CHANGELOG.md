# Changelog

All notable changes to this project are documented in this file.

## [Unreleased]

### Added

- Nothing yet.

### Changed

- Nothing yet.

### Fixed

- Nothing yet.

---

## [1.6.1] - 2026-07-08

### Changed

- Cutting (Vc vs Fz) and ap × ae engagement charts now shade the polygon Pass region in very light green, matching diameter-scaled charts.

---

## [1.6.0] - 2026-07-08

### Added

- **Live chart preview** on Settings — third column beside Cutting and Engagement shows the same constraint charts as the Verifier page, updating as you edit polygons or inequalities.

### Changed

- Settings constraint editor reorganized into four cards: Graph selector, Cutting, Engagement, and Chart preview.

---

## [1.5.1] - 2026-07-08

### Fixed

- Diameter-scaled engagement charts now correctly shade the full Pass region in very light green (canvas polygon fill instead of Chart.js baseline fill).

---

## [1.5.0] - 2026-07-08

### Added

- **Free-form inequality editor** for diameter-scaled ae vs Ø and ap vs Ø — add any number of bounds such as `ae >= 0`, `ae <= 1*D`, `ap >= 0.5*D`, or constant limits like `ae <= 0.2`.

### Changed

- Diameter-scaled charts draw each inequality as a boundary line and shade the combined Pass region in very light green.
- Legacy min/max D-range settings are migrated to inequalities on load.

---

## [1.4.1] - 2026-07-08

### Added

- Results table and Excel export columns **ae check** and **ap check** after **Engage Check** — show Pass/Fail for diameter-scaled graphs and N/A for ap × ae mode.

### Changed

- **Engage Check** in diameter-scaled mode is derived from both ae check and ap check (Pass only when both Pass; Fail if either Fail).

---

## [1.4.0] - 2026-07-08

### Added

- **Diameter range editor** for diameter-scaled engagement: set ae and ap limits as simple D-multiples (e.g. `0D ≤ ae ≤ 1D`, `0.5D ≤ ap ≤ 1D`) instead of manually entering polygon vertices.
- Engagement charts in diameter-scaled mode draw min/max boundary lines and shade the Pass region; boundary points Pass.

### Changed

- Diameter-scaled evaluation uses direct ratio checks (`minD × Ø ≤ value ≤ maxD × Ø`) with inclusive boundaries.
- Settings shows a live preview of each range formula and a chart Ø-axis max control.

### Fixed

- Replaced confusing mm-vertex polygon editor for ae vs Ø and ap vs Ø with intuitive range inputs.

---

## [1.3.0] - 2026-07-07

### Added

- **Diameter-scaled engagement mode** per constraint graph: toggle **Scale with Ø** on Settings splits engagement into separate **ae vs Ø** and **ap vs Ø** polygons (mm). Ratio limits such as max ae = 1D and ap = 0.5D–1D appear as lines through the origin and scale with tool diameter.
- Verifier chart gallery shows two engagement charts (ae vs Ø, ap vs Ø) when diameter-scaled mode is enabled; existing graphs keep the single ap × ae chart.

### Changed

- Settings engagement editor labels polygon axes (ap/ae or Ø) instead of generic X/Y.

---

## [1.2.0] - 2026-07-04

### Added

- **Process Specs** column in the verifier results table and Excel import/export (after **A/C Type**).
- **Process Specs** column in Settings mapping table (before **Material**); mapping resolution, chart subtitles, and bundled `constraints.json` include the new field. Blank **Process Specs** in a mapping row matches any imported value.

### Changed

- Mapping lookup expanded from five to six fields (Process Specs + Material, Surface/Finish, Cutter, Tool, Strategy).

---

## [1.1.2] - 2026-05-29

### Added

- Import validation for **Cutter Type**, **Tool Type (Carbide/HSS/PCD)**, **Machining Type (Conventional/HSM)**, and **Finish Type (Finish / Controlled Roughing / Free Roughing)** against configured mapping rules; invalid values appear in **Remarks**.

---

## [1.1.1] - 2026-05-29

### Added

- Application version display (`v1.1.1`) in the main header, sourced from `AppVersion.cs` (keep in sync with this changelog).

### Changed

- Main header title renamed to **CAM Cutting Verifier** (title case).

---

## [1.1.0] - 2026-05-29

### Added

- Excel import scans the first 50 used rows to locate the header row, supporting title/metadata rows above the table (for example, headers starting on row 4).
- Material Type validation on import: blank or unconfigured values are flagged in the **Remarks** column.
- Sample workbook `wwwroot/samples/sample-with-preamble.xlsx` (headers on row 4) for testing padded CAM exports.

### Changed

- Renamed **Strategy Type** to **Machining Type (Conventional/HSM)** in the Excel parser, results table, and export layout.
- Moved **Machining Type** to immediately follow **Tool Type (Carbide/HSS/PCD)** in the table and export column order.
- `ExcelService` now uses `IConstraintService` to resolve known materials for import validation.

### Fixed

- Sample Excel generator now uses mapping-compatible values (`Finish`, `Controlled Roughing`, `Free Roughing`, `End Milling`, `Aluminium`).
- Blank data rows after the header row are skipped during import.

---

## [1.0.0] - 2026-05-28

### Added

- Blazor Server web app for verifying CNC milling cutting parameters against constraint graphs.
- Excel (`.xlsx`) import with flexible, case-insensitive column header matching (current and legacy CAM templates).
- Five-field mapping lookup: Material, Finish/Surface type, Cutter/Milling type, Tool type, Machining/Strategy type → figure number(s).
- Cutting check (Vc vs Fz) and engagement check (ap vs ae) using configurable polygons.
- Sortable results table with filters (All / Pass both / Any fail / Any N/A) and figure links to chart tabs.
- Interactive Chart.js scatter gallery for cutting and engagement graphs.
- Excel export of filtered results with check outcomes and remarks.
- Settings page to edit mapping rules and polygon vertices; persisted to `Data/constraints.json`.
- Bundled default aluminium constraint library and merge-on-upgrade for older persisted configs.
- Sample Excel generator (`tools/SampleExcelGen`) and downloadable `sample.xlsx`.
- GitHub Actions CI workflow (restore + Release build).

[Unreleased]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.6.1...HEAD
[1.6.1]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.6.0...v1.6.1
[1.6.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.5.1...v1.6.0
[1.5.1]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.5.0...v1.5.1
[1.5.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.4.1...v1.5.0
[1.4.1]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.4.0...v1.4.1
[1.4.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.2.0...v1.3.0
[1.2.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.1.2...v1.2.0
[1.1.2]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.1.1...v1.1.2
[1.1.1]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/releases/tag/v1.0.0
