using SapphireXR_App.ViewModels;
using SapphireXR_App.Views;

namespace SapphireXR_App.WindowServices
{
    public static class LineHeaterControlWindow
    {
        public static LineHeaterControlView Show(string title, string message, string lineHeaterNumber)
        {
            var viewModel = new LineHeaterControlViewModel(title, message, lineHeaterNumber);
            LineHeaterControlView view = new LineHeaterControlView
            {
                DataContext = viewModel
            };
            view.Show();

            return view;
        }
    }
}
