using OxyPlot;

namespace SapphireXR_App.Models
{
    public static class RecipeRunBottomDashBoardModel
    {
        public static float? GetFlowControllerValue(string valueName, Recipe recipe)
        {
            switch (valueName)
            {
                case "M01":
                    return recipe.M01;

                case "M02":
                    return recipe.M02;

                case "M03":
                    return recipe.M03;

                case "M04":
                    return recipe.M04;

                case "M05":
                    return recipe.M05;

                case "M06":
                    return recipe.M06;

                case "M07":
                    return recipe.M07;

                case "M08":
                    return recipe.M08;

                case "M09":
                    return recipe.M09;

                case "M10":
                    return recipe.M10;

                case "M11":
                    return recipe.M11;

                case "M12":
                    return recipe.M12;

                case "E01":
                    return recipe.E01;

                case "E02":
                    return recipe.E02;

                case "E03":
                    return recipe.E03;

                case "E04":
                    return recipe.E04;

                case "R01":
                    return recipe.STemp;

                case "R02":
                    return recipe.RPress;

                case "R03":
                    return recipe.SRotation;

                default:
                    return 0.0f;
            }
        }
    }
}
