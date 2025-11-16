using SapphireXR_App.Common;
using SapphireXR_App.Enums;
using System.Collections;
using System.Windows.Threading;
using TwinCAT.Ads;
using TwinCAT.PlcOpen;

namespace SapphireXR_App.Models
{
    public static partial class PLCService
    {
        static PLCService()
        {
            IntializePubSub();
        }

        public static bool Connect()
        {
            if (Ads.IsConnected == true && Ads.ReadState().AdsState == AdsState.Run)
            {
                return true;
            }
          
            DateTime startTime = DateTime.Now;
            while (true)
            {
                if(TryConnect() == true)
                {
                    return true;
                }
                else
                {
                    if ((DateTime.Now - startTime).TotalMilliseconds < AppSetting.ConnectionRetryMilleseconds)
                    {
                        continue;
                    }
                    else
                    {
                        Connected = PLCConnection.Disconnected;
                        return false;
                    }
                }
            }
        }

        private static bool TryConnect()
        {
            try
            {
                if (AppSetting.PLCAddress != "Local")
                {
                    Ads.Connect(new AmsAddress(AppSetting.PLCAddress + ":" + AppSetting.PLCPort));
                }
                else
                {
                    Ads.Connect(AmsNetId.Local, AppSetting.PLCPort);
                }

                if (Ads.IsConnected == true && Ads.ReadState().AdsState == AdsState.Run)
                {
                    CreateHandle();
                    ReadCurrentValueFromPLC();

                    Connected = PLCConnection.Connected;
                    
                    PublishCurrentPLCState();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void OnConnected()
        {
            connectionTryTimer?.Stop();
            TryConnectAsync = null;

            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(2000000);
                timer.Tick += ReadStateFromPLC;
            }
            timer.Start();

            void listenCurentActiveRecipe()
            {
                try
                {
                    dCurrentActiveRecipeIssue?.Publish(Ads.ReadAny<short>(hRecipe.hRcpStepN));
                    dRecipeControlPauseTimeIssuer?.Publish(Ads.ReadAny<TIME>(hRecipe.hRecipeControlPauseTime).Time.Seconds);
                    RecipeRunET recipeRunET = Ads.ReadAny<RecipeRunET>(hRecipe.hRecipeRunET);
                    dRecipeRunElapsedTimeIssuer?.Publish((recipeRunET.ElapsedTime / 1000, recipeRunET.Mode));
                    if (RecipeRunEndNotified == false && Ads.ReadAny<short>(hRecipe.hCmd_RcpOperation) == 50)
                    {
                        dRecipeEndedPublisher?.Publish(true);
                        RecipeRunEndNotified = true;
                    }
                    else
                        if (RecipeRunEndNotified == true && Ads.ReadAny<short>(hRecipe.hCmd_RcpOperation) == 0)
                    {
                        RecipeRunEndNotified = false;
                    }
                }
                catch (Exception)
                {
                    Connected = PLCConnection.Disconnected;
                }
            }
            if (currentActiveRecipeListener == null)
            {
                currentActiveRecipeListener = new DispatcherTimer();
                currentActiveRecipeListener.Interval = new TimeSpan(TimeSpan.TicksPerMillisecond * 500);
                currentActiveRecipeListener.Tick += (object? sender, EventArgs e) =>listenCurentActiveRecipe();
            }
            listenCurentActiveRecipe();
            currentActiveRecipeListener.Start();
        }

        
        private static void OnDisconnected()
        {
            timer?.Stop();
            currentActiveRecipeListener?.Stop();

            if (connectionTryTimer == null)
            { 
                connectionTryTimer = new DispatcherTimer();
                connectionTryTimer.Interval = new TimeSpan(TimeSpan.TicksPerMillisecond);
                connectionTryTimer.Tick += (object? sender, EventArgs e) =>
                {
                    if(TryConnectAsync == null || (TryConnectAsync.IsCompleted == true && TryConnectAsync.Result == false))
                    {
                        TryConnectAsync = Task.Delay(1000).ContinueWith((task) => TryConnect(), TaskScheduler.FromCurrentSynchronizationContext());
                    }
                };
            }
            connectionTryTimer.Start();
        }

        private static void CreateHandle()
        {
            for (uint analogDevice = 0; analogDevice < DeviceConfiguration.NumControllers; ++analogDevice)
            {
                string hACName = "GVL_INTERFACE_UI.aAnalogController[" + (analogDevice + 1) + "]";
                hAnalogControllers[analogDevice] = new()
                {
                    hPV = Ads.CreateVariableHandle(hACName + ".pv"),
                    hCV = Ads.CreateVariableHandle(hACName + ".cvController.rControlValue"),
                    hCVControllerInput = Ads.CreateVariableHandle(hACName + ".cvController.input")
                };
                hMaxValues[analogDevice] = Ads.CreateVariableHandle("GVL_INTERFACE_UI.aMaxValue[" + (analogDevice + 1) + "]");
            }

            hOutputSolValve = Ads.CreateVariableHandle("GVL_INTERFACE_UI.aOutputSolValve");
            for(uint valve = 0; valve < hOutputSolValveElem.Length; ++valve)
            {
                hOutputSolValveElem[valve] = Ads.CreateVariableHandle("GVL_INTERFACE_UI.aOutputSolValve[" + (valve + 1) + "]");
            }
            hMonitoring_PV = Ads.CreateVariableHandle("GVL_INTERFACE_UI.aMonitoring_PV");

            hGeneralIO.hGeneralIOControl = Ads.CreateVariableHandle("GVL_INTERFACE_UI.generalIO.control");
            hGeneralIO.hGeneralIOState = Ads.CreateVariableHandle("GVL_INTERFACE_UI.generalIO.state");
            hGeneralIO.hGeneralIOStateTempManAuto = Ads.CreateVariableHandle($"GVL_INTERFACE_UI.generalIO.state[{DeviceConfiguration.GeneralIOStateTempManAuto + 1}]" );
            hOutputCmd1 = Ads.CreateVariableHandle("GVL_INTERFACE_UI.aOutputCmd[1]");
            hOutputCmd2 = Ads.CreateVariableHandle("GVL_INTERFACE_UI.aOutputCmd[2]");

            for(uint hInterlockEnable = 0; hInterlockEnable < hInterlock.hInterlockEnables.Length; ++hInterlockEnable)
            {
                hInterlock.hInterlockEnables[hInterlockEnable] = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockSetting.aInterlockEnable[" + (hInterlockEnable + 1) + "]");
            }
            for (uint hInterlockEnableThreshold = 0; hInterlockEnableThreshold < hInterlock.hInterlockEnableThresholds.Length; ++hInterlockEnableThreshold)
            {
                hInterlock.hInterlockEnableThresholds[hInterlockEnableThreshold] = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockSetting.aInterlockEnableThreshold[" + (hInterlockEnableThreshold + 1) + "]");
            }
            for (uint hAnalogAlarmEnable = 0; hAnalogAlarmEnable < hInterlock.hAnalogAlarmEnables.Length; ++hAnalogAlarmEnable)
            {
                hInterlock.hAnalogAlarmEnables[hAnalogAlarmEnable] = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockSetting.aAnalogAlarmEnable[" + (hAnalogAlarmEnable + 1) + "]");
            }
            for (uint hAnalogWarningEnable = 0; hAnalogWarningEnable < hInterlock.hAnalogWarningEnables.Length; ++hAnalogWarningEnable)
            {
                hInterlock.hAnalogWarningEnables[hAnalogWarningEnable] = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockSetting.aAnalogWarningEnable[" + (hAnalogWarningEnable + 1) + "]");
            }
            for (uint hDigitalAlarmEnable = 0; hDigitalAlarmEnable < hInterlock.hDigitalAlarmEnables.Length; ++hDigitalAlarmEnable)
            {
                hInterlock.hDigitalAlarmEnables[hDigitalAlarmEnable] = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockSetting.aDigitalAlarmEnable[" + (hDigitalAlarmEnable + 1) + "]");
            }
            for (uint hDigitalWarningEnable = 0; hDigitalWarningEnable < hInterlock.hDigitalWarningEnables.Length; ++hDigitalWarningEnable)
            {
                hInterlock.hDigitalWarningEnables[hDigitalWarningEnable] = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockSetting.aDigitalWarningEnable[" + (hDigitalWarningEnable + 1) + "]");
            }
            hInterlock.hLogicalInterlocks = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockState.aLogicalInterlock");
            hInterlock.hAlarmStates = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockState.aAlarmState");
            hInterlock.hWarningStates = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockState.aWarningState");
            hInterlock.hRecipeEnableSubstate = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockState.aInterlockSubstate.recipeEnableSubstate");
            hInterlock.hOpenReactorSubstate = Ads.CreateVariableHandle("GVL_INTERFACE_UI.interlock.interlockState.aInterlockSubstate.openReactorSubstate");


            hRecipe.hRcp = Ads.CreateVariableHandle("GVL_INTERFACE_UI.recipe.aRecipe");
            hRecipe.hRcpTotalStep = Ads.CreateVariableHandle("GVL_INTERFACE_UI.recipe.iRcpTotalStep");
            hRecipe.hCmd_RcpOperation = Ads.CreateVariableHandle("GVL_INTERFACE_UI.recipe.control.cmd_RcpOperation");
            hRecipe.hRcpStepN = Ads.CreateVariableHandle("GVL_INTERFACE_UI.recipe.control.iRcpStepN");
            hRecipe.hUserState = Ads.CreateVariableHandle("GVL_INTERFACE_UI.recipe.control.userState");
            hRecipe.hRecipeControlPauseTime = Ads.CreateVariableHandle("GVL_INTERFACE_UI.recipe.control.Pause_ET");
            hRecipe.hRecipeRunET = Ads.CreateVariableHandle("GVL_INTERFACE_UI.recipe.control.RecipeRunET");

            hControlModeCmd = Ads.CreateVariableHandle("GVL_INTERFACE_UI.mainPRGControl.controlModeCmd");
            hControlMode = Ads.CreateVariableHandle("GVL_INTERFACE_UI.mainPRGControl.controlMode");
        }

        private static void IntializePubSub()
        {
            dCurrentValueIssuers = new Dictionary<string, ObservableManager<float>.Publisher>();
            foreach (KeyValuePair<string, int> kv in DeviceConfiguration.dIndexController)
            {
                dCurrentValueIssuers.Add(kv.Key, ObservableManager<float>.Get("FlowControl." + kv.Key + ".CurrentValue"));
            }
            dControlValueIssuers = new Dictionary<string, ObservableManager<float>.Publisher>();
            foreach (KeyValuePair<string, int> kv in DeviceConfiguration.dIndexController)
            {
                dControlValueIssuers.Add(kv.Key, ObservableManager<float>.Get("FlowControl." + kv.Key + ".ControlValue"));
            }
            dTargetValueIssuers = new Dictionary<string, ObservableManager<float>.Publisher>();
            foreach (KeyValuePair<string, int> kv in DeviceConfiguration.dIndexController)
            {
                dTargetValueIssuers.Add(kv.Key, ObservableManager<float>.Get("FlowControl." + kv.Key + ".TargetValue"));
            }
            dControlCurrentValueIssuers = new Dictionary<string, ObservableManager<(float, float)>.Publisher>();
            foreach (KeyValuePair<string, int> kv in DeviceConfiguration.dIndexController)
            {
                dControlCurrentValueIssuers.Add(kv.Key, ObservableManager<(float, float)>.Get("FlowControl." + kv.Key + ".ControlTargetValue.CurrentPLCState"));
            }
            aMonitoringCurrentValueIssuers = new Dictionary<string, ObservableManager<float>.Publisher>();
            foreach (KeyValuePair<string, int> kv in DeviceConfiguration.dIndexController)
            {
                aMonitoringCurrentValueIssuers.Add(kv.Key, ObservableManager<float>.Get("MonitoringPresentValue." + kv.Key + ".CurrentValue"));
            }
            dValveStateIssuers = new Dictionary<string, ObservableManager<bool>.Publisher>();
            foreach ((string valveID, int valveIndex) in DeviceConfiguration.ValveIDtoOutputSolValveIdx)
            {
                dValveStateIssuers.Add(valveID, ObservableManager<bool>.Get("Valve.OnOff." + valveID + ".CurrentPLCState"));
            }
            dCurrentActiveRecipeIssue = ObservableManager<short>.Get("RecipeRun.CurrentActiveRecipe");
            baHardWiringInterlockStateIssuers = ObservableManager<BitArray>.Get("HardWiringInterlockState");
            dIOStateList = ObservableManager<BitArray>.Get("DeviceIOList");
            dRecipeEndedPublisher = ObservableManager<bool>.Get("RecipeEnded");
            dLineHeaterTemperatureIssuers = ObservableManager<float[]>.Get("LineHeaterTemperature");
            dRecipeControlPauseTimeIssuer = ObservableManager<int>.Get("RecipeControlTime.Pause");
            dRecipeRunElapsedTimeIssuer = ObservableManager<(int, RecipeRunETMode)>.Get("RecipeRun.ElapsedTime");
            dDigitalOutput2 = ObservableManager<BitArray>.Get("DigitalOutput2");
            dDigitalOutput3 = ObservableManager<BitArray>.Get("DigitalOutput3");
            dOutputCmd1 = ObservableManager<BitArray>.Get("OutputCmd1");
            dThrottleValveControlMode = ObservableManager<short>.Get("ThrottleValveControlMode");
            dPressureControlModeIssuer = ObservableManager<ushort>.Get("PressureControlMode");
            dThrottleValveStatusIssuer = ObservableManager<short>.Get("ThrottleValveStatus");
            dLogicalInterlockStateIssuer = ObservableManager<BitArray>.Get("LogicalInterlockState");
            dPLCConnectionPublisher = ObservableManager<PLCConnection>.Get("PLCService.Connected");
            dControlModeChangingPublisher = ObservableManager<ControlMode>.Get("ControlModeChanging");
            temperatureTVPublisher = ObservableManager<float>.Get("TemperatureTV");
            pressureTVPublisher = ObservableManager<float>.Get("PressureTV");
            rotationTVPublisher = ObservableManager<float>.Get("RotationTV");
            recipeEnableSubConditionPublisher = ObservableManager<BitArray>.Get("RecipeEnableSubCondition");
            reactorEnableSubConditionPublisher = ObservableManager<BitArray>.Get("ReactorEnableSubCondition");
        }

        private static void PublishCurrentPLCState()
        {
            if (aDeviceControlValues != null)
            {
                foreach (KeyValuePair<string, int> kv in DeviceConfiguration.dIndexController)
                {
                    dControlValueIssuers?[kv.Key].Publish(aDeviceControlValues[DeviceConfiguration.dIndexController[kv.Key]]);
                }
            }
            if (aDeviceCurrentValues != null)
            {
                foreach (KeyValuePair<string, int> kv in DeviceConfiguration.dIndexController)
                {
                    dCurrentValueIssuers?[kv.Key].Publish(aDeviceCurrentValues[DeviceConfiguration.dIndexController[kv.Key]]);
                }
            }
            if (aDeviceControlValues != null && aDeviceCurrentValues != null)
            {
                foreach (KeyValuePair<string, int> kv in DeviceConfiguration.dIndexController)
                {
                    dControlCurrentValueIssuers?[kv.Key].Publish((aDeviceCurrentValues[DeviceConfiguration.dIndexController[kv.Key]], aDeviceControlValues[DeviceConfiguration.dIndexController[kv.Key]]));
                }
            }

            if (aMonitoring_PVs != null)
            {
                foreach (KeyValuePair<string, int> kv in DeviceConfiguration.dMonitoringMeterIndex)
                {
                    aMonitoringCurrentValueIssuers?[kv.Key].Publish(aMonitoring_PVs[kv.Value]);
                }
            }

            if (aGeneralIOStates != null)
            {
                short value = aGeneralIOStates[0];
                baHardWiringInterlockStateIssuers?.Publish(new BitArray(BitConverter.IsLittleEndian == true ? BitConverter.GetBytes(value) : BitConverter.GetBytes(value).Reverse().ToArray()));
                dThrottleValveStatusIssuer?.Publish(aGeneralIOStates[4]);

                bool[] ioList = new bool[80];
                for (int inputState = 1; inputState < aGeneralIOStates.Length; ++inputState)
                {
                    new BitArray(BitConverter.IsLittleEndian == true ? BitConverter.GetBytes(aGeneralIOStates[inputState]) : BitConverter.GetBytes(aGeneralIOStates[inputState]).Reverse().ToArray()).CopyTo(ioList, (inputState - 1) * sizeof(short) * 8);
                }
                dIOStateList?.Publish(new BitArray(ioList));
            }

            if (aOutputSolValve != null)
            {
                foreach ((string valveID, int index) in DeviceConfiguration.ValveIDtoOutputSolValveIdx)
                {
                    dValveStateIssuers?[valveID].Publish(aOutputSolValve[index]);
                }
            }

            byte[] digitalOutput = Ads.ReadAny<byte[]>(hDigitalOutput, [4]);
            dDigitalOutput2?.Publish(new BitArray(new byte[1] { digitalOutput[1] }));
            dDigitalOutput3?.Publish(new BitArray(new byte[1] { digitalOutput[2] }));
            short[] outputCmd = Ads.ReadAny<short[]>(hGeneralIOState, [3]);
            dOutputCmd1?.Publish(bOutputCmd1 = new BitArray(BitConverter.IsLittleEndian == true ? BitConverter.GetBytes(outputCmd[0]) : BitConverter.GetBytes(outputCmd[0]).Reverse().ToArray()));
            dThrottleValveControlMode?.Publish(outputCmd[1]);
            dPressureControlModeIssuer?.Publish(Ads.ReadAny<ushort>(hOutputSetType));
            dLineHeaterTemperatureIssuers?.Publish(Ads.ReadAny<float[]>(hTemperaturePV, [(int)DeviceConfiguration.LineHeaterTemperature]));

            int iterlock1 = Ads.ReadAny<int>(hInterlock[0]);
            dLogicalInterlockStateIssuer?.Publish(new BitArray(BitConverter.IsLittleEndian == true ? BitConverter.GetBytes(iterlock1) : BitConverter.GetBytes(iterlock1).Reverse().ToArray()));

            short recipeEnableSubconditions = Ads.ReadAny<short>(hUIInterlockCheckRecipeEnable);
            recipeEnableSubConditionPublisher?.Publish(new BitArray(BitConverter.IsLittleEndian == true ? BitConverter.GetBytes(recipeEnableSubconditions) : BitConverter.GetBytes(recipeEnableSubconditions).Reverse().ToArray()));

            short reactorEnableSubconditions = Ads.ReadAny<short>(hUIInterlockCheckReactorEnable);
            reactorEnableSubConditionPublisher?.Publish(new BitArray(BitConverter.IsLittleEndian == true ? BitConverter.GetBytes(reactorEnableSubconditions) : BitConverter.GetBytes(reactorEnableSubconditions).Reverse().ToArray()));
        }

        public static void AddPLCStateUpdateTask(Action task)
        {
            AddOnPLCStateUpdateTask.Add(task);
        }

        public static void RemovePLCStateUpdateTask(Action task)
        {
            AddOnPLCStateUpdateTask.Remove(task);
        }
    }
}
