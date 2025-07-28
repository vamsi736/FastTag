using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FastTag.Services;
using FastTag.Views;
using Nice3point.Revit.Toolkit.External;
using System.Collections.Generic;
using System.Linq;

namespace FastTag.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class AutoDimensionCommand : ExternalCommand
    {
        // ────────────────────────────────────────────────────────────────
        // 1️⃣  Simple data holder for one Grid–Column dimension pair
        // ────────────────────────────────────────────────────────────────
        private class DimPair
        {
            public Grid Grid { get; }
            public Reference GridRef { get; }
            public FamilyInstance Column { get; }
            public Reference ColRef { get; }

            public DimPair(Grid grid, Reference gridRef,
                           FamilyInstance column, Reference colRef)
            {
                Grid = grid;
                GridRef = gridRef;
                Column = column;
                ColRef = colRef;
            }
        }

        // ────────────────────────────────────────────────────────────────
        // 2️⃣  Utility – nearest grid (same orientation) to a column point
        // ────────────────────────────────────────────────────────────────
        private Grid FindNearestGrid(XYZ colPt, IEnumerable<Grid> grids)
        {
            Grid best = null;
            double bestDist = double.MaxValue;

            foreach (Grid g in grids)
            {
                if (g.Curve is not Line gLine) continue;

                XYZ foot = gLine.Project(colPt)?.XYZPoint; // null in 3D views
                if (foot == null) continue;

                double d = colPt.DistanceTo(foot);
                if (d < bestDist) { bestDist = d; best = g; }
            }
            return best;
        }

        // ────────────────────────────────────────────────────────────────
        // 3️⃣  MAIN EXECUTE
        // ────────────────────────────────────────────────────────────────
        public override void Execute()
        {
            UIDocument uiDoc = UiDocument;
            Document doc = Document;
            View view = doc.ActiveView;

            // ——— Ask user for DimensionType ————————————————————————
            var dimTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(DimensionType))
                .Cast<DimensionType>()
                .ToList();

            if (!dimTypes.Any())
            {
                TaskDialog.Show("FastTag", "No dimension types in this project.");
                return;
            }

            var picker = new SelectDimensionTypeWindow(dimTypes);
            if (picker.ShowDialog() != true || picker.SelectedDimensionType == null)
            {
                TaskDialog.Show("FastTag", "Operation cancelled.");
                return;
            }
            DimensionType dimType = picker.SelectedDimensionType;

            // ——— Collect grids & columns ————————————————————————————
            var gridCollector = new GridCollector(doc, view);
            var (verticalGrids, horizontalGrids) = gridCollector.CollectGrids();

            var colCollector = new ColumnCollector(doc, view);
            var (lrRefs, fbRefs) = colCollector.CollectColumnCenterReferences();

            // ——— Build DimPairs list ——————————————————————————————
            List<DimPair> pairs = new();

            // LR refs ↔ vertical grids
            foreach (Reference lr in lrRefs)
            {
                var col = doc.GetElement(lr.ElementId) as FamilyInstance;
                XYZ pt = (col?.Location as LocationPoint)?.Point;
                if (pt == null) continue;

                Grid g = FindNearestGrid(pt, verticalGrids);
                if (g == null) continue;

                pairs.Add(new DimPair(g, g.Curve.Reference, col, lr));
            }

            // FB refs ↔ horizontal grids
            foreach (Reference fb in fbRefs)
            {
                var col = doc.GetElement(fb.ElementId) as FamilyInstance;
                XYZ pt = (col?.Location as LocationPoint)?.Point;
                if (pt == null) continue;

                Grid g = FindNearestGrid(pt, horizontalGrids);
                if (g == null) continue;

                pairs.Add(new DimPair(g, g.Curve.Reference, col, fb));
            }

            if (!pairs.Any())
            {
                TaskDialog.Show("FastTag", "No valid grid/column pairs found.");
                return;
            }

            // ——— Preview dialog ————————————————————————————————
            string preview = string.Join("\n",
                pairs.Take(15).Select((p, i) =>
                    $"{i + 1}. Grid: {p.Grid.Name} ↔ Column: {p.Column.Name}"));

            if (pairs.Count > 15)
                preview += $"\n… (+{pairs.Count - 15} more)";

            TaskDialogResult go = TaskDialog.Show(
                "FastTag – Dimension Preview",
                $"Found {pairs.Count} grid/column pairs:\n\n{preview}\n\n" +
                "Press **OK** to place dimensions, **Cancel** to abort.",
                TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel,
                TaskDialogResult.Ok);

            if (go != TaskDialogResult.Ok) return;

            // ——— Transaction: create dimensions ————————————————————
            int created = 0, skipped = 0;

            using (Transaction tx = new Transaction(doc, "FastTag Grid Dimensions"))
            {
                tx.Start();

                foreach (DimPair p in pairs)
                {
                    try
                    {
                        if (p.Grid.Curve is not Line gLine) { skipped++; continue; }

                        XYZ colPt = (p.Column.Location as LocationPoint).Point;
                        XYZ foot = gLine.Project(colPt).XYZPoint;

                        XYZ gDir = gLine.Direction.Normalize();
                        XYZ perp = new XYZ(-gDir.Y, gDir.X, 0).Normalize();

                        double off = UnitUtils.ConvertToInternalUnits(100, UnitTypeId.Millimeters);
                        Line dimLn = Line.CreateBound(foot + perp * off, colPt + perp * off);

                        ReferenceArray ra = new ReferenceArray();
                        ra.Append(p.GridRef);
                        ra.Append(p.ColRef);

                        doc.Create.NewDimension(view, dimLn, ra, dimType);
                        created++;
                    }
                    catch
                    {
                        skipped++;
                    }
                }

                tx.Commit();
            }

            TaskDialog.Show(
                "FastTag",
                $"✅ Dimensions placed: {created}\n" +
                $"⚠️ Pairs skipped:    {skipped}");
        }
    }
}
