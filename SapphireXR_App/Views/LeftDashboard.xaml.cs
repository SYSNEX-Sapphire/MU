using SapphireXR_App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SapphireXR_App.Views
{
    public partial class LeftDashboard : Page
    {
        public LeftDashboard()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService(typeof(LeftViewModel));
        }

        private void LineHeater_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBox? lineHeaterPV = sender as TextBox;
            if (lineHeaterPV != null)
            {
                ((LeftViewModel)DataContext).LineHeaterDoubleClickedCommand.Execute(lineHeaterPV.Name);
            }
        }
    }
}
