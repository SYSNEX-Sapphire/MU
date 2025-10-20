using SapphireXR_App.Common;
using SapphireXR_App.ViewModels;
using System.Windows.Controls;

namespace SapphireXR_App.Views
{
    public partial class RecipeEditPage : Page
    {
        public RecipeEditPage()
        {
            InitializeComponent();
            RecipeEditViewModel viewModel = (RecipeEditViewModel)(DataContext = App.Current.Services.GetService(typeof(RecipeEditViewModel)))!;
            flowControllerDataGridTextColumnTextBoxValidaterMaxValue = new FlowControllerDataGridTextColumnTextBoxValidaterMaxValue(viewModel, nameof(viewModel.Recipes));
            flowControllerDataGridTextColumnTextBoxValidaterOnlyNumber = new FlowControllerDataGridTextColumnTextBoxValidaterOnlyNumber(viewModel, nameof(viewModel.Recipes));
        }

        private void TextBox_TextChangedIntegerMaxNumber(object sender, TextChangedEventArgs e)
        {
            Util.CostraintTextBoxColumnMaxNumber(sender, flowControllerDataGridTextColumnTextBoxValidaterMaxValue, e, FlowControllerTextBoxValidater.NumberType.Integer);
        }

        private void TextBox_TextChangedIntegerOnlyNumber(object sender, TextChangedEventArgs e)
        {

            Util.CostraintTextBoxColumnOnlyNumber(sender, flowControllerDataGridTextColumnTextBoxValidaterOnlyNumber, FlowControllerTextBoxValidater.NumberType.Integer);
        }

        private void TextBox_TextChangedFloatingPointMaxNumber(object sender, TextChangedEventArgs e)
        {
            Util.CostraintTextBoxColumnMaxNumber(sender, flowControllerDataGridTextColumnTextBoxValidaterMaxValue, e, FlowControllerTextBoxValidater.NumberType.Float);
        }

        private void TextBox_TextChangedFloatingPointOnlyNumber(object sender, TextChangedEventArgs e)
        {

            Util.CostraintTextBoxColumnOnlyNumber(sender, flowControllerDataGridTextColumnTextBoxValidaterOnlyNumber, FlowControllerTextBoxValidater.NumberType.Float);
        }

        private void flowDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Util.ConstraintEmptyToDefaultValueOnDataGridCellCommit(sender, e, ColumnDefaultValue);
        }

        private void TextBox_LostFocusTrimLastDot(object sender, System.Windows.RoutedEventArgs e)
        {
            Util.TrimLastDotOnLostFocus(sender, e);
        }

        FlowControllerDataGridTextColumnTextBoxValidaterMaxValue flowControllerDataGridTextColumnTextBoxValidaterMaxValue;
        FlowControllerDataGridTextColumnTextBoxValidaterOnlyNumber flowControllerDataGridTextColumnTextBoxValidaterOnlyNumber;

        private static readonly Dictionary<string, string> ColumnDefaultValue = new Dictionary<string, string>() { { "Ramp", "1" }, { "Hold", "1" } };
    }
}
