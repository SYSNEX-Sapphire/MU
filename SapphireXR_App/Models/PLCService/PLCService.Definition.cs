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
        private static BitArray? baReadValveStatePLC = null;
        private static float[] aDeviceCurrentValues = new float[DeviceConfiguration.NumControllers];
        private static float[] aDeviceControlValues = new float[DeviceConfiguration.NumControllers];
        private static float[]? aMonitoring_PVs = null;
        private static short[]? aInputState = null;
        private static BitArray? bOutputCmd1 = null;
        private static int[] InterlockEnables = Enumerable.Repeat<int>(0, (int)DeviceConfiguration.NumAlarmWarningArraySize).ToArray();
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

        private struct HAnalogController
        {
            public uint hPV;
            public uint hCV;
            public uint hCVControllerInput;
        }
        // Read from PLC State
        private static uint hReadValveStatePLC;
        private static uint hRcp;
        private static uint hRcpTotalStep;
        private static uint hCmd_RcpOperation;
        private static uint hRcpStepN;
        private static uint hMonitoring_PV;
        private static uint hInputState;
        private static uint hInputState5;
        private static uint hControlModeCmd;
        private static uint hControlMode;
        private static uint hUserState;
        private static uint hRecipeControlPauseTime;
        private static uint hDigitalOutput;
        private static uint hDigitalOutput2;
        private static uint hOutputCmd;
        private static uint hOutputCmd1;
        private static uint hOutputCmd2;
        private static uint hOutputSetType;
        private static uint hOutputMode;
        private static uint hRecipeRunET;
        private static uint hTemperaturePV;
        private static uint hUIInterlockCheckRecipeEnable;
        private static uint hUIInterlockCheckReactorEnable;
        private static uint[] hInterlockEnable = new uint[DeviceConfiguration.NumAlarmWarningArraySize];
        private static uint[] hInterlockset = new uint[DeviceConfiguration.NumInterlockSet];
        private static uint[] hInterlock = new uint[DeviceConfiguration.NumInterlock];
        private static HAnalogController[] hAnalogControllers = new HAnalogController[DeviceConfiguration.NumControllers];
        private static uint[] hReactorMaxValue = new uint[DeviceConfiguration.NumReactor];

        private static bool RecipeRunEndNotified = false;
        private static bool ShowMessageOnOnTick = true;

        private static Dictionary<string, string> LeftCoupled = new Dictionary<string, string>();
        private static Dictionary<string, string> RightCoupled = new Dictionary<string, string>();

        private static HashSet<int> InterlockEnableUpperIndiceToCommit = new HashSet<int>();
        private static HashSet<int> InterlockEnableLowerIndiceToCommit = new HashSet<int>();
        private static Dictionary<int, float> AnalogDeviceInterlockSetIndiceToCommit = new Dictionary<int, float>();
        private static (bool, float) DigitalDevicelnterlockSetToCommit = (false, 0.0f);
        private static Dictionary<int, float> InterlockSetIndiceToCommit = new Dictionary<int, float>();
        static Dictionary<DeviceConfiguration.Reactor, float> ReactorMaxValueToCommit = new Dictionary<DeviceConfiguration.Reactor, float>();

        private static List<Action> AddOnPLCStateUpdateTask = new List<Action>();
    }
}
