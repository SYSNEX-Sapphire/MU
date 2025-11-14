namespace SapphireXR_App.DeviceDependency
{
    public static class DependentConfiguration
    {
        internal enum HardWiringInterlockStateIndex
        {
            MaintenanceKey = 0, CleanDryAir = 3, CoolingWater = 4, SusceptorMotorRun = 5, SusceptorMotorStop = 6, SusceptorMotorFault = 7, VacuumPumpFault = 8,
            VacuumPumpWarning = 9, VacuumPumpAlarm = 10, VacuumPumpRunning = 11, DoorSensorReactor = 13, DoorSensorGasDelivery = 14, DoorSensorElectricControl = 15
        };

        const int NumShortBits = sizeof(short) * 8;

        internal enum IOListIndex
        {
            PowerResetSwitch = 0, Cover_UpperLimit = 1, Cover_LowerLimit = 2, CylinderAutoSwicthClosed = 3, CylinderAutoSwicthOpen = 4, SMPS_24V480 = 5, SMPS_24V72 = 6,
            SMPS_15VPlus = 7, SMPS_15VMinus = 8, CB_GraphiteHeater = 9, CB_ThermalBath = 10, CB_VaccumPump = 11, CB_LineHeater = 12, CB_RotationMotor = 13, CB_CoverLiftMotor = 14,
            CB_ThrottleValve = 15, CB_Lamp = NumShortBits * 1 + 1, CB_GasDetector = NumShortBits * 1, CB_CabitnetLamp = NumShortBits * 1 + 1, CB_MFCPower = NumShortBits * 1 + 2,
            LineHeader1 = NumShortBits * 1 + 3, LineHeader2 = NumShortBits * 1 + 4, LineHeader3 = NumShortBits * 1 + 5, LineHeader4 = NumShortBits * 1 + 6,
            LineHeader5 = NumShortBits * 1 + 7, LineHeader6 = NumShortBits * 1 + 8, LineHeader7 = NumShortBits * 1 + 9, LineHeader8 = NumShortBits * 1 + 10,
            GasDetectorH2 = NumShortBits * 1 + 11, GasDetectorH2S = NumShortBits * 1 + 12, GasDetectorH2Se = NumShortBits * 1 + 13, FireSensor = NumShortBits * 1 + 14,
            ExternalScrubberFault = NumShortBits * 1 + 15, ExternalH2GasCabinetFault = NumShortBits * 2, ExternalH2SGasCabinetFault = NumShortBits * 2 + 1,
            ExternalH2SeGasCabinetFault = NumShortBits * 2 + 2, ExternalUserInputAlarm = NumShortBits * 2 + 3, ThermalBath1_Deviaiton = NumShortBits * 2 + 4,
            ThermalBath1_CutOff = NumShortBits * 2 + 5, ThermalBath2_Deviaiton = NumShortBits * 2 + 6, ThermalBath2_CutOff = NumShortBits * 2 + 7, ThermalBath3_Deviaiton = NumShortBits * 2 + 8,
            ThermalBath3_CutOff = NumShortBits * 2 + 9, ThermalBath4_Deviaiton = NumShortBits * 2 + 10, ThermalBath4_CutOff = NumShortBits * 2 + 11,
            SingalTower_RED = NumShortBits * 3, SingalTower_YELLOW = NumShortBits * 3 + 1, SingalTower_GREEN = NumShortBits * 3 + 2, SingalTower_BLUE = NumShortBits * 3 + 3,
            SingalTower_WHITE = NumShortBits * 3 + 4, SingalTower_BUZZER = NumShortBits * 3 + 5, ClampLock = NumShortBits * 3 + 6, ClampRelease = NumShortBits * 3 + 7,
            DOR_Vaccum_State = NumShortBits * 3 + 8, Temp_Controller_Alarm = NumShortBits * 3 + 9
        };


        internal enum DigitalOutput2Index
        {
            InductionHeaterOn = 0, InductionHeaterReset, VaccumPumpOn, VaccumPumpReset,
        }

        public enum DigitalOutput3Index
        {
            InductionHeaterMC = 0, ThermalBathMC, VaccumPumpMC, LineHeaterMC, RotationAlaramReset = 6
        }

        public enum OutputCmd1Index
        {
            GraphiteHeaterPower = 0, ThermalBathPower, VaccumPumpPower, LineHeaterPower, VaccumPumpControl = 6, VaccumPumpReset, RotationControl = 8, RotationReset = 10,
            TempControllerManAuto = 12, PressureControlMode = 13
        }
        public enum InterlockEnableSetting
        {
            Buzzer = 2, CanOpenSusceptorTemperature, CanOpenReactorPressure, PressureLimit, RetryCount, SusceptorRotationMotor
        };

        public enum InterlockValueSetting
        {
            ProcessGasPressureAlarm = 5, ProcessGasPressureWarning, SusceptorOverTemperature, ReactorOverPressure, CanOpenSusceptorTemperature, CanOpenReactorPressure,
            PressureLimit, RetryCount, DORFault
        };

        public enum Reactor { SusceptorTemperature = 0, ReactorPressure, SusceptorRotation };

        public const uint LineHeaterTemperature = 8;
        public const uint NumAlarmWarningArraySize = 6;
        public const uint NumInterlockSet = 20;
        public const uint NumInterlock = 5;
        public const uint NumDigitalDevice = 17;
        public const uint NumAnalogDevice = 19;
        public const uint NumReactor = 3;
        public const int NumRecipeEnableSubConditions = 12;
        public const int NumReactorEnableSubConditions = 10;
        public const float AnalogControllerOutputVoltage = 5.0f;

        public static readonly Dictionary<string, int> ValveIDtoOutputSolValveIdx = new Dictionary<string, int>
        {
            { "V01", 0 }, { "V02", 1 }, { "V03", 2 }, { "V04", 3 }, { "V05", 4 }, { "V06", 5 }, { "V07", 6 }, { "V08", 7 },
            { "V09", 8 }, { "V10", 9 },  { "V11", 10 }, { "V12", 11 }, { "V13", 12 }, { "V14", 13 }, { "V15", 14 }, { "V16", 15 },
            { "V17", 16 }, { "V18", 17 },  { "V19", 18 }, { "V20", 19 }, { "V21", 20 }, { "V22", 21 }, { "V23", 22 }, { "V24", 23 },
            { "V25", 24 }, { "V26", 25 },  { "V27", 26 }, { "V28", 27 }, { "V29", 28 }, { "V30", 29 }, { "V31", 30 }, { "V32", 31 }
        };

        public static readonly List<string> RecipeFlowControllers = ["M01", "M02", "M03", "M04", "M05", "M06", "M07", "M08", "M09", "M10", "M11", "M12", "E01", "E02", "E03", "E04",
            "STemp", "RPress", "SRotation"];
        public static readonly List<string> RecipeValves = ["V01", "V02", "V03", "V04", "V05", "V06", "V07", "V08", "V09", "V10", "V11", "V12", "V13", "V14", "V15", "V16", "V17", "V18", "V19", "V20"];

        public static readonly Dictionary<string, int> dIndexController = new Dictionary<string, int>
        {
            { "M01", 0 }, { "M02", 1 }, { "M03", 2 }, { "M04", 3 }, { "M05", 4 }, { "M06", 5 }, { "M07", 6 }, { "M08", 7 }, { "M09", 8 }, { "M10", 9 },
            { "M11", 10 }, { "M12", 11 }, { "E01", 12 },  { "E02", 13 }, { "E03", 14 }, { "E04", 15 }, {"R01", 16}, {"R02", 17}, {"R03", 18}
        };
        public static readonly int NumControllers = dIndexController.Count;

        public static readonly Dictionary<string, int> dMonitoringMeterIndex = new Dictionary<string, int>
        {
            { "UltimatePressure", 0 },  { "ExtPressure", 1},  { "DorPressure", 2}, { "Gas1", 3}, { "Gas2", 4}, { "Gas3", 5}, { "Gas4", 6}, { "ShowerHeadTemp", 7}, { "DP1_Chalcogen", 8 },
            { "DP2_MODP", 9 }, { "HeaterPowerRate", 10 }, { "ValvePosition", 11 }, { "Source1", 12}, { "Source2", 13},  { "Source3", 14},  { "Source4", 15}, { "TotalFlow_CAL", 16 },
            { "TotalFlow_MO", 17 }
        };

        public static readonly Dictionary<string, int> dAnalogDeviceAlarmWarningBit = new Dictionary<string, int>
        {
            { "R01", 0 }, { "R02", 1 }, { "R03", 2 },  { "M01", 3 }, { "M02", 4 }, { "M03", 5 },  { "M04", 6 }, { "M05", 7 }, { "M06", 8 }, { "M07", 9 }, { "M08", 10 }, { "M09", 11 },
            { "M10", 12 }, { "M11", 13 }, { "M12", 14 },  { "E01", 15 }, { "E02", 16 }, { "E03", 17 }, { "E04", 18 }
        };

        public static readonly Dictionary<string, int> dDigitalDeviceAlarmWarningBit = new Dictionary<string, int>
        {
            { "A01", 0 }, { "A02", 1 }, { "A03", 2 },  { "A04", 3 }, { "A05", 4 }, { "A06", 5 },  { "A07", 6 }, { "A08", 7 }, { "A09", 8 }, { "A10", 9 }, { "A11", 10 }, { "A12", 11 },
            { "A13", 12 },  { "A14", 13 },  { "A15", 14 },  { "A16", 15 },  { "A17", 16 }
        };

        public static readonly Dictionary<string, string> RecipeReactorColumnHeaderToControllerID = new Dictionary<string, string>
        {
            { "Susceptor Temp.", "R01" }, { "Reactor Press.", "R02" }, { "Sus. Rotation", "R03" },
        };
    }
}
