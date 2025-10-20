using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SapphireXR_App.Enums;
using SapphireXR_App.Models;
using SapphireXR_App.WindowServices;
using System.Windows;
using System.Windows.Media;

namespace SapphireXR_App.ViewModels
{
    public partial class LineHeaterControlViewModel : ObservableObject
    {
        public LineHeaterControlViewModel(string title, string message, string lineHeaterNumberStr)
        {
            Title = title;
            Message = message;
            FontColor = OnNormal;
            lineHeaterNumber = lineHeaterNumberStr;
            PropertyChanged += (sender, args) =>
            {
                switch(args.PropertyName)
                {
                    case nameof(TargetValue):
                        ConfirmCommand.NotifyCanExecuteChanged();
                        break;
                }
            };
        }

        private bool confirmEnable()
        {
            return TargetValue != string.Empty;
        }

        [RelayCommand(CanExecute = "confirmEnable")]
        public void Confirm(Window window)
        {
            var onFlowControllerConfirmed = (PopupExResult result, float targetValue) =>
            {
                if (FlowControlConfirmEx.Show("변경 확인", "Target Value " + targetValue + "으로 설정하시겠습니까?") == DialogResult.Ok)
                {
                    try
                    {
                        PLCService.WriteLineHeaterTargetValue(int.Parse(lineHeaterNumber), targetValue);
                        //App.Current.MainWindow.Dispatcher.InvokeAsync(() => ToastMessage.Show("Line Heater " + lineHeaterNumber + " Target Value 설정 완료", ToastMessage.MessageType.Success));
                        ToastMessage.Show("Line Heater " + lineHeaterNumber + " Target Value 설정 완료", ToastMessage.MessageType.Success);
                    }
                    catch (Exception ex)
                    {
                        ToastMessage.Show("PLC로 값을 쓰는데 문제가 발생하였습니다. 자세한 원인은 다음과 같습니다: " + ex.Message, ToastMessage.MessageType.Error);
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            };

            if (onFlowControllerConfirmed(PopupExResult.Confirm, float.Parse(TargetValue)) == true)
            {
                window.Close();
            }
        }


        [RelayCommand]
        private void Close(Window window)
        {
            window.Close();
        }

        [ObservableProperty]
        private string _title = string.Empty;
        [ObservableProperty]
        private string _message = string.Empty;
        [ObservableProperty]
        private string _targetValue = string.Empty;
        [ObservableProperty]
        private int _maxValue = 100;
        [ObservableProperty]
        private SolidColorBrush _fontColor = OnNormal;

        private string lineHeaterNumber;

        private static readonly SolidColorBrush OnNormal = new SolidColorBrush(Colors.Red);
    }
}
