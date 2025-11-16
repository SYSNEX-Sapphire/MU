using SapphireXR_App.Common;
using System.Collections;
using System.Windows.Threading;
using TwinCAT.Ads;
using SapphireXR_App.Enums;
using System.Runtime.InteropServices;

namespace SapphireXR_App.Models
{
    public static partial class PLCService
    {
        internal class ReadValveStateException : Exception
        {
            public ReadValveStateException(string message) : base(message) { }
        }

        internal class ConnectionFaiulreException : Exception
        {
            public ConnectionFaiulreException(string internalMessage) : base("PLC로의 연결이 실패했습니다. 물리적 연결이나 서비스가 실행 중인지 확인해 보십시요." +
                (internalMessage != string.Empty ? "문제의 원인은 다음과 같습니다: " + internalMessage : internalMessage)) { }
        }

        private class ReadBufferException : Exception
        {
            public ReadBufferException(string message) : base(message) { }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct RecipeRunET
        {
            public int ElapsedTime;
            public RecipeRunETMode Mode;
        }

        public struct RampGeneratorInput
        {
            public bool restart;
            public float targetValue; // REAL in PLC is a 32-bit floating point, which is 'float' in C#
            public ushort rampTime;     // UINT in PLC is a 16-bit unsigned integer, which is 'ushort' in C#
        }

        private struct AnalogControllerHandle
        {
            public uint hPV;
            public uint hCV;
            public uint hCVControllerInput;
        }

        private struct InterelockHandle
        {
            public InterelockHandle() { }

            public uint[] hInterlockEnableThresholds = new uint[DeviceConfiguration.NumInterlockSettingEnableThreshold];
            public uint[] hInterlockEnables = new uint[DeviceConfiguration.NumInterlockSettingEnable];
            public uint[] hAnalogAlarmEnables = new uint[DeviceConfiguration.NumInterlockSettingAnalogEnable];
            public uint[] hAnalogWarningEnables = new uint[DeviceConfiguration.NumInterlockSettingAnalogEnable];
            public uint[] hDigitalAlarmEnables = new uint[DeviceConfiguration.NumInterlockSettingDigitalEnable];
            public uint[] hDigitalWarningEnables = new uint[DeviceConfiguration.NumInterlockSettingDigitalEnable];

            public uint hLogicalInterlocks;
            public uint hAlarmStates;
            public uint hWarningStates;
            public uint hRecipeEnableSubstate;
            public uint hOpenReactorSubstate;
        }

        private struct RecipeHandle
        {
            public uint hRcp;
            public uint hRcpTotalStep;
            public uint hRcpStepN;
            public uint hCmd_RcpOperation;
            public uint hUserState;
            public uint hRecipeRunET;
            public uint hRecipeControlPauseTime;
        }

        private struct GeneralIOHandle
        {
            public uint hGeneralIOControl;
            public uint hGeneralIOState;
            public uint hGeneralIOStateTempManAuto;
        }

        internal enum RecipeRunETMode : short
        {
            None = 0, Ramp = 1, Hold = 2
        };

        public enum ControlMode : short
        {
            Manual = 0, Recipe = 1, Priority = 2
        };

        public enum OutputSetType : ushort
        {
            Pressure = 1, Position = 2
        }

        public enum TriggerType { Alarm = 0, Warning };

        // Variable handles to be connected plc variables
        private static bool[]? aOutputSolValve = null;
        private static float[] aDeviceCurrentValues = new float[DeviceConfiguration.NumControllers];
        private static float[] aDeviceControlValues = new float[DeviceConfiguration.NumControllers];
        private static bool[] aLogicalInterlocks = null;
        private static float[]? aMonitoring_PVs = null;
        private static byte[]? aGeneralIOStates = null;
        private static BitArray? bOutputCmd1 = null;
        private static Memory<byte> userStateBuffer = new Memory<byte>([ 0x00, 0x00 ]);

        private static Dictionary<string, ObservableManager<float>.Publisher>? dCurrentValueIssuers;
        private static Dictionary<string, ObservableManager<float>.Publisher>? dControlValueIssuers;
        private static Dictionary<string, ObservableManager<float>.Publisher>? dTargetValueIssuers;
        private static Dictionary<string, ObservableManager<(float, float)>.Publisher>? dControlCurrentValueIssuers;
        private static Dictionary<string, ObservableManager<float>.Publisher>? aMonitoringCurrentValueIssuers;
        private static ObservableManager<BitArray>.Publisher? baHardWiringInterlockStateIssuers;
        private static ObservableManager<BitArray>.Publisher? dIOStateList;
        private static Dictionary<string, ObservableManager<bool>.Publisher>? dValveStateIssuers;
        private static ObservableManager<bool>.Publisher? dRecipeEndedPublisher;
        private static ObservableManager<short>.Publisher? dCurrentActiveRecipeIssue;
        private static ObservableManager<int>.Publisher? dRecipeControlPauseTimeIssuer;
        private static ObservableManager<(int, RecipeRunETMode)>.Publisher? dRecipeRunElapsedTimeIssuer;
        private static ObservableManager<BitArray>.Publisher? dDigitalOutput2;
        private static ObservableManager<BitArray>.Publisher? dDigitalOutput3;
        private static ObservableManager<BitArray>.Publisher? dOutputCmd1;
        private static ObservableManager<BitArray>.Publisher? recipeEnableSubConditionPublisher;
        private static ObservableManager<BitArray>.Publisher? reactorEnableSubConditionPublisher;
        private static ObservableManager<short>.Publisher? dThrottleValveControlMode;
        private static ObservableManager<ushort>.Publisher? dPressureControlModeIssuer;
        private static ObservableManager<short>.Publisher? dThrottleValveStatusIssuer;
        private static ObservableManager<BitArray>.Publisher? dLogicalInterlockStateIssuer;
        private static ObservableManager<PLCConnection>.Publisher? dPLCConnectionPublisher;
        private static ObservableManager<ControlMode>.Publisher? dControlModeChangingPublisher;
        private static ObservableManager<float>.Publisher? temperatureTVPublisher;
        private static ObservableManager<float>.Publisher? pressureTVPublisher; 
        private static ObservableManager<float>.Publisher? rotationTVPublisher;
        private static ObservableManager<float[]>.Publisher? dLineHeaterTemperatureIssuers;

        static Task<bool>? TryConnectAsync = null;

        public static PLCConnection Connected 
        { 
            get { return connected;  } 
            private set 
            { 
                connected = value;
                switch(connected)
                {
                    case PLCConnection.Connected:
                        OnConnected();
                        break;

                    case PLCConnection.Disconnected:
                        OnDisconnected();
                        break;
                }
                dPLCConnectionPublisher?.Publish(value);
            } 
        }
        private static PLCConnection connected = PLCConnection.Disconnected;

        //Create an instance of the TcAdsClient()
        public static AdsClient Ads { get; set; } = new AdsClient();
        private static DispatcherTimer? timer = null;
        private static DispatcherTimer? currentActiveRecipeListener = null;
        private static DispatcherTimer? connectionTryTimer = null;

        // Read from PLC State
        private static uint hOutputSolValve;
        private static uint[] hOutputSolValveElem = new uint [DeviceConfiguration.NumOutputSolValve];
        private static uint hMonitoring_PV;
        private static uint hControlModeCmd;
        private static uint hControlMode;
        private static uint hTemperaturePV;
        private static AnalogControllerHandle[] hAnalogControllers = new AnalogControllerHandle[DeviceConfiguration.NumControllers];
        private static InterelockHandle hInterlock = new InterelockHandle();
        private static RecipeHandle hRecipe = new RecipeHandle();
        private static GeneralIOHandle hGeneralIO = new GeneralIOHandle();
        private static uint[] hMaxValues = new uint[DeviceConfiguration.NumControllers];

        private static bool RecipeRunEndNotified = false;
        private static bool ShowMessageOnOnTick = true;

        private static Dictionary<string, string> LeftCoupled = new Dictionary<string, string>();
        private static Dictionary<string, string> RightCoupled = new Dictionary<string, string>();

    
        private static Dictionary<int, float> AnalogDeviceInterlockSetIndiceToCommit = new Dictionary<int, float>();
        private static (bool, float) DigitalDevicelnterlockSetToCommit = (false, 0.0f);
        private static Dictionary<int, float> InterlockSetIndiceToCommit = new Dictionary<int, float>();
        static Dictionary<DeviceConfiguration.Reactor, float> ReactorMaxValueToCommit = new Dictionary<DeviceConfiguration.Reactor, float>();

        private static List<Action> AddOnPLCStateUpdateTask = new List<Action>();
    }
}
