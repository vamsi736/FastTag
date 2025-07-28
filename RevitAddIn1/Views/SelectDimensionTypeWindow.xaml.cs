using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FastTag.Views
{
    public partial class SelectDimensionTypeWindow : Window
    {
        public DimensionType? SelectedDimensionType { get; private set; }

        public SelectDimensionTypeWindow(List<DimensionType> dimensionTypes)
        {
            InitializeComponent();

            // Bind dimension types to ComboBox
            DimensionTypeCombo.ItemsSource = dimensionTypes;
            DimensionTypeCombo.DisplayMemberPath = "Name";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            SelectedDimensionType = DimensionTypeCombo.SelectedItem as DimensionType;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedDimensionType = null;
            DialogResult = false;
            Close();
        }
    }
}
