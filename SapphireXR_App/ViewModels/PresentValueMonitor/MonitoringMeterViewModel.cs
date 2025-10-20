using SapphireXR_App.Common;

namespace SapphireXR_App.ViewModels
{
    public class MonitoringMeterViewModel : PresentValueMonitorViewModel
    {
        protected override void updatePresentValue(float value)
        {
            PresentValue = Util.FloatingPointStrWithMaxDigit(value, AppSetting.FloatingPointMaxNumberDigit);
        }
    }
}
