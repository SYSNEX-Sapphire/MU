using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SapphireXR_App.Views
{
    public partial class LineHeaterControlView : Window
    {
        private void MessageBoxView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        public LineHeaterControlView()
        {
            InitializeComponent();
            MouseLeftButtonDown += MessageBoxView_MouseLeftButtonDown;
        }
    }
}
