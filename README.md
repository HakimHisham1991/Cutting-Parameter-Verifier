# Cutting Parameter Verifier

A Blazor web application for verifying CNC milling cutting parameters against configurable constraint graphs. Upload a CAM Excel export, and the app maps each operation to the correct figure(s), checks whether cutting and engagement parameters fall inside approved polygons, and visualizes results in interactive charts.

Repository: [github.com/HakimHisham1991/Cutting-Parameter-Verifier](https://github.com/HakimHisham1991/Cutting-Parameter-Verifier)

---

## Table of contents

- [Overview](#overview)
- [Features](#features)
- [How verification works](#how-verification-works)
- [Technology stack](#technology-stack)
- [Requirements](#requirements)
- [Getting started](#getting-started)
- [Using the application](#using-the-application)
  - [Verifier page (home)](#verifier-page-home)
  - [Settings page](#settings-page)
- [Excel import format](#excel-import-format)
- [Exporting results](#exporting-results)
- [Configuration file](#configuration-file)
- [Sample data and tools](#sample-data-and-tools)
- [Project structure](#project-structure)
- [Development](#development)
- [Deployment notes](#deployment-notes)

---

## Overview

Manufacturing teams often define acceptable cutting-parameter envelopes as 2D constraint graphs (polygons) keyed by material, surface finish, cutter type, and strategy. This application automates the check that each CAM operation’s parameters lie within those envelopes.

For every imported row, the app:

1. **Parses** the Excel row into a structured operation record.
2. **Maps** the row to one or more constraint graph identifiers (figure numbers) using a five-field lookup table.
3. **Evaluates** two independent checks:
   - **Parameter check (cutting)** — is the point `(Vc, Fz)` inside the cutting polygon?
   - **Engagement check** — is the point `(ap, ae)` inside the engagement polygon?
4. **Displays** Pass / Fail / N/A in a sortable results table and plots points on Chart.js scatter charts.

Configuration (mapping rules and polygon vertices) is stored in `Data/constraints.json` and can be edited from the **Settings** page without redeploying the app.

---

## Features

| Area | Capability |
|------|------------|
| **Import** | Upload `.xlsx` workbooks; flexible column header matching (legacy and current CAM templates) |
| **Validation** | Rows missing required fields are flagged invalid with remarks; they receive N/A for checks |
| **Mapping** | Case-insensitive five-tuple match: Material, Surface/Finish type, Milling/Cutter type, Tool type, Strategy type |
| **Multi-graph** | One operation can match multiple figures; per-figure Pass/Fail shown comma-separated |
| **Charts** | Tabbed gallery with cutting (Vc vs Fz) and engagement (ap vs ae) charts per figure |
| **Table** | Sortable columns, row filters (All / Pass both / Any fail / Any N/A), click figure links to jump to chart |
| **Export** | Download filtered results as Excel with check outcomes and remarks |
| **Settings** | Edit mapping table and polygon vertices in the browser; changes re-evaluate the current session |

---

## How verification works

```mermaid
flowchart LR
    A[Excel upload] --> B[Parse rows]
    B --> C{Row valid?}
    C -->|No| D[N/A checks + remarks]
    C -->|Yes| E[Mapping rules]
    E --> F{Figure(s) found?}
    F -->|No| D
    F -->|Yes| G[For each figure]
    G --> H[Cutting polygon<br/>Vc × Fz]
    G --> I[Engagement polygon<br/>ap × ae]
    H --> J[Param Check]
    I --> K[Engage Check]
    J --> L[Aggregate Pass/Fail/N/A]
    K --> L
    L --> M[Table + charts]
```

### Cutting check (Parameter check)

- **Axes:** X = surface speed `Vc` (m/min), Y = feed per tooth `Fz` (mm/tooth)
- **Rule:** Pass if the point lies inside (or on the boundary of) the cutting polygon for the matched figure

### Engagement check

- **Axes:** X = axial depth of cut `ap` (mm), Y = radial depth of cut `ae` (mm)
- **Rule:** Pass if the point lies inside (or on the boundary of) the engagement polygon
- **Note:** Polygon vertices are stored as `(X = ae, Y = ap)` in JSON; charts and evaluation normalize to `(ap, ae)` for display and point-in-polygon tests

### Status aggregation

When multiple figures match one row:

- **Fail** if any figure fails
- Else **N/A** if any figure is N/A
- Else **Pass** if all figures pass

Invalid rows (missing required import fields) or rows with no mapping match always show **N/A** for both checks.

---

## Technology stack

| Component | Technology |
|-----------|------------|
| UI | ASP.NET Core **Blazor Server** (interactive server render mode) |
| Excel I/O | [ClosedXML](https://github.com/ClosedXML/ClosedXML) |
| Charts | [Chart.js](https://www.chartjs.org/) 4.x (CDN) |
| Styling | Bootstrap 5 |
| Target framework | **.NET 10** (`net10.0`) |

---

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (the project targets `net10.0`; retarget to `net8.0` in the `.csproj` if you only have the .NET 8 SDK installed)
- A modern web browser
- Optional: Visual Studio 2022 or VS Code with C# Dev Kit

---

## Getting started

### Clone and run

```bash
git clone https://github.com/HakimHisham1991/Cutting-Parameter-Verifier.git
cd Cutting-Parameter-Verifier
dotnet restore CuttingParameterVerifier.sln
dotnet run --project CuttingParameterVerifier/CuttingParameterVerifier.csproj
```

The app listens on:

| Profile | URL |
|---------|-----|
| HTTP | http://localhost:5010 |
| HTTPS | https://localhost:7003 |

Launch profiles are defined in `CuttingParameterVerifier/Properties/launchSettings.json`.

### Build

```bash
dotnet build CuttingParameterVerifier.sln --configuration Release
```

### Publish

```bash
dotnet publish CuttingParameterVerifier/CuttingParameterVerifier.csproj -c Release -o ./publish-output
```

Ensure `Data/constraints.json` is writable on the host if you use the Settings page in production (the file is created on first run if missing).

---

## Using the application

### Verifier page (home)

Route: `/`

1. **Upload Excel** — click **Upload Excel (.xlsx)** and select a workbook. Progress is shown during read, parse, and evaluation.
2. **Download sample** — use **Download sample sheet** to get `wwwroot/samples/sample.xlsx` with the expected column layout.
3. **Review results** — the table lists every imported operation with check outcomes. Invalid rows are highlighted.
4. **Filter** — narrow rows by Pass on both, Any fail, or Any N/A.
5. **Sort** — click any column header to sort ascending/descending.
6. **Charts** — the right-hand panel shows constraint charts for all configured figures. Click a **Figure No.** link in the table to select and scroll to that chart tab.
7. **Export results** — **Export results** downloads the currently filtered/sorted rows as `cutting-parameter-results.xlsx`.

#### Results table columns

| Column | Source / meaning |
|--------|----------------|
| No. | Row index from Excel |
| A/C Type | Aircraft/program type |
| Part Number | Part identifier |
| Material Type | Material (used in mapping) |
| Tool Ref. Number | Tool reference |
| Cutter Description | Tool name / description |
| Cutter Type | Milling/cutter type (used in mapping) |
| Tool Type (Carbide/HSS/PCD) | Tool material (used in mapping) |
| Finish Type | Surface/finish type (used in mapping) |
| Tool Diameter (mm) | Tool diameter |
| Number of Flutes (teeth) | Flute count |
| Feed Rate 100% (mm/min) | Program feed at 100% |
| Speed Rate 100% (rpm) | Program speed at 100% |
| Axial (ap) D.O.C (mm) | Axial depth of cut |
| Radial (ae) D.O.C (mm) | Radial depth of cut |
| Feed per Tooth [Fz] (mm/tooth) | Feed per tooth |
| Speed Vc (m/min) | Surface speed |
| Strategy Type | Machining strategy (used in mapping) |
| Operation Name | CAM operation name |
| Figure No. | Matched constraint graph identifier(s) |
| Param Check | Cutting (Vc–Fz) outcome |
| Engage Check | Engagement (ap–ae) outcome |
| Remarks | Validation errors or empty if valid |

### Settings page

Route: `/settings`

Three areas:

1. **Mapping table** — rows linking `(Material, Surface type, Milling type, Tool type, Strategy type)` → `Graph number`. Matching is case-insensitive. Duplicate five-tuples with different graph numbers are allowed (one operation can map to multiple figures).
2. **Graph selector** — add, delete, or rename figure identifiers (e.g. `3.2.2.4.1.2`).
3. **Constraint polygons** — edit vertex lists for:
   - **Cutting (Vc, Fz)** — X = Vc, Y = Fz
   - **Engagement (ap, ae)** — X = ae, Y = ap in the editor (see note above)

Click **Save configuration** to persist to `Data/constraints.json`. The active session re-evaluates immediately. Use **Reload** to discard unsaved edits and read from disk.

---

## Excel import format

The importer reads the **first worksheet** and maps columns by header text (case-insensitive, substring matching). Legacy header names are still accepted.

### Expected columns (current CAM template)

| Column header | Required for checks | Notes |
|---------------|---------------------|-------|
| No. | No | Row number |
| A/C Type | No | |
| Part Number | No | |
| Material Type | **Yes** | Also accepts `Material` |
| Tool Ref. Number | No | |
| Cutter Description | No | Legacy: `Tool Name` |
| Cutter Type | **Yes** | Legacy: `Milling Type` |
| Tool Type (Carbide/HSS/PCD) | **Yes** | |
| Finish Type (Finish / Controlled Roughing / Free Roughing) | **Yes** | Legacy: `Surface Type` |
| Tool Diameter (mm) | No | |
| Number of Flutes (teeth) | No | |
| Feed Rate 100% (mm/min) | No | Legacy: `Vf (mm/min)` |
| Speed Rate 100% (rpm) | No | Legacy: `n (RPM)` |
| Axial (ap) D.O.C (mm) | **Yes** | |
| Radial (ae) D.O.C (mm) | **Yes** | |
| Feed per Tooth [Fz] (mm/tooth) | **Yes** | |
| Speed Vc (m/min) | **Yes** | Legacy: `Surface Speed, Vc (m/min)` |
| Justification | No | Imported but not shown in results table |
| Ramp Angle (Deg) | No | |
| Approach / Plunge Feed (mm/min) | No | |
| Strategy Type | **Yes** | |
| Operation Name | No | |

Rows failing required-field validation are imported but marked invalid; **Remarks** lists missing fields and both checks show **N/A**.

A reference workbook ships at `CuttingParameterVerifier/wwwroot/samples/sample.xlsx`.

---

## Exporting results

**Export results** writes an Excel file containing source columns plus:

- Figure No.
- Parameter In Spec (Param Check)
- Engagement In Spec (Engage Check)
- Remarks

When multiple figures apply, Pass/Fail values are comma-separated in the same order as Figure No.

---

## Configuration file

Path: `CuttingParameterVerifier/Data/constraints.json`

```json
{
  "mappingRules": [
    {
      "material": "Aluminium",
      "surfaceType": "Finish",
      "millingType": "End Milling",
      "toolType": "Carbide",
      "strategyType": "Conventional",
      "graphNumber": "3.2.2.4.1.2"
    }
  ],
  "graphs": [
    {
      "graphNumber": "3.2.2.4.1.2",
      "cuttingPolygon": [
        { "x": 100, "y": 0.05 },
        { "x": 800, "y": 0.05 },
        { "x": 800, "y": 0.4 },
        { "x": 100, "y": 0.4 }
      ],
      "engagementPolygon": [
        { "x": 0.5, "y": 0.5 },
        { "x": 4.0, "y": 0.5 },
        { "x": 4.0, "y": 3.0 },
        { "x": 0.5, "y": 3.0 }
      ]
    }
  ]
}
```

| Field | Description |
|-------|-------------|
| `mappingRules` | Lookup from CAM context to figure number |
| `graphs` | Polygon definitions per figure |
| `cuttingPolygon` | Vertices for Vc (X) vs Fz (Y) boundary |
| `engagementPolygon` | Vertices stored as X = ae, Y = ap |

On first run, if the file is missing, a bundled default is written to disk. When upgrading, missing graphs or rules from the embedded bundle are merged into an older persisted file automatically.

`Data/sample-constraints.json` is a full reference copy used by the sample generator tool.

---

## Sample data and tools

### Sample Excel generator

Regenerates the sample workbook and reference constraints JSON:

```bash
dotnet run --project tools/SampleExcelGen/SampleExcelGen.csproj -c Release
```

Outputs:

- `CuttingParameterVerifier/wwwroot/samples/sample.xlsx`
- `CuttingParameterVerifier/Data/sample-constraints.json`

---

## Project structure

```
Cutting-Parameter-Verifier/
├── CuttingParameterVerifier/          # Main Blazor web app
│   ├── Components/
│   │   ├── Pages/
│   │   │   ├── Verifier.razor         # Home: import, table, export
│   │   │   └── Settings.razor         # Mapping + polygon editor
│   │   ├── Verifier/
│   │   │   ├── GraphGallery.razor     # Chart tabs
│   │   │   └── ChartSpecBuilder.cs    # Chart.js payload builder
│   │   └── Settings/
│   │       └── PointsEditor.razor     # Polygon vertex editor
│   ├── Data/
│   │   └── constraints.json           # Runtime configuration (persisted)
│   ├── Models/                        # DTOs and domain types
│   ├── Services/
│   │   ├── ExcelService.cs            # Import / export
│   │   ├── MappingService.cs          # Five-tuple → figure lookup
│   │   ├── EvaluationService.cs       # Pass/Fail evaluation
│   │   ├── ConstraintService.cs       # Load/save constraints.json
│   │   ├── ConstraintEval.cs          # Point-in-polygon checks
│   │   └── CuttingSessionState.cs     # Per-session import + results
│   └── wwwroot/
│       ├── samples/sample.xlsx
│       └── js/verifierCharts.js       # Chart.js integration
├── tools/SampleExcelGen/              # Sample workbook generator
├── .github/workflows/ci.yml           # Build on push/PR
└── CuttingParameterVerifier.sln
```

---

## Development

### CI

GitHub Actions workflow **CI** (`.github/workflows/ci.yml`) runs on every push and pull request:

```bash
dotnet restore CuttingParameterVerifier.sln
dotnet build CuttingParameterVerifier.sln --configuration Release --no-restore
```

### VS Code / Visual Studio

Open `CuttingParameterVerifier.sln`. Launch configurations are in `.vscode/launch.json`.

### Key services (DI)

Registered in `Program.cs`:

- `IMappingService` → `MappingService`
- `IConstraintService` → `ConstraintService`
- `IEvaluationService` → `EvaluationService`
- `IExcelService` → `ExcelService`
- `CuttingSessionState` (scoped per Blazor circuit)

---

## Deployment notes

- **Blazor Server** maintains a SignalR connection per user; scale-out requires sticky sessions or a compatible SignalR backplane.
- **constraints.json** must be writable if operators use Settings in production.
- Static assets (Bootstrap, Chart.js CDN) require network access for the Chart.js script unless you self-host it.
- The app excludes `publish-output/` from compilation to avoid Blazor static asset conflicts during publish.

---

## Troubleshooting

| Symptom | Likely cause |
|---------|----------------|
| All checks N/A | Mapping rule does not match row text (check spelling/casing of Material, Finish Type, etc.) |
| Row marked invalid | Missing Vc, Fz, ap, ae, or required context fields — see Remarks column |
| No charts | Empty `graphs` in constraints.json |
| Import shows 0 rows | Wrong worksheet or missing header row |
| Settings not persisting | `Data/` folder not writable on the server |

---

For questions or contributions, open an issue on the [GitHub repository](https://github.com/HakimHisham1991/Cutting-Parameter-Verifier).
