using SapphireXR_App.ViewModels;
using System.Collections;

namespace SapphireXR_App.Models
{
    public static partial class PLCService
    {
        private static void DoWriteValveState(BitArray valveUpdate)
        {
            uint[] sentBuffer = new uint[1];
            valveUpdate.CopyTo(sentBuffer, 0);
            Ads.WriteAny(hOutputSolValve, sentBuffer, [1]);
        }

        public static void WriteValveState(string valveID, bool onOff)
        {
            if (DeviceConfiguration.ValveIDtoOutputSolValveIdx.TryGetValue(valveID, out int index) == true)
            {
                Ads.WriteAny(hOutputSolValveElem[index], onOff);
            }
        }

        public static void WriteValveState(bool[] valveUpdate)
        {
            Ads.WriteAny(hOutputSolValve, valveUpdate, [(int)DeviceConfiguration.NumOutputSolValve]);
        }

        public static void WriteRecipe(PlcRecipe[] recipe)
        {
            Ads.WriteAny(hRecipe.hRcp, recipe, [recipe.Length]);
        }

        public static void RefreshRecipe(PlcRecipe[] updates)
        {
            foreach (PlcRecipe recipe in updates)
            {
                Ads.WriteAny(Ads.CreateVariableHandle("RCP.aRecipe[" + recipe.aRecipeShort[0] + "]"), recipe);
            }
        }

        public static void WriteTotalStep(short totalStep)
        {
            Ads.WriteAny(hRecipe.hRcpTotalStep, totalStep);
        }

        public static void WriteRCPOperationCommand(short operationState)
        {
            Ads.WriteAny(hRecipe.hCmd_RcpOperation, operationState);
        }

        public static void WriteControlModeCmd(ControlMode controlMode)
        {
            dControlModeChangingPublisher?.Publish(controlMode);
            Ads.WriteAny(hControlModeCmd, (short)controlMode);
        }

        public static void WriteGeneralDeviceIOControl(DeviceConfiguration.OutputCmd1Index index, bool powerOn)
        {
          
        }

        public static void WriteThrottleValveMode(short value)
        {
        
        }

        private static int SetBit(bool bitValue, int bitField, int bit)
        {
            int invMask = ~(1 << bit);
            bitField &= invMask;
            bitField |= (bitValue ? 1 : 0) << bit;

            return bitField;
        }

        private static void WriteAnalogAlarmEnable(Dictionary<int, bool> interlockEnableIndiceToCommit)
        {
            foreach((int index, bool enable) in interlockEnableIndiceToCommit)
            {
                Ads.WriteAny(hInterlock.hAnalogAlarmEnables[index], enable);
            }
        }

        private static void WriteAnalogWarningEnable(Dictionary<int, bool> interlockEnableIndiceToCommit)
        {
            foreach ((int index, bool enable) in interlockEnableIndiceToCommit)
            {
                Ads.WriteAny(hInterlock.hAnalogWarningEnables[index], enable);
            }
        }

        private static void WriteDigitalAlarmEnable(Dictionary<int, bool> interlockEnableIndiceToCommit)
        {
            foreach ((int index, bool enable) in interlockEnableIndiceToCommit)
            {
                Ads.WriteAny(hInterlock.hDigitalAlarmEnables[index], enable);
            }
        }

        private static void WriteDigitalWarningEnable(Dictionary<int, bool> interlockEnableIndiceToCommit)
        {
            foreach ((int index, bool enable) in interlockEnableIndiceToCommit)
            {
                Ads.WriteAny(hInterlock.hDigitalWarningEnables[index], enable);
            }
        }

        public static void WriteAlarmWarningSetting(List<AnalogDeviceIO> analogDeviceIOs, List<SwitchDI> switchDIs)
        {
            var setEnable = (string deviceID, bool bitValue, Dictionary<string, int> deviceIDToIndex, uint[] enableHandles) =>
            {
                if (deviceIDToIndex.TryGetValue(deviceID, out int index) == true)
                {
                    Ads.WriteAny(enableHandles[index], bitValue);
                }
            };

            foreach (AnalogDeviceIO analogDeviceIO in analogDeviceIOs)
            {
                if (analogDeviceIO.ID != null)
                {
                    setEnable(analogDeviceIO.ID, analogDeviceIO.AlarmSet, DeviceConfiguration.dAnalogDeviceAlarmWarningBit, hInterlock.hAnalogAlarmEnables);
                    setEnable(analogDeviceIO.ID, analogDeviceIO.WarningSet, DeviceConfiguration.dAnalogDeviceAlarmWarningBit, hInterlock.hAnalogWarningEnables);
                }
            }
            foreach (SwitchDI switchID in switchDIs)
            {
                if (switchID.ID != null)
                {
                    setEnable(switchID.ID, switchID.AlarmSet, DeviceConfiguration.dDigitalDeviceAlarmWarningBit, hInterlock.hDigitalAlarmEnables);
                    setEnable(switchID.ID, switchID.WarningSet, DeviceConfiguration.dDigitalDeviceAlarmWarningBit, hInterlock.hDigitalWarningEnables);
                }
            }
        }

        public static void WriteAlarmDeviationState(float deviation)
        {
            AnalogDeviceInterlockSetIndiceToCommit[0] = deviation;
        }

        public static void WriteWarningDeviationState(float deviation)
        {
            AnalogDeviceInterlockSetIndiceToCommit[1] = deviation;
        }

        public static void WriteAnalogDeviceDelayTime(float delayTime)
        {
            AnalogDeviceInterlockSetIndiceToCommit[2] = delayTime;
        }

        public static void WriteDigitalDeviceDelayTime(float delayTime)
        {
            DigitalDevicelnterlockSetToCommit = (false, delayTime);
        }

        public static void CommitAnalogDeviceInterlockSettingToPLC()
        {
            CommitInterlockSetToPLC(AnalogDeviceInterlockSetIndiceToCommit);
        }

        public static void CommitDigitalDeviceInterlockSettingToPLC()
        {
            if (DigitalDevicelnterlockSetToCommit.Item1 == false)
            {
                Ads.WriteAny(hInterlockset[3], DigitalDevicelnterlockSetToCommit.Item2);
                DigitalDevicelnterlockSetToCommit.Item1 = true;
            }
        }

        private static void CommitInterlockSetToPLC(Dictionary<int, float> interlockSetIndiceToCommit)
        {
            foreach ((int index, float setValue) in interlockSetIndiceToCommit)
            {
                Ads.WriteAny(hInterlockset[index], setValue);
            }
            interlockSetIndiceToCommit.Clear();
        }

        private static void WriteFirstInterlockSetting(bool onOff, int bit)
        {
            InterlockEnables[0] = SetBit(onOff, InterlockEnables[0], bit);
            Ads.WriteAny(hInterlockEnable[0], InterlockEnables[0]);
        }

        public static void WriteBuzzerOnOff(bool onOff)
        {
            WriteFirstInterlockSetting(onOff, 2);
        }

        public static void WriteInterlockEnableState(bool onOff, DeviceConfiguration.InterlockEnableSetting interlockEnableSetting)
        {
            InterlockEnables[0] = SetBit(onOff, InterlockEnables[0], (int)interlockEnableSetting);
        }

        public static void CommitInterlockEnableToPLC()
        {
            Ads.WriteAny(hInterlockEnable[0], InterlockEnables[0]);
        }

        public static void WriteInterlockValueState(float value, DeviceConfiguration.InterlockValueSetting interlockEnableSetting)
        {
            InterlockSetIndiceToCommit[((int)interlockEnableSetting) - 1] = value;
        }

        public static void CommitInterlockValueToPLC()
        {
            CommitInterlockSetToPLC(InterlockSetIndiceToCommit);
        }

        public static void WriteAlarmReset()
        {
            WriteFirstInterlockSetting(true, 0);
        }

        public static void WriteWarningReset()
        {
            WriteFirstInterlockSetting(true, 1);
        }

        public static void WriteFlowControllerTargetValue((string, float?)[] aControllerIdTargetValues, short rampTime)
        {
            foreach ((string id, float? targetValue) in aControllerIdTargetValues)
            {
                if (targetValue != null)
                {
                    WriteFlowControllerTargetValue(id, targetValue.Value, rampTime);
                }
            }
        }

        public static void WriteFlowControllerTargetValue(string controllerID, float targetValue, short rampTime)
        {
            int controllerIDIndex = DeviceConfiguration.dIndexController[controllerID];
            switch (controllerIDIndex)
            {
                case 16:
                    Ads.WriteAny(hAnalogControllers[controllerIDIndex].hCVControllerInput, new RampGeneratorInput { restart = true, rampTime = (ushort)rampTime, targetValue = targetValue });
                    temperatureTVPublisher?.Publish(targetValue);
                    break;

                case 17:
                    Ads.WriteAny(hAnalogControllers[controllerIDIndex].hCVControllerInput, new RampGeneratorInput { restart = true, rampTime = (ushort)rampTime, targetValue = targetValue });
                    pressureTVPublisher?.Publish(targetValue);
                    break;

                case 18:
                    Ads.WriteAny(hAnalogControllers[controllerIDIndex].hCVControllerInput, new RampGeneratorInput { restart = true, rampTime = (ushort)rampTime, targetValue = targetValue });
                    rotationTVPublisher?.Publish(targetValue);
                    break;

                default:
                    int? maxValue = SettingViewModel.ReadMaxValue(controllerID);
                    if (maxValue == null)
                    {
                        throw new ArgumentException(controllerID + " is invalid");
                    }
                    Ads.WriteAny(hAnalogControllers[controllerIDIndex].hCVControllerInput, new RampGeneratorInput { restart = true, rampTime = (ushort)rampTime, targetValue = ((float)targetValue / (float)maxValue * DeviceConfiguration.AnalogControllerOutputVoltage) });
                    break;
            }
        }

        public static void WriteLineHeaterTargetValue(int lineHeaterNum, float targetValue)
        {
            Ads.WriteAny(hInterlockset[lineHeaterNum + 11], targetValue);
        }

        public static void WriteReactorMaxValue(DeviceConfiguration.Reactor reactor, float maxValue)
        {
            ReactorMaxValueToCommit[reactor] = maxValue;
        }

        public static void CommitReactorMaxValueToPLC()
        {
            foreach ((DeviceConfiguration.Reactor reactor, float maxValue) in ReactorMaxValueToCommit)
            {
                Ads.WriteAny(hMaxValues[(int)reactor], maxValue);
            }
            ReactorMaxValueToCommit.Clear();
        }
    }
}
