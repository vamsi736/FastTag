using FastTag.ViewModels;

namespace FastTag.Views
{
    public sealed partial class FastTagView
    {
        public FastTagView(FastTagViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}