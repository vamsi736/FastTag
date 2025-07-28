using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace FastTag.Services
{
    public class ColumnCollector
    {
        private readonly Document _doc;
        private readonly View _view;

        public ColumnCollector(Document doc, View view)
        {
            _doc = doc;
            _view = view;
        }

        public (List<Reference> centerLeftRight, List<Reference> centerFrontBack) CollectColumnCenterReferences()
        {
            List<Reference> leftRightRefs = new List<Reference>();
            List<Reference> frontBackRefs = new List<Reference>();

            var columns = new FilteredElementCollector(_doc, _view.Id)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>();

            foreach (var column in columns)
            {
                var lr = column.GetReferences(FamilyInstanceReferenceType.CenterLeftRight).FirstOrDefault();
                if (lr != null) leftRightRefs.Add(lr);

                var fb = column.GetReferences(FamilyInstanceReferenceType.CenterFrontBack).FirstOrDefault();
                if (fb != null) frontBackRefs.Add(fb);
            }

            return (leftRightRefs, frontBackRefs);
        }
    }
}
