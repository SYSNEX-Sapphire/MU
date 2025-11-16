namespace SapphireXR_App.Models
{
    internal class SettingModel
    {
        public void addDeviceAlarmWarningEnableUpdate(string deviceID, bool bitValue, Dictionary<string, int> deviceToIndex, Dictionary<int, bool> toCommitList)
        {
            if (deviceToIndex.TryGetValue(deviceID, out int bit) == true)
            {
                toCommitList[bit] = bitValue;
            }
        }

        public void addAnalogDeviceAlarmStateUpdate(string deviceID, bool bitValue)
        {
            addDeviceAlarmWarningEnableUpdate(deviceID, bitValue, DeviceConfiguration.dAnalogDeviceAlarmWarningBit, analogAlarmEnableToCommit);
        }

        public void addAnalogDeviceWarningStateUpdate(string deviceID, bool bitValue)
        {
            addDeviceAlarmWarningEnableUpdate(deviceID, bitValue, DeviceConfiguration.dAnalogDeviceAlarmWarningBit, analogWarningEnableToCommit);
        }

        public void addDigitalDeviceAlarmStateUpdate(string deviceID, bool bitValue)
        {
            addDeviceAlarmWarningEnableUpdate(deviceID, bitValue, DeviceConfiguration.dDigitalDeviceAlarmWarningBit, digitalAlarmEnableToCommit);
        }

        public void addDigitalDeviceWarningStateUpdate(string deviceID, bool bitValue)
        {
            addDeviceAlarmWarningEnableUpdate(deviceID, bitValue, DeviceConfiguration.dDigitalDeviceAlarmWarningBit, digitalWarningEnableToCommit);
        }

        private Dictionary<int, bool> analogAlarmEnableToCommit = new Dictionary<int, bool>();
        private Dictionary<int, bool> analogWarningEnableToCommit = new Dictionary<int, bool>();
        private Dictionary<int, bool> digitalAlarmEnableToCommit = new Dictionary<int, bool>();
        private Dictionary<int, bool> digitalWarningEnableToCommit = new Dictionary<int, bool>();
    }
}
