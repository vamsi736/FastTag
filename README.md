# FastTag 📐 – Automatic Grid-to-Column Dimensioning for Revit 2024
FastTag is a **C# Revit add-in** that scans the active view, pairs every structural
column with its nearest parallel grid, and places offset dimensions
automatically.  
The goal is to replicate “manual” grid dimensions that remain visible even when
grids are hidden via *VV*, filters, or worksets.

---

## ✨ Main Features
| Feature | Details |
|---------|---------|
| 🔍 **Smart pairing** | Columns detected via `CenterLeftRight` / `CenterFrontBack` references and matched to the closest vertical / horizontal grid. |
| 📏 **Persistent refs** | Uses `grid.Curve.Reference` (same as a manual pick) so dimensions do *not* disappear when grids are turned off. |
| 🪟 **Preview dialog** | Shows the first 15 grid-column pairs and total count; user can **Cancel** before any dimension is created. |
| 🎯 **Offset control** | Dimensions are placed on a 100 mm offset line (hard-coded for now; easy to expose in UI). |
| 📊 **Result summary** | Reports how many dimensions were placed and how many pairs were skipped. |

---

## 🛠️ Prerequisites
| Software | Version |
|----------|---------|
| Autodesk Revit | **2024** (tested on 2024.2) |
| .NET Framework | **4.8** (same as Revit 2024 API) |
| Visual Studio | 2022 or later (Community edition is fine) |

---

## 🚀 Installation
1. **Clone or download** the repo  
   ```bash
   git clone https://github.com/vamsi736/FastTag.git

Open FastTag.sln in Visual Studio.

Restore NuGet packages (Nice3Point Toolkit, etc.).

Build the solution in Release | AnyCPU.

Copy FastTag.addin to
%ProgramData%\Autodesk\Revit\Addins\2024\

Copy the built DLLs (FastTag.dll + dependencies from
bin\Release) to the same folder (or reference path set in .addin).

Launch Revit 2024 ➜ Add-ins ribbon ➜ FastTag panel ➜ Auto Dimension.
