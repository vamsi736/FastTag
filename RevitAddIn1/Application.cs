using Nice3point.Revit.Toolkit.External;
using FastTag.Commands;

namespace FastTag
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            CreateRibbon();
        }

        private void CreateRibbon()
        {
            var panel = Application.CreatePanel("Commands", "FastTag");

            panel.AddPushButton<AutoDimensionCommand>("Auto Dimension")
                .SetImage("/FastTag;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/FastTag;component/Resources/Icons/RibbonIcon32.png");
        }
    }
}