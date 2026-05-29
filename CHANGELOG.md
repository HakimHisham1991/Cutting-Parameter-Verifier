# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Version numbers use **MAJOR.MINOR.PATCH**:

- **MAJOR** — incompatible changes (breaking API, config schema, or import/export contract)
- **MINOR** — backward-compatible functionality
- **PATCH** — backward-compatible bug fixes

## [Unreleased]

### Added

- Nothing yet.

### Changed

- Nothing yet.

### Fixed

- Nothing yet.

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

[Unreleased]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.1.2...HEAD
[1.1.2]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.1.1...v1.1.2
[1.1.1]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/HakimHisham1991/Cutting-Parameter-Verifier/releases/tag/v1.0.0
