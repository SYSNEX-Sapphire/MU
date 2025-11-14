using SapphireXR_App.ViewModels.FlowController;
using System.Windows.Controls;

namespace SapphireXR_App.Controls
{
    /// <summary>
    /// RecipeEditFlowController.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecipeEditFlowController : UserControl
    {
        public RecipeEditFlowController()
        {
            InitializeComponent();
            DataContext = new RecipeEditFlowControllerViewModel();
        }

        public string? Type { get; set; }
        required public string ControllerID { get; set; }
    }
}
