using SapphireXR_App.Models;

namespace SapphireXR_App.DeviceDependency.Model
{
    public class RecipeLog
    {
#pragma warning disable CS8618 // null을 허용하지 않는 필드는 생성자를 종료할 때 null이 아닌 값을 포함해야 합니다. 'required' 한정자를 추가하거나 nullable로 선언하는 것이 좋습니다.
        public RecipeLog() { }
#pragma warning restore CS8618 // null을 허용하지 않는 필드는 생성자를 종료할 때 null이 아닌 값을 포함해야 합니다. 'required' 한정자를 추가하거나 nullable로 선언하는 것이 좋습니다.
        public RecipeLog(IList<Recipe> recipes)
        {
            if (0 < recipes.Count)
            {
                SV_M01 = PLCService.ReadFlowControllerTargetValue("M01");
                SV_M02 = PLCService.ReadFlowControllerTargetValue("M02");
                SV_M03 = PLCService.ReadFlowControllerTargetValue("M03");
                SV_M04 = PLCService.ReadFlowControllerTargetValue("M04");
                SV_M05 = PLCService.ReadFlowControllerTargetValue("M05");
                SV_M06 = PLCService.ReadFlowControllerTargetValue("M06");
                SV_M07 = PLCService.ReadFlowControllerTargetValue("M07");
                SV_M08 = PLCService.ReadFlowControllerTargetValue("M08");
                SV_M09 = PLCService.ReadFlowControllerTargetValue("M09");
                SV_M10 = PLCService.ReadFlowControllerTargetValue("M10");
                SV_M11 = PLCService.ReadFlowControllerTargetValue("M11");
                SV_M12 = PLCService.ReadFlowControllerTargetValue("M12");

                SV_E01 = PLCService.ReadFlowControllerTargetValue("E01");
                SV_E02 = PLCService.ReadFlowControllerTargetValue("E02");
                SV_E03 = PLCService.ReadFlowControllerTargetValue("E03");
                SV_E04 = PLCService.ReadFlowControllerTargetValue("E04");
                SV_TEMP = PLCService.ReadFlowControllerTargetValue("R01");
                SV_PRES = PLCService.ReadFlowControllerTargetValue("R02");
                SV_ROT = PLCService.ReadFlowControllerTargetValue("R03");

                PV_M01 = PLCService.ReadCurrentValue("M01");
                PV_M02 = PLCService.ReadCurrentValue("M02");
                PV_M03 = PLCService.ReadCurrentValue("M03");
                PV_M04 = PLCService.ReadCurrentValue("M04");
                PV_M05 = PLCService.ReadCurrentValue("M05");
                PV_M06 = PLCService.ReadCurrentValue("M06");
                PV_M07 = PLCService.ReadCurrentValue("M07");
                PV_M08 = PLCService.ReadCurrentValue("M08");
                PV_M09 = PLCService.ReadCurrentValue("M09");
                PV_M10 = PLCService.ReadCurrentValue("M10");
                PV_M11 = PLCService.ReadCurrentValue("M11");
                PV_M12 = PLCService.ReadCurrentValue("M12");
                PV_E01 = PLCService.ReadCurrentValue("E01");
                PV_E02 = PLCService.ReadCurrentValue("E02");
                PV_E03 = PLCService.ReadCurrentValue("E03");
                PV_E04 = PLCService.ReadCurrentValue("E04");
                PV_TEMP = PLCService.ReadCurrentValue("R01");
                PV_PRES = PLCService.ReadCurrentValue("R02");
                PV_ROT = PLCService.ReadCurrentValue("R03");

                Step = recipes[Math.Min(PLCService.ReadCurrentStep() - 1, recipes.Count - 1)].Name;

                LogTime = DateTime.Now;
            }
            else
            {
                throw new ArgumentException("recipes must contain at least one element");
            }
        }

        public string Step { get; set; }
        public float PV_M01 { get; set; }
        public float PV_M02 { get; set; }
        public float PV_M03 { get; set; }
        public float PV_M04 { get; set; }
        public float PV_M05 { get; set; }
        public float PV_M06 { get; set; }
        public float PV_M07 { get; set; }
        public float PV_M08 { get; set; }
        public float PV_M09 { get; set; }
        public float PV_M10 { get; set; }
        public float PV_M11 { get; set; }
        public float PV_M12 { get; set; }
        public float PV_E01 { get; set; }
        public float PV_E02 { get; set; }
        public float PV_E03 { get; set; }
        public float PV_E04 { get; set; }
        public float PV_TEMP { get; set; }
        public float PV_PRES { get; set; }
        public float PV_ROT { get; set; }
        public float PV_IHT_KW { get; set; }
        public float PV_SH_CW { get; set; }
        public float PV_IHT_CW { get; set; }
        public float SV_M01 { get; set; }
        public float SV_M02 { get; set; }
        public float SV_M03 { get; set; }
        public float SV_M04 { get; set; }
        public float SV_M05 { get; set; }
        public float SV_M06 { get; set; }
        public float SV_M07 { get; set; }
        public float SV_M08 { get; set; }
        public float SV_M09 { get; set; }
        public float SV_M10 { get; set; }
        public float SV_M11 { get; set; }
        public float SV_M12 { get; set; }
        public float SV_E01 { get; set; }
        public float SV_E02 { get; set; }
        public float SV_E03 { get; set; }
        public float SV_E04 { get; set; }
        public float SV_TEMP { get; set; }
        public float SV_PRES { get; set; }
        public float SV_ROT { get; set; }
                          
        public DateTime LogTime { get; set; }
    }
}
