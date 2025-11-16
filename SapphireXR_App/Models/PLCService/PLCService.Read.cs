using SapphireXR_App.Enums;
using System.Windows;

namespace SapphireXR_App.Models
{
    public static partial class PLCService
    {
        private static void ReadStateFromPLC(object? sender, EventArgs e)
        {
            try
            {
                ReadCurrentValueFromPLC();
                PublishCurrentPLCState();

                foreach (Action task in AddOnPLCStateUpdateTask)
                {
                    task();
                }

                string exceptionStr = string.Empty;
                if (aMonitoring_PVs == null)
                {
                    if (exceptionStr != string.Empty)
                    {
                        exceptionStr += "\r\n";
                    }
                    exceptionStr += "aMonitoring_PVs is null in OnTick PLCService";
                }
                if (aOutputSolValve == null)
                {
                    if (exceptionStr != string.Empty)
                    {
                        exceptionStr += "\r\n";
                    }
                    exceptionStr += "baReadValveStatePLC1 is null in OnTick PLCService";
                }
                if (exceptionStr != string.Empty)
                {
                    throw new ReadBufferException(exceptionStr);
                }
            }
            catch (ReadBufferException exception)
            {
                if (ShowMessageOnOnTick == true)
                {
                    ShowMessageOnOnTick = MessageBox.Show("PLC로부터 상태 (Analog Device Control/Valve 상태)를 읽어오는데 실패했습니다. 이 메시지를 다시 표시하지 않으려면 Yes를 클릭하세요. 원인은 다음과 같습니다: " + exception.Message, "",
                        MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes ? false : true;
                }
            }
            catch (Exception)
            {
                Connected = PLCConnection.Disconnected;
            }
        }

        private static void ReadCurrentValueFromPLC()
        {
            foreach (KeyValuePair<string, int> kv in DeviceConfiguration.dIndexController)
            {
                aDeviceControlValues[kv.Value] = (float)Ads.ReadAny<double>(hAnalogControllers[kv.Value].hCV);
                aDeviceCurrentValues[kv.Value] = Ads.ReadAny<float>(hAnalogControllers[kv.Value].hPV);
            }
            aMonitoring_PVs = Ads.ReadAny<float[]>(hMonitoring_PV, [(int)DeviceConfiguration.NumMonitoring]);
            aGeneralIOStates = Ads.ReadAny<byte[]>(hGeneralIO.hGeneralIOState, [(int)DeviceConfiguration.NumGeneralIOState]);
            aOutputSolValve = Ads.ReadAny<bool[]>(hOutputSolValve, [(int)DeviceConfiguration.NumOutputSolValve]);
            aLogicalInterlocks = Ads.ReadAny<bool[]>(hInterlock.hLogicalInterlocks, [(int)DeviceConfiguration.NumLogicalInterlockState]);
        }

        public static float ReadCurrentValue(string controllerID)
        {
            return aDeviceCurrentValues[DeviceConfiguration.dIndexController[controllerID]];
        }

        public static short ReadUserState()
        {
            int length = userStateBuffer.Length;
            Ads.Read(hRecipe.hUserState, userStateBuffer);
            return BitConverter.ToInt16(userStateBuffer.Span);
        }

        public static bool ReadLineHeaterPowerState()
        {

        }
        public static bool ReadInductionHeaterPowerState()
        {

        }
        public static bool ReadThermalBathPowerState()
        {

        }

        public static bool ReadVaccumPumpPowerState()
        {
           
        }

  
        public static bool ReadPressureControlMode()
        {
           
        }

        public static byte ReadThrottleValveMode()
        {
           
        }

        public static bool ReadTempManAuto()
        {
           
        }

        public static bool ReadBit(int bitField, int bit)
        {
            int bitMask = 1 << bit;
            return ((bitField & bitMask) != 0) ? true : false;
        }

        public static bool ReadBuzzerOnOff()
        {
            return Ads.ReadAny<bool>(hInterlock.hInterlockEnables[2]);
        }

        public static bool[] ReadDeviceAlarms()
        {
            return Ads.ReadAny<bool[]>(hInterlock.hAlarmStates, [(int)DeviceConfiguration.NumInterlockAlarmWarningState]);
        }

        public static bool[] ReadDeviceWarnings()
        {
            return Ads.ReadAny<bool[]>(hInterlock.hWarningStates, [(int)DeviceConfiguration.NumInterlockAlarmWarningState]);
        }

        public static bool ReadRecipeStartAvailable()
        {
            if(aLogicalInterlocks == null)
            {
                throw new InvalidOperationException("Failed in reading Logical interlocks from PLC yet.");
            }
            return aLogicalInterlocks[DeviceConfiguration.LogicalInterlockRecipeEnable];
        }

        public static bool ReadAlarmTriggered()
        {
            if (aLogicalInterlocks == null)
            {
                throw new InvalidOperationException("Failed in reading Logical interlocks from PLC yet.");
            }
            return aLogicalInterlocks[DeviceConfiguration.LogicalInterlockAlarmTriggered];
        }

        public static ControlMode ReadControlMode()
        {
            return (ControlMode)(Ads.ReadAny<short>(hControlMode));
        }

        public static float ReadFlowControllerTargetValue(string controllerID)
        {
            return Ads.ReadAny<RampGeneratorInput>(hAnalogControllers[DeviceConfiguration.dIndexController[controllerID]].hCVControllerInput).targetValue;
        }

        public static short ReadCurrentStep()
        {
            return Ads.ReadAny<short>(hRecipe.hRcpStepN);
        }
    }
}
