using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SapphireXR_App.Common;
using SapphireXR_App.Enums;
using SapphireXR_App.Models;
using SapphireXR_App.WindowServices;
using System.Collections;
using System.ComponentModel;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using TwinCAT.Ads;

namespace SapphireXR_App.ViewModels
{
    public partial class LeftViewModel : ObservableObject
    {
        public abstract partial class SourceStatusViewModel : ObservableObject, IDisposable
        {
            private class ValveStateSubscriber : IObserver<bool>
            {
                internal ValveStateSubscriber(SourceStatusViewModel vm, Action<bool> onNextValveStateAC, string valveIDStr)
                {
                    onNextValveState = onNextValveStateAC;
                    sourceStatusViewModel = vm;
                    valveID = valveIDStr;
                }

                void IObserver<bool>.OnCompleted()
                {
                    throw new NotImplementedException();
                }

                void IObserver<bool>.OnError(Exception error)
                {
                    throw new NotImplementedException();
                }

                void IObserver<bool>.OnNext(bool value)
                {
                    if (currentValveState == null || currentValveState != value)
                    {
                        onNextValveState(value);
                        currentValveState = value;
                    }
                }

                protected readonly SourceStatusViewModel sourceStatusViewModel;
                private readonly Action<bool> onNextValveState;
                public readonly string valveID;
                private bool? currentValveState = null; 
            }

            public SourceStatusViewModel(LeftViewModel vm, string valveStateSubscsribePostfixStr)
            {
                valveStateSubscsribePostfix = valveStateSubscsribePostfixStr;
                valveStateSubscrbers = [
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Gas3Source = vm.Gas3; Gas3SourceColor = GasColor;  } else { Gas3Source = vm.Gas1;Gas3SourceColor = Gas1Color; }  }, "V03"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Gas4Source = vm.Gas4; Gas4SourceColor = GasColor; } else { Gas4Source = vm.Gas1; Gas4SourceColor = Gas1Color; }  }, "V04"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source1Carrier = vm.Gas2; Source1CarrierColor = GasColor; } else { Source1Carrier = vm.Gas1; Source1CarrierColor = Gas1Color; }  }, "V05"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source1Source = "Run"; Source1SourceColor = RunColor; } else { Source1Source = "Bypass"; Source1SourceColor = VentBypassColor; } }, "V06"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source2Carrier = vm.Gas2;  Source2CarrierColor = GasColor;} else { Source2Carrier = vm.Gas1; Source2CarrierColor = Gas1Color; }  }, "V07"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source2Source = "Run"; Source2SourceColor = RunColor;} else { Source2Source = "Bypass"; Source2SourceColor = VentBypassColor; } }, "V08"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source3Carrier = vm.Gas2;  Source3CarrierColor = GasColor;} else { Source3Carrier = vm.Gas1; Source3CarrierColor = Gas1Color; }  }, "V09"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source3Source = "Run"; Source3SourceColor = RunColor; } else { Source3Source = "Bypass"; Source3SourceColor = VentBypassColor; } }, "V10"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source4Carrier = vm.Gas2;  Source4CarrierColor = GasColor;} else { Source4Carrier = vm.Gas1; Source4CarrierColor = Gas1Color; }  }, "V11"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source4Source = "Run"; Source4SourceColor = RunColor; } else { Source4Source = "Bypass"; Source4SourceColor = VentBypassColor; } }, "V12"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source1Vent = "Run"; Source1VentColor = RunColor; } else { Source1Vent = "Vent"; Source1VentColor = VentBypassColor; }  }, "V14"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source2Vent = "Run"; Source2VentColor = RunColor; } else { Source2Vent = "Vent"; Source2VentColor = VentBypassColor; }  }, "V15"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source3Vent = "Run"; Source3VentColor = RunColor; } else { Source3Vent = "Vent"; Source3VentColor = VentBypassColor; }  }, "V16"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Gas3Vent = "Run"; Gas3VentColor = RunColor; } else { Gas3Vent = "Vent"; Gas3VentColor = VentBypassColor; }  }, "V17"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Gas4Vent = "Run"; Gas4VentColor = RunColor; } else { Gas4Vent = "Vent"; Gas4VentColor = VentBypassColor; }  }, "V18"),
                    new ValveStateSubscriber(this, (bool nextValveState) => { if (nextValveState == true) { Source4Vent = "Run"; Source4VentColor = RunColor; } else { Source4Vent = "Vent"; Source4VentColor = VentBypassColor; }  }, "V19")
                ];
                foreach (ValveStateSubscriber valveStateSubscriber in valveStateSubscrbers)
                {
                    unsubscribers.Add(ObservableManager<bool>.Subscribe("Valve.OnOff." + valveStateSubscriber.valveID + "." + valveStateSubscsribePostfix, valveStateSubscriber));
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        foreach (IDisposable unsubscriber in unsubscribers)
                        {
                            unsubscriber.Dispose();
                        }
                        unsubscribers.Clear();
                    }

                    // TODO: 비관리형 리소스(비관리형 개체)를 해제하고 종료자를 재정의합니다.
                    // TODO: 큰 필드를 null로 설정합니다.
                    disposedValue = true;
                }
            }

            void IDisposable.Dispose()
            {
                // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public void dispose()
            {
                Dispose(disposing: true);
            }
          
            [ObservableProperty]
            private string gas3Carrier = Util.GetGasDeviceName("Gas1") ?? "";
            [ObservableProperty]
            private string gas3Source = "";
            [ObservableProperty]
            private string gas3Vent = "";
            [ObservableProperty]
            private string gas4Carrier = Util.GetGasDeviceName("Gas1") ?? "";
            [ObservableProperty]
            private string gas4Source = "";
            [ObservableProperty]
            private string gas4Vent = "";
            [ObservableProperty]
            private string source1Carrier = "";
            [ObservableProperty]
            private string source1Source = "";
            [ObservableProperty]
            private string source1Vent = "";
            [ObservableProperty]
            private string source2Carrier = "";
            [ObservableProperty]
            private string source2Source = "";
            [ObservableProperty]
            private string source2Vent = "";
            [ObservableProperty]
            private string source3Carrier = "";
            [ObservableProperty]
            private string source3Source = "";
            [ObservableProperty]
            private string source3Vent = "";
            [ObservableProperty]
            private string source4Carrier = "";
            [ObservableProperty]
            private string source4Source = "";
            [ObservableProperty]
            private string source4Vent = "";
          
            [ObservableProperty]
            private Brush gas3CarrierColor = Gas1Color;
            [ObservableProperty]
            private Brush gas3SourceColor = DefaultColor;
            [ObservableProperty]
            private Brush gas3VentColor = DefaultColor;
            [ObservableProperty]
            private Brush gas4CarrierColor = Gas1Color;
            [ObservableProperty]
            private Brush gas4SourceColor = DefaultColor;
            [ObservableProperty]
            private Brush gas4VentColor = DefaultColor;
            [ObservableProperty]
            private Brush source1CarrierColor = DefaultColor;
            [ObservableProperty]
            private Brush source1SourceColor = DefaultColor;
            [ObservableProperty]
            private Brush source1VentColor = DefaultColor;
            [ObservableProperty]
            private Brush source2CarrierColor = DefaultColor;
            [ObservableProperty]
            private Brush source2SourceColor = DefaultColor;
            [ObservableProperty]
            private Brush source2VentColor = DefaultColor;
            [ObservableProperty]
            private Brush source3CarrierColor = DefaultColor;
            [ObservableProperty]
            private Brush source3SourceColor = DefaultColor;
            [ObservableProperty]
            private Brush source3VentColor = DefaultColor;
            [ObservableProperty]
            private Brush source4CarrierColor = DefaultColor;
            [ObservableProperty]
            private Brush source4SourceColor = DefaultColor;
            [ObservableProperty]
            private Brush source4VentColor = DefaultColor;

            private readonly ValveStateSubscriber[] valveStateSubscrbers;
            private IList<IDisposable> unsubscribers = new List<IDisposable>();
            private readonly string valveStateSubscsribePostfix;

            private static Brush DefaultColor = Application.Current.Resources.MergedDictionaries[0]["SourceStatusDefault"] as Brush ?? Brushes.Black;
            private static Brush Gas1Color = Application.Current.Resources.MergedDictionaries[0]["SourceStatusGas1"] as Brush ?? Brushes.Black;
            private static Brush GasColor = Application.Current.Resources.MergedDictionaries[0]["SourceStatusGasDefault"] as Brush ?? Brushes.Black;
            private static Brush VentBypassColor = Application.Current.Resources.MergedDictionaries[0]["SourceStatusVentBypass"] as Brush ?? Brushes.Black;
            private static Brush RunColor = Application.Current.Resources.MergedDictionaries[0]["SourceStatusRun"] as Brush ?? Brushes.Black;

            private bool disposedValue = false;
        }

        public class SourceStatusFromCurrentPLCStateViewModel : SourceStatusViewModel
        {
            public SourceStatusFromCurrentPLCStateViewModel(LeftViewModel vm) : base(vm, "CurrentPLCState") { }
        }

        public class SourceStatusFromCurrentRecipeStepViewModel: SourceStatusViewModel
        {
            public SourceStatusFromCurrentRecipeStepViewModel(LeftViewModel vm) :base(vm, "CurrentRecipeStep") {  }
        }

        public partial class SubConditionViewModel: ObservableObject
        {
            public SubConditionViewModel(string condition, Brush stateColor)
            {
                Condition = condition;
                StateColor = stateColor;
            }

            [ObservableProperty]
            private Brush stateColor;

            public string Condition { get; private set; }
        }

        public LeftViewModel()
        {
            ObservableManager<float>.Subscribe("MonitoringPresentValue.ShowerHeadTemp.CurrentValue", showerHeaderTempSubscriber = new CoolingWaterValueSubscriber("ShowerHeadTemp", this));
            ObservableManager<BitArray>.Subscribe("HardWiringInterlockState", hardWiringInterlockStateSubscriber = new HardWiringInterlockStateSubscriber(this));
            ObservableManager<int>.Subscribe("MainView.SelectedTabIndex", mainViewTabIndexChagedSubscriber = new MainViewTabIndexChagedSubscriber(this));
            ObservableManager<BitArray>.Subscribe("DeviceIOList", signalTowerStateSubscriber = new SignalTowerStateSubscriber(this));
            ObservableManager<float[]>.Subscribe("LineHeaterTemperature", lineHeaterTemperatureSubscriber = new LineHeaterTemperatureSubscriber(this));
            ObservableManager<bool>.Subscribe("Reset.CurrentRecipeStep", resetCurrentRecipeSubscriber = new ResetCurrentRecipeSubscriber(this));
            ObservableManager<BitArray>.Subscribe("LogicalInterlockState", logicalInterlockSubscriber = new LogicalInterlockSubscriber(this));
            ObservableManager<(string, string)>.Subscribe("GasIOLabelChanged", gasIOLabelSubscriber = new GasIOLabelSubscriber(this));
            ObservableManager<BitArray>.Subscribe("RecipeEnableSubCondition", recipeEnableSubStateSubscriber = new RecipeEnableSubStateSubscriber(this));
            ObservableManager<BitArray>.Subscribe("ReactorEnableSubCondition", reactorEnableSubStateSubscriber = new ReactorEnableSubStateSubscriber(this));

            CurrentSourceStatusViewModel = new SourceStatusFromCurrentPLCStateViewModel(this);
            RecipeEnableConditions = [ new SubConditionViewModel("Not Alarm Triggered", OffLampColor), new SubConditionViewModel("Not Warning Triggered", OffLampColor),new SubConditionViewModel("Not Main Key", OffLampColor), 
                new SubConditionViewModel("DOR On", OffLampColor), new SubConditionViewModel("Chamber Close", OffLampColor), new SubConditionViewModel("Gas Valve Close", OffLampColor), 
                new SubConditionViewModel("Source Valve Close", OffLampColor), new SubConditionViewModel("Vent Valve Close", OffLampColor), new SubConditionViewModel("Valve 21 Open", OffLampColor), 
                new SubConditionViewModel("Temp Auto Mode", OffLampColor), new SubConditionViewModel("Pressure Mode", OffLampColor), new SubConditionViewModel("Chamber Clamp Close", OffLampColor) ];
            ReactorEnableConditions = [ new SubConditionViewModel("Not Alarm Triggered", OffLampColor), new SubConditionViewModel("Open Temp", OffLampColor),new SubConditionViewModel("Open Pressure", OffLampColor),
                new SubConditionViewModel("DOR Off", OffLampColor), new SubConditionViewModel("Recipe Not Running", OffLampColor), new SubConditionViewModel("Gas Valve Close", OffLampColor), 
                new SubConditionViewModel("Source Valve Close", OffLampColor), new SubConditionViewModel("Vent Valve Close", OffLampColor), new SubConditionViewModel("Can Open Temperature(℃)", OffLampColor), 
                new SubConditionViewModel("Can Open Reactor Pressure(Torr)", OffLampColor) ];

            PropertyChanging += (object? sender, PropertyChangingEventArgs args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(CurrentSourceStatusViewModel):
                        CurrentSourceStatusViewModel.dispose();
                        break;
                }
            };
            PropertyChanged += (object? sender, PropertyChangedEventArgs args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(PLCConnectionStatus):
                        switch(PLCConnectionStatus)
                        {
                            case "Connected":
                                PLCConnectionStatusColor = PLCConnectedFontColor;
                                break;

                            case "Disconnected":
                                PLCConnectionStatusColor = PLCDisconnectedFontColor;
                                break;
                        }
                        break;
                }
            };
            setConnectionStatusText(PLCConnectionState.Instance.Online);
            PLCConnectionState.Instance.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(PLCConnectionState.Online))
                {
                    setConnectionStatusText(PLCConnectionState.Instance.Online);
                }
            };
        }

        private static Brush UpdateStateColor(bool state)
        {
            return state == true ? OnLampColor : OffLampColor;
        }

        public void updateRecipeAvailbleSubstate(BitArray recipeAvaibleSubstate)
        {
            for (int substate = 0; substate < PLCService.NumRecipeEnableSubConditions; substate++)
            {
                RecipeEnableConditions[substate].StateColor = UpdateStateColor(recipeAvaibleSubstate[substate]);
            }
        }

        public void updateReactorAvailbleSubstate(BitArray reactorAvaibleSubstate)
        {
            for (int substate = 0; substate < PLCService.NumReactorEnableSubConditions; substate++)
            {
                ReactorEnableConditions[substate].StateColor = UpdateStateColor(reactorAvaibleSubstate[substate]);
            }
        }


        public static string GetIogicalInterlockLabel(string? gasName)
        {
            if (gasName != default)
            {
                return "Gas Pressure " + gasName;
            }
            else
            {
                return "";
            }
        }

        private void setConnectionStatusText(bool online)
        {
            switch (online)
            {
                case true:
                    PLCConnectionStatus = "Connected";
                    try
                    {
                        bool onOff = PLCService.ReadBuzzerOnOff();
                        BuzzerIcon = onOff == true ? BuzzerOnIcon : BuzzerOffIcon;

                        PLCService.WriteInterlockEnableState(onOff, PLCService.InterlockEnableSetting.Buzzer);
                    }
                    catch(Exception)
                    {
                    }
                    break;

                case false:
                    PLCConnectionStatus = "Disconnected";
                    BuzzerIcon = null;
                    break;
            }
        }

        [RelayCommand]
        public void ToggleBuzzerOnOff()
        {
            bool onOff = BuzzerIcon == BuzzerOffIcon;
            if (ConfirmMessage.Show("Buzzer 상태 변경", "Buzzer" + (onOff == true ? " On" : " Off") + " 상태로 변경하시겠습니까?", WindowStartupLocation.Manual) == DialogResult.Ok)
            {
                PLCService.WriteBuzzerOnOff(onOff);
                BuzzerIcon = onOff == true ? BuzzerOnIcon : BuzzerOffIcon;
            }
            
        }

        [RelayCommand]
        private void ShowBuzzerOnOfRect()
        {
            BuzzerOnOffRectOpacity = 0.6;
        }

        [RelayCommand]
        private void HideBuzzerOnOfRect()
        {
            BuzzerOnOffRectOpacity = 0.0;
        }

        [ObservableProperty]
        private static string _gas3 = Util.GetGasDeviceName("Gas3") ?? "";
        [ObservableProperty]
        private static string _gas4 = Util.GetGasDeviceName("Gas4") ?? "";
        [ObservableProperty]
        private static string _source1 = Util.GetGasDeviceName("Source1") ?? "";
        [ObservableProperty]
        private static string _source2 = Util.GetGasDeviceName("Source2") ?? "";
        [ObservableProperty]
        private static string _source3 = Util.GetGasDeviceName("Source3") ?? "";
        [ObservableProperty]
        private static string _source4 = Util.GetGasDeviceName("Source4") ?? "";
        [ObservableProperty]
        private static string _logicalInterlockGas1 = GetIogicalInterlockLabel(Util.GetGasDeviceName("Gas1"));
        [ObservableProperty]
        private static string _logicalInterlockGas2 = GetIogicalInterlockLabel(Util.GetGasDeviceName("Gas2"));
        [ObservableProperty]
        private static string _logicalInterlockGas3 = GetIogicalInterlockLabel(Util.GetGasDeviceName("Gas3"));
        [ObservableProperty]
        private static string _logicalInterlockGas4 = GetIogicalInterlockLabel(Util.GetGasDeviceName("Gas4"));

        private static readonly Brush OnLampColor = Application.Current.Resources.MergedDictionaries[0]["LampOnColor"] as Brush ?? Brushes.Lime;
        private static readonly Brush OffLampColor = Application.Current.Resources.MergedDictionaries[0]["LampOffColor"] as Brush ?? Brushes.DarkGray;
        private static readonly Brush ReadyLampColor = Application.Current.Resources.MergedDictionaries[0]["LampReadyColor"] as Brush ?? Brushes.Yellow;
        private static readonly Brush RunLampColor = Application.Current.Resources.MergedDictionaries[0]["LampRunolor"] as Brush ?? Brushes.Lime;
        private static readonly Brush FaultLampColor = Application.Current.Resources.MergedDictionaries[0]["LampFaultColor"] as Brush ?? Brushes.Red;

        private static readonly Brush PLCConnectedFontColor = Application.Current.Resources.MergedDictionaries[0]["Sapphire_Blue"] as Brush ?? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x60, 0xCD, 0xFF));
        private static readonly Brush PLCDisconnectedFontColor = Application.Current.Resources.MergedDictionaries[0]["Alert_Red_02"] as Brush ?? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xEC, 0x3D, 0x3F));

        private static readonly string SignalTowerRedPath = "/Resources/icons/icon=ani_signal_red.gif";
        private static readonly string SignalTowerBluePath = "/Resources/icons/icon=ani_signal_blue.gif";
        private static readonly string SignalTowerGreenath = "/Resources/icons/icon=ani_signal_green.gif";
        private static readonly string SignalTowerYellowPath = "/Resources/icons/icon=ani_signal_yellow.gif";
        private static readonly string SignalTowerWhitePath = "/Resources/icons/icon=ani_signal_white.gif";
        private static readonly string SignalTowerDefaultPath = "/Resources/icons/icon=ani_signal_default.gif";

        private string Gas1 = Util.GetGasDeviceName("Gas1") ?? "";
        private string Gas2 = Util.GetGasDeviceName("Gas2") ?? "";

        [ObservableProperty]
        private string _showerHeadTemp = "";

        [ObservableProperty]
        private Brush _maintenanceKeyLampColor = OnLampColor;
        [ObservableProperty]
        private Brush _cleanDryAirLampColor = OnLampColor;
        [ObservableProperty]
        private Brush _doorReactorCabinetLampColor = OnLampColor;
        [ObservableProperty]
        private Brush _doorPowerDistributeCabinetLampColor = OnLampColor;
        [ObservableProperty]
        private Brush _doorGasDeliveryCabinetLampColor = OnLampColor;
        [ObservableProperty]
        private Brush _coolingWaterLampColor = OnLampColor;
        [ObservableProperty]
        private Brush _susceptorMotorLampColor = OnLampColor;
        [ObservableProperty]
        private Brush _vacuumPumpLampColor = ReadyLampColor;
        [ObservableProperty]
        private Brush _dorVacuumStateLampColor = ReadyLampColor;
        [ObservableProperty]
        private Brush _tempControllerAlarmLampColor = OnLampColor;

        [ObservableProperty]
        private double _glowOpacity = 0.0;

        [ObservableProperty]
        private Brush _gasPressureGas2StateColor = Brushes.Transparent;
        [ObservableProperty]
        private Brush _gasPressureGas1StateColor = Brushes.Transparent;
        [ObservableProperty]
        private Brush _gasPressureGas3StateColor = Brushes.Transparent;
        [ObservableProperty]
        private Brush _gasPressureGas4StateColor = Brushes.Transparent;
        [ObservableProperty]
        private Brush _recipeStartStateColor = Brushes.Transparent;
        [ObservableProperty]
        private Brush _reactorOpenStateColor = Brushes.Transparent;
        [ObservableProperty]
        private Brush _pumpTurnOnStateColor = Brushes.Transparent;

        [ObservableProperty]
        private int _lineHeater1;
        [ObservableProperty]
        private int _lineHeater2;
        [ObservableProperty]
        private int _lineHeater3;
        [ObservableProperty]
        private int _lineHeater4;
        [ObservableProperty]
        private int _lineHeater5;
        [ObservableProperty]
        private int _lineHeater6;
        [ObservableProperty]
        private int _lineHeater7;
        [ObservableProperty]
        private int _lineHeater8;

        [ObservableProperty]
        private string _pLCAddressText = AmsNetId.Local.ToString();
        [ObservableProperty]
        private string _pLCConnectionStatus = "Diconnected";
        [ObservableProperty]
        private Brush _pLCConnectionStatusColor = PLCDisconnectedFontColor;

        [ObservableProperty]
        private SourceStatusViewModel _currentSourceStatusViewModel;

        [ObservableProperty]
        private string signalTowerImage = SignalTowerDefaultPath;

        [ObservableProperty]
        private double buzzerOnOffRectOpacity = 0.0;

        [ObservableProperty]
        private object? buzzerIcon = null;

        private static readonly Canvas? BuzzerOffIcon = App.Current.Resources.MergedDictionaries[4]["buzzer_off"] as Canvas;
        private static readonly Canvas? BuzzerOnIcon = App.Current.Resources.MergedDictionaries[5]["buzzer_on"] as Canvas;

        private readonly CoolingWaterValueSubscriber showerHeaderTempSubscriber;
        private readonly HardWiringInterlockStateSubscriber hardWiringInterlockStateSubscriber;
        private readonly MainViewTabIndexChagedSubscriber mainViewTabIndexChagedSubscriber;
        private readonly SignalTowerStateSubscriber signalTowerStateSubscriber;
        private readonly LineHeaterTemperatureSubscriber lineHeaterTemperatureSubscriber;
        private readonly ResetCurrentRecipeSubscriber resetCurrentRecipeSubscriber;
        private readonly LogicalInterlockSubscriber logicalInterlockSubscriber;
        private readonly GasIOLabelSubscriber gasIOLabelSubscriber;
        private readonly RecipeEnableSubStateSubscriber recipeEnableSubStateSubscriber;
        private readonly ReactorEnableSubStateSubscriber reactorEnableSubStateSubscriber;

        public IList<SubConditionViewModel> RecipeEnableConditions { get; private set; }
        public IList<SubConditionViewModel> ReactorEnableConditions { get; private set; }

        public ICommand LineHeaterDoubleClickedCommand  => new RelayCommand<object?>((object? arg) => {
            string? lineHeaterName = arg as string;
            if(lineHeaterName != null)
            {
                string lineHeaterNumber = lineHeaterName.Substring("lineHeater".Length);
                if (int.TryParse(lineHeaterNumber, out int number) == true)
                {
                    LineHeaterControlWindow.Show("Line Heater", "Line Heater " + lineHeaterNumber + "의 온도를 조절하시겠습니까?", lineHeaterNumber);
                }
            }
        });
    }
}
