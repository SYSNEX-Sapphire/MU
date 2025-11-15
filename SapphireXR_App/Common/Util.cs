using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SapphireXR_App.Enums;
using SapphireXR_App.Models;
using SapphireXR_App.ViewModels;
using SapphireXR_App.WindowServices;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static SapphireXR_App.ViewModels.ManualBatchViewModel;

namespace SapphireXR_App.Common
{
    static class Util
    {
        public static T? FindParent<T>(DependencyObject child, string parentName)
          where T : DependencyObject
        {
            if (child == null) return null;

            T? foundParent = null;
            var currentParent = VisualTreeHelper.GetParent(child);

            do
            {
                var frameworkElement = currentParent as FrameworkElement;
                if (frameworkElement?.Name == parentName && frameworkElement is T)
                {
                    foundParent = (T)currentParent;
                    break;
                }

                currentParent = VisualTreeHelper.GetParent(currentParent);

            } while (currentParent != null);

            return foundParent;
        }

        public static bool IsTextNumeric(string str)
        {
            return regUint.IsMatch(str);

        }

        public static bool IsTextFloatintgPoint(string str)
        {
            return regUFloat.IsMatch(str) || regUFloatADotPostFix.IsMatch(str);
        }

        static readonly System.Text.RegularExpressions.Regex regUint = new System.Text.RegularExpressions.Regex("^\\d+$");
        static readonly System.Text.RegularExpressions.Regex regUFloat = new System.Text.RegularExpressions.Regex("^\\d*\\.?\\d*$");
        static readonly System.Text.RegularExpressions.Regex regUFloatADotPostFix = new System.Text.RegularExpressions.Regex("^\\d*\\.$");

        public static string GetResourceAbsoluteFilePath(string subPath)
        {
            return GetAbsoluteFilePathFromAppRelativePath("\\Resources\\" + subPath);
        }

        public static string GetAbsoluteFilePathFromAppRelativePath(string subPath)
        {
            return AppDomain.CurrentDomain.BaseDirectory + subPath.TrimStart('\\');
        }

        public static void SetIfChanged(bool newValue, ref bool? prevValue, Action<bool> onChanged)
        {
            if (prevValue == null || prevValue != newValue)
            {
                onChanged(newValue);
                prevValue = newValue;
            }
        }

        public static void CostraintTextBoxColumnOnlyNumber(object sender, FlowControllerDataGridTextColumnTextBoxValidaterOnlyNumber flowControllerDataGridTextColumnTextBoxValidaterOnlyNumber,
            FlowControllerTextBoxValidater.NumberType numberType)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox != null)
            {
                string validatedStr = flowControllerDataGridTextColumnTextBoxValidaterOnlyNumber.validate(textBox, numberType);
                if (validatedStr != textBox.Text)
                {
                    int textCaret = Math.Max(textBox.CaretIndex - 1, 0);
                    textBox.Text = validatedStr;
                    textBox.CaretIndex = textCaret;
                }
            }
        }

        public static void CostraintTextBoxColumnMaxNumber(object sender, FlowControllerDataGridTextColumnTextBoxValidaterMaxValue flowControllerDataGridTextColumnTextBoxValidaterMaxValue, 
            uint maxValue, FlowControllerTextBoxValidater.NumberType numberType)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox != null)
            {
                (string valiatedStr, FlowControllerTextBoxValidaterMaxValue.Result result) = flowControllerDataGridTextColumnTextBoxValidaterMaxValue.validate(textBox, maxValue, numberType);
                if (FlowControllerTextBoxValidaterMaxValue.Result.NotNumber <= result && result <= FlowControllerTextBoxValidaterMaxValue.Result.ExceedMax)
                {
                    int textCaret = Math.Max(textBox.CaretIndex - 1, 0);
                    textBox.Text = valiatedStr;
                    textBox.CaretIndex = textCaret;
                }
            }
        }

        public static void CostraintTextBoxColumnMaxNumber(object sender, FlowControllerDataGridTextColumnTextBoxValidaterMaxValue flowControllerDataGridTextColumnTextBoxValidaterMaxValue, TextChangedEventArgs e,
            FlowControllerTextBoxValidater.NumberType numberType)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox != null)
            {
                (string? validatedStr, FlowControllerTextBoxValidaterMaxValue.Result result) = flowControllerDataGridTextColumnTextBoxValidaterMaxValue.validate(textBox, e, numberType);
                if (FlowControllerTextBoxValidaterMaxValue.Result.NotNumber <= result && result <= FlowControllerTextBoxValidaterMaxValue.Result.ExceedMax)
                {
                    int caretIndex = Math.Max(textBox.CaretIndex - 1, 0);
                    textBox.Text = validatedStr;
                    textBox.CaretIndex = caretIndex;
                }
            }
        }

        public static void ConstraintEmptyToZeroOnDataGridCellCommit(object sender, DataGridCellEditEndingEventArgs e, IList<string> headers)
        {
            ConstraintEmptyToDefaultValueOnDataGridCellCommit(sender, e, headers, "0");
        }

        public static void ConstraintEmptyToDefaultValueOnDataGridCellCommit(object sender, DataGridCellEditEndingEventArgs e, IList<string> headers, string defaultValue)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                string? columnHeader = e.Column.Header as string;
                if (columnHeader != null && headers.Contains(columnHeader) == true)
                {
                    TextBox? editingElement = e.EditingElement as TextBox;
                    if (editingElement != null && editingElement.Text == "")
                    {
                        editingElement.Text = defaultValue;
                    }
                }
            }
        }

        public static void ConstraintEmptyToDefaultValueOnDataGridCellCommit(object sender, DataGridCellEditEndingEventArgs e, Dictionary<string, string> headerDefaultValuePair)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                string? columnHeader = e.Column.Header as string;
                if (columnHeader != null && headerDefaultValuePair.TryGetValue(columnHeader, out string? defaultValue) == true)
                {
                    TextBox? editingElement = e.EditingElement as TextBox;
                    if (editingElement != null && editingElement.Text == "")
                    {
                        editingElement.Text = defaultValue;
                    }
                }
            }
        }

        public static bool SynchronizeExpected<T>(T expected, Func<T> checkFunc, long timeOutMS) where T : INumber<T>
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                if (checkFunc() == expected)
                {
                    return true;
                }

                if (timeOutMS <= stopwatch.ElapsedMilliseconds)
                {
                    return false;
                }
            }
        }

        public static string ToEventLogFormat(DateTime dateTime)
        {
            return dateTime.ToString("yyyy.MM.dd HH:mm:ss");
        }

        public static string FloatingPointStrWithMaxDigit(float value, int maxNumberDigit)
        {
            var numberDecimalDigits = (float value, int maxNumberDigit) =>
            {
                int intValue = (int)value;
                if (0 <= intValue && intValue < 10)
                {
                    return maxNumberDigit - 1;
                }
                else if (10 <= intValue && intValue < 100)
                {
                    return maxNumberDigit - 2;
                }
                else if (100 <= intValue && intValue < 1000)
                {
                    return maxNumberDigit - 3;
                }
                else
                {
                    return 0;
                }
            };
            return value.ToString("N", new NumberFormatInfo() { NumberDecimalDigits = numberDecimalDigits(value, AppSetting.FloatingPointMaxNumberDigit) });
        }

        public static void LoadBatchToPLC(Batch batch)
        {
            if (batch.RampingTime != null)
            {
                PLCService.WriteFlowControllerTargetValue(batch.AnalogIOUserStates.Select((AnalogIOUserState analogIOUserState) => (analogIOUserState.ID, analogIOUserState.Value)).ToArray(),
                    (short)batch.RampingTime);
                BitArray valveStates = new BitArray(DeviceConfiguration.RecipeValves.Count);
                foreach (DigitalIOUserState digitalIOUserState in batch.DigitalIOUserStates)
                {
                    int index;
                    if (DeviceConfiguration.ValveIDtoOutputSolValveIdx.TryGetValue(digitalIOUserState.ID, out index) == true && index < valveStates.Count)
                    {
                        valveStates[index] = digitalIOUserState.On;
                    }
                }
                PLCService.WriteValveState(valveStates);
            }
        }

        public static object? GetSettingValue(JToken rootToken, string key)
        {
            if (rootToken != null)
            {
                JToken? token = rootToken[key];
                if (token != null)
                {
                    return JsonConvert.DeserializeObject(token.ToString());
                }
            }

            return null;
        }

        public static string? GetGasDeviceName(string id)
        {
            return SettingViewModel.GasIO.Where((Device device) => device.ID == id).Select((Device device) => device.Name != null ? device.Name : default).FirstOrDefault();
        }

        public static string? GetFlowControllerName(string id)
        {
            return SettingViewModel.dAnalogDeviceIO.Where((KeyValuePair<string, AnalogDeviceIO> device) => device.Key == id).Select((KeyValuePair<string, AnalogDeviceIO> device) => device.Value.Name != null ? device.Value.Name : default).FirstOrDefault();
        }

        public static string? GetValveName(string id)
        {
            return SettingViewModel.ValveDeviceIO.Where((Device device) => device.ID == id).Select((Device device) => device.Name != null ? device.Name : default).FirstOrDefault();
        }

        public static void ConfirmBeforeToggle(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ToggleButton? toggleSwitch = sender as ToggleButton;
            if (toggleSwitch != null)
            {
                string destState = toggleSwitch.IsChecked == true ? "Off": "On";
                if (ValveOperationEx.Show("", destState + " 상태로 변경하시겠습니까?") == DialogResult.Cancel)
                {
                    e.Handled = true;
                }
                else
                {
                    toggleSwitch.IsChecked = !toggleSwitch.IsChecked;
                    toggleSwitch.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                }
            }
        }

        public static void TrimLastDotOnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.Text = textBox.Text.TrimEnd('.');
            }
        }

        public static void ShowFlowControllersMaxValueExceededIfExist(HashSet<string>? fcMaxValueExceeded)
        {
            if(fcMaxValueExceeded != null && 0 < fcMaxValueExceeded.Count)
            {
                string message = "Recipe를 읽어오는 중 최대값을 초과한 Flow Controller 값들이 발견되었습니다. 이 값들은 최대값으로 강제됩니다:\r\n";
                foreach (string flowController in fcMaxValueExceeded)
                {
                    message += flowController + " = " + SettingViewModel.ReadMaxValue(flowController) + "\r\n";
                }
                
                MessageBox.Show(message);
            }
        }
    }
}
