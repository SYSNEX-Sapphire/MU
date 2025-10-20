﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SapphireXR_App.Common;
using SapphireXR_App.Enums;
using SapphireXR_App.Models;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace SapphireXR_App.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        static SettingViewModel()
        {
            var fdevice = File.ReadAllText(DevceIOSettingFilePath);
            JToken? jDeviceInit = JToken.Parse(fdevice);
            if(jDeviceInit == null)
            {
                return;
            }

            var publishDeviceNameChanged = (object? sender, string? propertyName, ObservableManager<(string, string)>.Publisher publisher) =>
            {
                if (propertyName == nameof(Device.Name))
                {
                    Device? device = sender as Device;
                    if (device != null && device.ID != null && device.Name != null)
                    {
                        publisher.Publish((device.ID, device.Name));
                    }
                }
            };

            JToken? jAnalogDeviceIO = jDeviceInit["AnalogDeviceIO"];
            if (jAnalogDeviceIO != null)
            {
                var deserialized = JsonConvert.DeserializeObject<Dictionary<string, AnalogDeviceIO>>(jAnalogDeviceIO.ToString());
                if (deserialized != null)
                {
                    dAnalogDeviceIO = deserialized;
                }
            }
            JToken? jSwitchDI = jDeviceInit["SwitchDI"];
            if (jSwitchDI != null)
            {
                dSwitchDI = JsonConvert.DeserializeObject<Dictionary<string, SwitchDI>>(jSwitchDI.ToString());
            }
            JToken? jAlarmDeviation = jDeviceInit["AlarmDeviation"];
            if (jAlarmDeviation != null)
            {
                AlarmDeviationValue = JsonConvert.DeserializeObject<float>(jAlarmDeviation.ToString());
            }
            JToken? jWarningDeviation = jDeviceInit["WarningDeviation"];
            if (jWarningDeviation != null)
            {
                WarningDeviationValue = JsonConvert.DeserializeObject<float>(jWarningDeviation.ToString());
            }
            JToken? jAnalogDelayTime = jDeviceInit["AlarmDelayTimeA"];
            if (jAnalogDelayTime != null)
            {
                AnalogDeviceDelayTimeValue = JsonConvert.DeserializeObject<float>(jAnalogDelayTime.ToString());
            }
            JToken? jDigitalDelayTime = jDeviceInit["AlarmDelayTimeD"];
            if (jDigitalDelayTime != null)
            {
                DigitalDeviceDelayTimeValue = JsonConvert.DeserializeObject<float>(jDigitalDelayTime.ToString());
            }
            JToken? jInterLockD = jDeviceInit["InterLockD"];
            if (jInterLockD != null)
            {
                dInterLockD = JsonConvert.DeserializeObject<Dictionary<string, InterLockD>>(jInterLockD.ToString());
                if (dInterLockD != null)
                {
                    foreach ((string name, InterLockD interlockD) in dInterLockD)
                    {
                        PLCService.InterlockEnableSetting? plcArg = null;
                        switch (name)
                        {
                            case "SusceptorRotationMotor":
                                plcArg = PLCService.InterlockEnableSetting.SusceptorRotationMotor;
                                break;
                        }
                        if (plcArg != null)
                        {
                            interlockD.PropertyChanged += (sender, args) =>
                            {
                                if (PLCService.Connected == PLCConnection.Connected)
                                {
                                    InterLockD? interlockD = sender as InterLockD;
                                    if (interlockD != null)
                                    {
                                        switch(args.PropertyName)
                                        {
                                            case nameof(InterLockD.IsEnable):
                                                PLCService.WriteInterlockEnableState(interlockD.IsEnable, plcArg.Value);
                                                break;
                                        }
                                    }
                                }
                            };
                        }
                    }
                }
            }
            JToken? jInterLockA = jDeviceInit["InterLockA"];
            if (jInterLockA != null)
            {
                dInterLockA = JsonConvert.DeserializeObject<Dictionary<string, InterLockA>>(jInterLockA.ToString());
                if (dInterLockA != null)
                {
                    foreach ((string name, InterLockA interlockA) in dInterLockA)
                    {
                        PLCService.InterlockEnableSetting interlockEnableSetting;
                        if (InterlockSettingNameToPLCInterlockEnableSettingEnum.TryGetValue(name, out interlockEnableSetting) == true)
                        {
                            interlockA.PropertyChanged += (sender, args) =>
                            {
                                if (PLCService.Connected == PLCConnection.Connected)
                                {
                                    InterLockA? interlockA = sender as InterLockA;
                                    if (interlockA != null)
                                    {
                                        switch (args.PropertyName)
                                        {
                                            case nameof(InterLockA.IsEnable):
                                                PLCService.WriteInterlockEnableState(interlockA.IsEnable, interlockEnableSetting);
                                                break;
                                        }
                                    }
                                }
                            };
                        }
                        PLCService.InterlockValueSetting interlockValueSetting;
                        if (InterlockSettingNameToPLCInterlockValueSettingEnum.TryGetValue(name, out interlockValueSetting) == true)
                        {
                            interlockA.PropertyChanged += (sender, args) =>
                            {
                                if (PLCService.Connected == PLCConnection.Connected)
                                {
                                    InterLockA? interlockA = sender as InterLockA;
                                    if (interlockA != null)
                                    {
                                        switch (args.PropertyName)
                                        {
                                            case nameof(InterLockA.Treshold):
                                                try
                                                {
                                                    PLCService.WriteInterlockValueState(float.Parse(interlockA.Treshold), interlockValueSetting);
                                                }
                                                catch(ArgumentException)
                                                { 
                                                }
                                                catch(FormatException)
                                                {
                                                }
                                                catch(OverflowException)
                                                {
                                                }
                                                break;
                                        }
                                    }
                                }
                            };
                        }
                    }
                }
            }
            JToken? jValveDeviceIO = jDeviceInit["ValveDeviceIO"];
            if (jValveDeviceIO != null)
            {
                var deserialized = JsonConvert.DeserializeObject<List<Device>>(jValveDeviceIO.ToString());
                if(deserialized != null)
                {
                    ValveDeviceIO = deserialized;
                }
                else
                {
                    ValveDeviceIO = CreateDefaultValveDeviceIO();
                }
            }
            else
            {
                ValveDeviceIO = CreateDefaultValveDeviceIO();
            }
            foreach (var io in ValveDeviceIO)
            {
                io.PropertyChanged += (object? sender, PropertyChangedEventArgs args) =>
                {
                    PublishDeviceNameChanged(sender, args.PropertyName, ValveIOLabelChangedPublisher);
                };
            }
            JToken? jGasIO = jDeviceInit["GasIO"];
            if (jGasIO != null)
            {
                var deserialized = JsonConvert.DeserializeObject<List<Device>>(jGasIO.ToString());
                if (deserialized != null)
                {
                    GasIO = deserialized;
                }
            }
            foreach (var io in GasIO)
            {
                io.PropertyChanged += (object? sender, PropertyChangedEventArgs args) =>
                {
                    PublishDeviceNameChanged(sender, args.PropertyName, GasIOLabelChangedPublisher);
                };
            }
        }

        public SettingViewModel()
        {
            AlarmSettingLoad();
            IOList = new List<IOSetting> {
                new() { Name= "Power Reset Switch", OnOff = false },new() { Name= "Cover - Upper Limit", OnOff = false }, new() { Name= "Cover - Lower Limit", OnOff = false },
                new() { Name= "Cylinder Auto Switch - Closed", OnOff = false },new() { Name= "Cylinder Auto Switch -Opened", OnOff = false }, new() { Name= "SMPS - 24V 480", OnOff = false }, 
                new() { Name= "SMPS - 24V 72", OnOff = false },  new() { Name= "SMPS - 15V Plus", OnOff = false }, new() { Name= "SMPS - 15V Minus", OnOff = false },  
                new() { Name= "CB GraphiteHeater", OnOff = false }, new() { Name= "CB Thermal Bath", OnOff = false }, new() { Name= "CB Vacuum Pump", OnOff = false }, 
                new() { Name= "CB Line Heater", OnOff = false }, new() { Name= "CB Rotation Motor", OnOff = false }, new() { Name= "CB Cover Lift Motor", OnOff = false },  
                new() { Name= "CB Throttle Valve", OnOff = false }, new() { Name= "CB Gas Detector", OnOff = false }, new() { Name= "CB Cabinet Lamp", OnOff = false },
                new() { Name= "CB MFC Power", OnOff = false },  new() { Name= "Line Heater #1", OnOff = false }, new() { Name= "Line Heater #2", OnOff = false },
                new() { Name= "Line Heater #3", OnOff = false }, new() { Name= "Line Heater #4", OnOff = false },  new() { Name= "Line Heater #5", OnOff = false },
                new() { Name= "Line Heater #6", OnOff = false },  new() { Name= "Line Heater #7", OnOff = false },  new() { Name= "Line Heater #8", OnOff = false },
                new() { Name= "Gas Detector H2", OnOff = false }, new() { Name= "Gas Detector H2S", OnOff = false }, new() { Name= "Gas Detector H2Se", OnOff = false },
                new() { Name= "Fire Sensor", OnOff = false },  new() { Name= "External Scrubber Fault", OnOff = false }, new() { Name= "External H2 Gas Cabinet Fault", OnOff = false },  
                new() { Name= "External H2S Gas Cabinet Fault", OnOff = false }, new() { Name= "External H2Se Gas Cabinet Fault", OnOff = false }, new() { Name= "External User Input Alarm", OnOff = false },
                new() { Name= "Thermal Bath #1 Deviation", OnOff = false }, new() { Name= "Thermal Bath #1 CutOff", OnOff = false },  new() { Name= "Thermal Bath #2 Deviation", OnOff = false }, 
                new() { Name= "Thermal Bath #2 CutOff", OnOff = false },  new() { Name= "Thermal Bath #3 Deviation", OnOff = false }, new() { Name= "Thermal Bath #3 CutOff", OnOff = false },
                new() { Name= "Thermal Bath #4 Deviation", OnOff = false }, new() { Name= "Thermal Bath #4 CutOff", OnOff = false }, new() { Name= "Singal Tower - RED", OnOff = false }, 
                new() { Name= "Singal Tower - YELLOW", OnOff = false }, new() { Name= "Singal Tower - GREEN", OnOff = false }, new() { Name= "Singal Tower - BLUE", OnOff = false }, 
                new() { Name= "Singal Tower - WHITE", OnOff = false }, new() { Name= "Singal Tower - BUZZWER", OnOff = false }, new() { Name= "Clamp Lock", OnOff = false },
                new() { Name= "Clamp Release", OnOff = false }
            };

            ObservableManager<BitArray>.Subscribe("DeviceIOList", iOStateListSubscriber = new IOStateListSubscriber(this));
            ObservableManager<bool>.Subscribe("App.Closing", appClosingSubscriber = new AppClosingSubscriber(this));

            AnalogWarningCheckAllColumnViewModel = new CheckAllColumnViewModel<AnalogDeviceIO>(lAnalogDeviceIO, PLCService.TriggerType.Warning);
            AnalogAlarmCheckAllColumnViewModel = new CheckAllColumnViewModel<AnalogDeviceIO>(lAnalogDeviceIO, PLCService.TriggerType.Alarm);
            DigitalWarningCheckAllColumnViewModel = new CheckAllColumnViewModel<SwitchDI>(lSwitchDI, PLCService.TriggerType.Warning);
            DigitalAlarmCheckAllColumnViewModel = new CheckAllColumnViewModel<SwitchDI>(lSwitchDI, PLCService.TriggerType.Alarm);

            PropertyChanged += (object? sender, PropertyChangedEventArgs args) =>
            {
                if (PLCConnectionState.Instance.Online == true)
                {
                    switch (args.PropertyName)
                    {
                        case nameof(AlarmDeviation):
                            PLCService.WriteAlarmDeviationState(AlarmDeviation);
                            break;

                        case nameof(WarningDeviation):
                            PLCService.WriteWarningDeviationState(WarningDeviation);
                            break;

                        case nameof(AnalogDeviceDelayTime):
                            PLCService.WriteAnalogDeviceDelayTime(AnalogDeviceDelayTime);
                            break;

                        case nameof(DigitalDeviceDelayTime):
                            PLCService.WriteDigitalDeviceDelayTime(DigitalDeviceDelayTime);
                            break;

                        case nameof(InductionHeaterPowerOnOff):
                            if (InductionHeaterPowerOnOff != null)
                            {
                                PLCService.WriteOutputCmd1(PLCService.OutputCmd1Index.GraphiteHeaterPower, InductionHeaterPowerOnOff.Value);
                            }
                            break;

                        case nameof(ThermalBathPowerOnOff):
                            if(ThermalBathPowerOnOff != null)
                            {
                                PLCService.WriteOutputCmd1(PLCService.OutputCmd1Index.ThermalBathPower, ThermalBathPowerOnOff.Value);
                            }
                            break;

                        case nameof(VaccumPumpPowerOnOff):
                            if(VaccumPumpPowerOnOff != null)
                            {
                                PLCService.WriteOutputCmd1(PLCService.OutputCmd1Index.VaccumPumpPower, VaccumPumpPowerOnOff.Value);
                            }
                            break;

                        case nameof(LineHeaterPowerOnOff):
                            if(LineHeaterPowerOnOff != null)
                            {
                                PLCService.WriteOutputCmd1(PLCService.OutputCmd1Index.LineHeaterPower, LineHeaterPowerOnOff.Value);
                            }
                            break;
                    }
                }
            };
            PLCConnectionState.Instance.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == nameof(PLCConnectionState.Online))
                {
                    if(PLCConnectionState.Instance.Online == true)
                    {
                        initializeSettingToPLC();
                    }
                }
            };
        }

        private static void PublishDeviceNameChanged(object? sender, string? propertyName, ObservableManager<(string, string)>.Publisher publisher)
        {
            if (propertyName == nameof(Device.Name))
            {
                Device? device = sender as Device;
                if (device != null && device.ID != null && device.Name != null)
                {
                    publisher.Publish((device.ID, device.Name));
                }
            }
        }

        public static int? ReadMaxValue(string id)
        {
            if (AnalogDeviceIDShortNameMap.TryGetValue(id, out var shortName) == true)
            {
                var found = dAnalogDeviceIO.Where((KeyValuePair<string, AnalogDeviceIO> analogDeviceIO) => shortName == analogDeviceIO.Key).Select((KeyValuePair<string, AnalogDeviceIO> analogDeviceIO) => analogDeviceIO.Value.MaxValue);
                if(0 < found.Count())
                {
                    return found.First();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static string? ReadFlowControllerDeviceName(string id)
        {
            if (AnalogDeviceIDShortNameMap.TryGetValue(id, out var shortName) == true)
            {
                var found = dAnalogDeviceIO.Where((KeyValuePair<string, AnalogDeviceIO> analogDeviceIO) => shortName == analogDeviceIO.Key).Select((KeyValuePair<string, AnalogDeviceIO> analogDeviceIO) => analogDeviceIO.Value.Name);
                if (0 < found.Count())
                {
                    return found.First();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static string? ReadValveDeviceName(string id)
        {
            var found = ValveDeviceIO.Where((Device device) => id == device.ID).Select((Device device) => device.Name);
            if (0 < found.Count())
            {
                return found.First();
            }
            else
            { 
                return null;
            }
        }

        public void AlarmSettingLoad()
        {
            PropertyChanged += (object? sender, PropertyChangedEventArgs args) =>
            {
                switch(args.PropertyName)
                {
                    case nameof(LogIntervalInRecipeRun):
                        if (LogIntervalInRecipeRun != null)
                        {
                            AppSetting.LogIntervalInRecipeRunInMS = int.Parse(LogIntervalInRecipeRun);
                        }
                        break;
                }
            };

            lAnalogDeviceIO = dAnalogDeviceIO?.Values.ToList();
            if (lAnalogDeviceIO != null)
            {
                foreach (var io in lAnalogDeviceIO)
                {
                    io.PropertyChanged += (object? sender, PropertyChangedEventArgs args) =>
                    {
                        PublishDeviceNameChanged(sender, args.PropertyName, AnalogIOLabelChangedPublisher);
                        AnalogDeviceIO? analogDevice = sender as AnalogDeviceIO;
                        if (analogDevice != null && analogDevice.ID != null)
                        {
                            switch (args.PropertyName)
                            {
                                case nameof(AnalogDeviceIO.AlarmSet):
                                    PLCService.WriteAnalogDeviceAlarmState(analogDevice.ID, analogDevice.AlarmSet);
                                    break;

                                case nameof(AnalogDeviceIO.WarningSet):
                                    PLCService.WriteAnalogDeviceWarningState(analogDevice.ID, analogDevice.WarningSet);
                                    break;
                            }
                        }
                    };
                }
            }
            lSwitchDI = dSwitchDI?.Values.ToList();
            if(lSwitchDI != null)
            {
                foreach(var io in lSwitchDI)
                {
                    io.PropertyChanged += (object? sender, PropertyChangedEventArgs args) =>
                    {
                        SwitchDI? switchID = sender as SwitchDI;
                        if (switchID != null && switchID.ID != null)
                        {
                            switch (args.PropertyName)
                            {
                                case nameof(SwitchDI.AlarmSet):
                                    PLCService.WriteDigitalDeviceAlarmState(switchID.ID, switchID.AlarmSet);
                                    break;

                                case nameof(SwitchDI.WarningSet):
                                    PLCService.WriteDigitalDeviceWarningState(switchID.ID, switchID.WarningSet);
                                    break;
                            }
                        }
                    };
                }
            }

            if (PLCConnectionState.Instance.Online == true)
            {
                initializeSettingToPLC();
            }
        }

        [RelayCommand]
        private void AnalogDeviceIOAllAlarmCheck()
        {
            if (lAnalogDeviceIO != null)
            {
                foreach (var analogIO in lAnalogDeviceIO)
                {
                    if (analogIO != null)
                    {
                        analogIO.AlarmSet = !analogIO.AlarmSet;
                    }
                }
            }
        }

        [RelayCommand]
        private void AnalogDeviceIOAllWarningCheck()
        {
            if (lAnalogDeviceIO != null)
            {
                foreach (var analogIO in lAnalogDeviceIO)
                {
                    if (analogIO != null)
                    {
                        analogIO.WarningSet = !analogIO.WarningSet;
                    }
                }
            }
        }

        [RelayCommand]
        private void DigitalDeviceIOAllAlarmCheck()
        {
            if (lSwitchDI != null)
            {
                foreach (var digitalIO in lSwitchDI)
                {
                    if (digitalIO != null)
                    {
                        digitalIO.AlarmSet = !digitalIO.AlarmSet;
                    }
                }
            }
        }

        [RelayCommand]
        private void DigitalDeviceIOAllWarningCheck()
        {
            if (lSwitchDI != null)
            {
                foreach (var digitalIO in lSwitchDI)
                {
                    if (digitalIO != null)
                    {
                        digitalIO.WarningSet = !digitalIO.WarningSet;
                    }
                }
                HieDigitalDeviceIOWarningCheckBoxGuidePlaceHolder();
            }
        }

        [RelayCommand]
        private void ShowDigitalDeviceIOWarningCheckBoxGuidePlaceHolder()
        {
            ShowDigitalDeviceAlarmGuideCheckBoxPlaceHolder = Visibility.Visible;
            ShowDigitalDeviceAlarmCheckBox = Visibility.Hidden;
        }

        [RelayCommand]
        private void HieDigitalDeviceIOWarningCheckBoxGuidePlaceHolder()
        {
            ShowDigitalDeviceAlarmGuideCheckBoxPlaceHolder = Visibility.Hidden;
            ShowDigitalDeviceAlarmCheckBox = Visibility.Visible;
        }

        [ObservableProperty]
        private Visibility _showDigitalDeviceAlarmCheckBox = Visibility.Visible;
        [ObservableProperty]
        private Visibility _showDigitalDeviceAlarmGuideCheckBoxPlaceHolder = Visibility.Hidden;

        public void AlarmSettingSave()
        {
            JToken jsonAnalogDeviceIO = JsonConvert.SerializeObject(dAnalogDeviceIO);
            JToken jValveDeviceIO = JsonConvert.SerializeObject(ValveDeviceIO);
            JToken jGasIO = JsonConvert.SerializeObject(GasIO);
            JToken jsonSwitchDI = JsonConvert.SerializeObject(dSwitchDI);
            JToken jInterLockD = JsonConvert.SerializeObject(dInterLockD);
            JToken jInterLockA = JsonConvert.SerializeObject(dInterLockA);
            JToken jAlarmDeviation = JsonConvert.SerializeObject(AlarmDeviationValue);
            JToken jWarningDeviation = JsonConvert.SerializeObject(WarningDeviationValue);
            JToken jAnalogDelayTime = JsonConvert.SerializeObject(AnalogDeviceDelayTimeValue);
            JToken jDigitalDelayTime = JsonConvert.SerializeObject(DigitalDeviceDelayTimeValue);


            JObject jDeviceIO = new(
                new JProperty("AlarmDeviation", jAlarmDeviation),
                new JProperty("WarningDeviation", jWarningDeviation),
                new JProperty("AlarmDelayTimeA", jAnalogDelayTime),
                new JProperty("AlarmDelayTimeD", jDigitalDelayTime),
                new JProperty("AnalogDeviceIO", jsonAnalogDeviceIO),
                new JProperty("ValveDeviceIO", jValveDeviceIO),
                new JProperty("GasIO", jGasIO),
                new JProperty("SwitchDI", jsonSwitchDI),
                new JProperty("InterLockD", jInterLockD),
                new JProperty("InterLockA", jInterLockA)
            );

            if (File.Exists(DevceIOSettingFilePath)) File.Delete(DevceIOSettingFilePath);
            File.WriteAllText(DevceIOSettingFilePath, jDeviceIO.ToString());
        }

        private void updateIOState(BitArray ioStateList)
        {
            int io = 0;
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.PowerResetSwitch];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.Cover_UpperLimit];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.Cover_LowerLimit];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CylinderAutoSwicthClosed];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CylinderAutoSwicthOpen];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.SMPS_24V480];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.SMPS_24V72];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.SMPS_15VPlus];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.SMPS_15VMinus];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CB_GraphiteHeater];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CB_ThermalBath];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CB_VaccumPump];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CB_LineHeater];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CB_RotationMotor];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CB_CoverLiftMotor];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CB_ThrottleValve];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CB_GasDetector];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CB_CabitnetLamp];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.CB_MFCPower];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.LineHeader1];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.LineHeader2];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.LineHeader3];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.LineHeader4];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.LineHeader5];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.LineHeader6];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.LineHeader7];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.LineHeader8];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.GasDetectorH2];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.GasDetectorH2S];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.GasDetectorH2Se];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.FireSensor];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ExternalScrubberFault];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ExternalH2GasCabinetFault];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ExternalH2SGasCabinetFault];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ExternalH2SeGasCabinetFault];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ExternalUserInputAlarm];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ThermalBath1_Deviaiton];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ThermalBath1_CutOff];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ThermalBath2_Deviaiton];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ThermalBath2_CutOff];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ThermalBath3_Deviaiton];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ThermalBath3_CutOff];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ThermalBath4_Deviaiton];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ThermalBath4_CutOff];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.SingalTower_RED];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.SingalTower_YELLOW];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.SingalTower_GREEN];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.SingalTower_BLUE];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.SingalTower_WHITE];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.SingalTower_BUZZER];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ClampLock];
            IOList[io++].OnOff = ioStateList[(int)PLCService.IOListIndex.ClampRelease];
        }

        public void initializeSettingToPLC()
        {
            PLCService.WriteDeviceMaxValue(lAnalogDeviceIO);
            PLCService.WriteAlarmWarningSetting(lAnalogDeviceIO ?? [], lSwitchDI ?? []);
                
            PLCService.WriteAlarmDeviationState(AlarmDeviation);
            PLCService.WriteWarningDeviationState(WarningDeviation);
            PLCService.WriteAnalogDeviceDelayTime(AnalogDeviceDelayTime);
            PLCService.CommitAnalogDeviceInterlockSettingToPLC();
            PLCService.WriteDigitalDeviceDelayTime(DigitalDeviceDelayTime);
            PLCService.CommitDigitalDeviceInterlockSettingToPLC();

            BitArray outputCmd1 = PLCService.ReadOutputCmd1();
            InductionHeaterPowerOnOff = outputCmd1[(int)PLCService.OutputCmd1Index.GraphiteHeaterPower];
            ThermalBathPowerOnOff = outputCmd1[(int)PLCService.OutputCmd1Index.ThermalBathPower];
            VaccumPumpPowerOnOff = outputCmd1[(int)PLCService.OutputCmd1Index.VaccumPumpPower];
            LineHeaterPowerOnOff = outputCmd1[(int)PLCService.OutputCmd1Index.LineHeaterPower];

            if (dInterLockA != null)
            {
                foreach ((string name, InterLockA interlockA) in dInterLockA)
                {
                    PLCService.InterlockEnableSetting interlockEnableSetting;
                    if (InterlockSettingNameToPLCInterlockEnableSettingEnum.TryGetValue(name, out interlockEnableSetting) == true)
                    {
                        PLCService.WriteInterlockEnableState(interlockA.IsEnable, interlockEnableSetting);
                          
                    }
                    PLCService.InterlockValueSetting interlockValueSetting;
                    if(InterlockSettingNameToPLCInterlockValueSettingEnum.TryGetValue(name, out interlockValueSetting) == true)
                    {
                        try
                        {
                            PLCService.WriteInterlockValueState(float.Parse(interlockA.Treshold), interlockValueSetting);
                        }
                        catch (ArgumentNullException) { }
                        catch (FormatException) { }
                        catch (OverflowException) { }
                    }
                }
            }
            if(dInterLockD != null)
            {
                foreach((string name, InterLockD interlockD) in dInterLockD)
                {
                    PLCService.InterlockEnableSetting? plcArg = null;
                    switch (name)
                    {
                        case "SusceptorRotationMotor":
                            plcArg = PLCService.InterlockEnableSetting.SusceptorRotationMotor;
                            break;
                    }
                    if(plcArg != null)
                    {
                        PLCService.WriteInterlockEnableState(interlockD.IsEnable, plcArg.Value);
                    }
                }
            }
            PLCService.CommitInterlockEnableToPLC();
            PLCService.CommitInterlockValueToPLC();
        }

        private static List<Device> CreateDefaultValveDeviceIO()
        {
            List<Device> valveDeviceIO = new List<Device>();

            foreach(string valveID in PLCService.ValveIDtoOutputSolValveIdx.Keys)
            {
                valveDeviceIO.Add(new () { ID = valveID, Name = valveID });
            }

            return valveDeviceIO;
        }

        private static List<Device> CreateDefaultGasIO()
        {
            return [new() { ID = "Gas1", Name = "H2"}, new() { ID = "Gas2", Name = "N2" }, new() { ID = "Gas3", Name = "NH3" }, new() { ID = "Gas4", Name = "SiH4" },
                new() { ID = "Source1", Name = "TEB" }, new() { ID = "Source2", Name = "TMAI"}, new() { ID = "Source3", Name = "TMIn"}, new() { ID = "Source4", Name = "TMGa"},
                new() { ID = "Source5", Name = "DTMGa"}, new() { ID = "Source6", Name = "Cp2Mg"}];
        }

        [RelayCommand]
        private void AnalogDeviceSettingSave()
        {
            if (PLCConnectionState.Instance.Online == true)
            {
                PLCService.CommitAnalogDeviceAlarmWarningSettingStateToPLC();
            }
            AlarmSettingSave();
        }
        [RelayCommand]
        private void DigitalDeviceSettingSave()
        {
            if (PLCConnectionState.Instance.Online == true)
            {
                PLCService.CommitDigitalDeviceAlarmWarningSettingStateToPLC();
            }
            AlarmSettingSave();
        }
        [RelayCommand]
        private void InterlockSettingSave()
        {
            if (PLCConnectionState.Instance.Online == true)
            {
                PLCService.CommitInterlockEnableToPLC();
                PLCService.CommitInterlockValueToPLC();
            }
            AlarmSettingSave();
        }
    }
}
