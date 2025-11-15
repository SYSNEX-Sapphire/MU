using CsvHelper;
using Microsoft.Win32;
using SapphireXR_App.Models;
using System.IO;
using static SapphireXR_App.Models.RecipeValidator;

namespace SapphireXR_App.Common
{
    public static class RecipeService
    {
        internal class MaxValueExceedSubscriber : IObserver<string>
        {
            void IObserver<string>.OnCompleted()
            {
                throw new NotImplementedException();
            }

            void IObserver<string>.OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            void IObserver<string>.OnNext(string value)
            {
                fcMaxValueExceeded.Add(value);
            }

            public HashSet<string> fcMaxValueExceeded { get; private set; } = new HashSet<string>();
        }

        public static (bool, string?, List<Recipe>?, HashSet<string>?) OpenRecipe(CsvHelper.Configuration.CsvConfiguration config, string? initialDirectory)
        {
            OpenFileDialog openFile = new();
            openFile.Multiselect = false;
            openFile.Filter = "csv 파일(*.csv)|*.csv";

            if (Path.Exists(initialDirectory) == false)
            {
                initialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Recipe";
                if (Path.Exists(initialDirectory) == false)
                {
                    Directory.CreateDirectory(initialDirectory);
                }
            }
            openFile.InitialDirectory = initialDirectory;

            if (openFile.ShowDialog() != true) return (false, null, null, null);
            string recipeFilePath = openFile.FileName;

            using (StreamReader streamReader = new(recipeFilePath))
            {
                MaxValueExceedSubscriber maxValueExceedSubscriber;
                using (var csvReader = new CsvReader(streamReader, config))
                using (var unsubscriber = ObservableManager<string>.Subscribe("Recipe.MaxValueExceed", maxValueExceedSubscriber = new MaxValueExceedSubscriber()))
                {
                    return (true, recipeFilePath, csvReader.GetRecords<Recipe>().ToList(), maxValueExceedSubscriber.fcMaxValueExceeded);
                }
            }
        }

        public static PlcRecipe[] ToPLCRecipe(IList<Recipe> recipes)
        {
            (bool success, string message) = RecipeValidator.ValidOnLoadedFromDisk(recipes);
            if (success == false)
            {
                throw new Exception(message);
            }

            Recipe first = recipes.First();
            AnalogRecipe analogRecipe = new(first);

            PlcRecipe[] aRecipePLC = new PlcRecipe[recipes.Count];
            int i = 0;
            foreach (Recipe iRecipeRow in recipes)
            {
                aRecipePLC[i] = new PlcRecipe(iRecipeRow, analogRecipe);
                analogRecipe.update(aRecipePLC[i]);
                i += 1;
            }

            return aRecipePLC;
        }

        public static void PLCLoad(IList<Recipe> recipes)
        {
            if (recipes.Count == 0)
            {
                throw new InvalidOperationException("빈 Recipe 입니다.");
            }

            PlcRecipe[] aRecipePLC = ToPLCRecipe(recipes);
            PLCService.WriteRecipe(aRecipePLC);
            PLCService.WriteTotalStep((short)aRecipePLC.Length);
        }

        public static void SetRecipeStepValidator(IList<Recipe> recipes, Action onValidChanged)
        {
            if (0 < recipes.Count)
            {
                recipes[0].stepValidator = new FirstRecipeStepValidator(false);
                recipes[0].stepValidator!.PropertyChanged += (sender, args) =>
                {
                    switch (args.PropertyName)
                    {
                        case nameof(RecipeStepValidator.Valid):
                            onValidChanged();
                            break;
                    }
                };
                for (int recipe = 1; recipe < recipes.Count; ++recipe)
                {
                    recipes[recipe].stepValidator = DefaultRecipeStepValidator;
                }
            }
        }

        private static readonly NormalRecipeStepValidator DefaultRecipeStepValidator = new NormalRecipeStepValidator();
    }
}
