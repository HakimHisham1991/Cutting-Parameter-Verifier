# Changelog

All notable changes to this project are documented in this file.

## [1.8.4] - 2026-07-10

### Added

- Settings graph selector — shows **Graph is not used** below the rename hint when no mapping table row references the selected graph.

---

## [1.8.3] - 2026-07-10

### Changed

- Header username and **Logout** button font size increased by 1.25×.

---

## [1.8.2] - 2026-07-10

### Changed

- Header — removed “Blazor Server · CAM parameter verification”; moved “Developed by UPECA PDC” under the app title (above version); username and **Logout** sit side by side.

### Fixed

- **Logout** — uses `/logout` navigation (static layout could not run Blazor click handlers).

---

## [1.8.1] - 2026-07-10

### Fixed

- Login form — fixed static SSR form submission (`FormName` + `[SupplyParameterFromForm]`); sign-in no longer fails silently or errors on second submit.

---

## [1.8.0] - 2026-07-09

### Added

- **Login page** (`/login`) — cookie-based sign-in matching the Tool-Master-Control theme (Zenix logo, blue primary `#2563EB`, card layout). Hardcoded accounts: `admin` / `abc12345`, `pdc` / `abc12345`. Verifier and Settings require authentication; **Sign out** in the header.

---

## [1.7.12] - 2026-07-09

### Fixed

- Verifier numeric test fields accept partial decimals while typing (e.g. `.2`); values normalize to `0.2` on Enter or click away.

---

## [1.7.11] - 2026-07-09

### Changed

- Verifier test editing — checks and charts now update only when you press **Enter** or click away from the field, instead of on every keystroke.

---

## [1.7.10] - 2026-07-09

### Changed

- Verifier test editing — **Feed Rate 100% (mm/min)** and **Speed Rate 100% (rpm)** are now editable; **Feed per Tooth [Fz]** and **Speed Vc (m/min)** are read-only and update from the program feed/speed inputs.

---

## [1.7.9] - 2026-07-09

### Fixed

- Settings mapping table and graph selector — deleting bundled mapping rows or graphs and saving no longer resurrects them on Reload; intentional removals are persisted as tombstones in `constraints.json`.

---

## [1.7.8] - 2026-07-09

### Changed

- Verifier test editing — **Cutter Type** is now editable; **Tool Diameter** and **Number of Flutes** are read-only again.

---

## [1.7.7] - 2026-07-09

### Added

- **Test editing** on the Verifier imported operations table — Process Specs, Material Type, Tool Type, Machining Type, Finish Type, Tool Diameter, Number of Flutes, ap, ae, Fz, and Vc can be edited in-session; checks and charts update live. Re-import Excel to restore source values.

---

## [1.7.6] - 2026-07-09

### Added

- Settings polygon editors (**Cutting (Vc, Fz)** and **Engagement ap × ae**) show **Error: Invalid points.** below Add point when the envelope is not closed (fewer than three distinct vertices or zero area).

---

## [1.7.5] - 2026-07-09

### Changed

- Mapping table **Status** column — removed **Error**; added **Not Used** (grey) when the graph number is missing or not defined in Graph selector. **OK** and **Duplicate** remain.

---

## [1.7.4] - 2026-07-09

### Added

- Mapping table **Status** column on Settings — **OK** (green), **Error** (red) when another row could match the same operation but maps to a different graph, **Duplicate** (yellow) when another row has the same match fields.
- Verifier filter **Fail on both** — shows rows where both Cutting (Vc, Fz) and Engagement are Fail.

---

## [1.7.3] - 2026-07-08

### Added

- **Duplicate selected graph** button on Settings — clones the current graph (polygons, engagement mode, inequalities) as `{name}_copy` and selects the new copy.

---

## [1.7.2] - 2026-07-08

### Fixed

- Renaming a graph no longer leaves the old graph number stuck in `constraints.json` — the bundled merge-on-load no longer resurrects renamed mapping/graph ids after Save.

---

## [1.7.1] - 2026-07-08

### Fixed

- Renaming a graph number no longer resets the Graph number selector to the first graph; the renamed graph stays selected.

---

## [1.7.0] - 2026-07-08

### Added

- Mapping table **per-field ON/OFF toggles** — turn a field OFF to grey it out, set it to N/A, and ignore that condition when matching rows to graphs (Pass/Fail evaluation unchanged).

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

[Unreleased]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.8.4...HEAD
[1.8.4]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.8.3...v1.8.4
[1.8.3]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.8.2...v1.8.3
[1.8.2]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.8.1...v1.8.2
[1.8.1]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.8.0...v1.8.1
[1.8.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.12...v1.8.0
[1.7.12]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.11...v1.7.12
[1.7.11]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.10...v1.7.11
[1.7.10]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.9...v1.7.10
[1.7.9]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.8...v1.7.9
[1.7.8]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.7...v1.7.8
[1.7.7]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.6...v1.7.7
[1.7.6]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.5...v1.7.6
[1.7.5]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.4...v1.7.5
[1.7.4]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.3...v1.7.4
[1.7.3]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.2...v1.7.3
[1.7.2]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.1...v1.7.2
[1.7.1]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.7.0...v1.7.1
[1.7.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.6.1...v1.7.0
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
