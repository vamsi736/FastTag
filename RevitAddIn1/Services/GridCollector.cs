using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace FastTag.Services
{
    public class GridCollector
    {
        private readonly Document _doc;
        private readonly View _view;

        public GridCollector(Document doc, View view)
        {
            _doc = doc;
            _view = view;
        }

        public (List<Grid> verticalGrids, List<Grid> horizontalGrids) CollectGrids()
        {
            List<Grid> verticalGrids = new List<Grid>();
            List<Grid> horizontalGrids = new List<Grid>();

            var grids = new FilteredElementCollector(_doc, _view.Id)
                .OfClass(typeof(Grid))
                .Cast<Grid>();

            foreach (var grid in grids)
            {
                if (grid.Curve is Line line)
                {
                    XYZ dir = line.Direction;
                    if (System.Math.Abs(dir.X) > System.Math.Abs(dir.Y))
                        horizontalGrids.Add(grid);
                    else
                        verticalGrids.Add(grid);
                }
            }
            return (verticalGrids, horizontalGrids);
        }
    }
}
