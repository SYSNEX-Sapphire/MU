using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using SapphireXR_App.Common;
using SapphireXR_App.Models;

namespace SapphireXR_App.ViewModels.BottomDashBoard
{
    public class RecipeRunBottomDashBoardViewModel: BottomDashBoardViewModel
    {
        class ControlTargetValueSeriesUpdaterForRecipeRun : ControlTargetValueSeriesUpdater, IObserver<RecipeRunViewModel.RecipeUserState>, IObserver<int>
        {
            internal ControlTargetValueSeriesUpdaterForRecipeRun(string title) : base(title) 
            {
                ObservableManager<RecipeRunViewModel.RecipeUserState>.Subscribe("RecipeRun.State", this);
            }

            protected override Axis initializeXAxis()
            {
                return new TimeSpanAxis
                {
                    Title = "Time Span (Second)",
                    Position = AxisPosition.Bottom,
                    AxislineColor = OxyColors.White,
                    MajorGridlineColor = OxyColors.White,
                    MinorGridlineColor = OxyColors.White,
                    TicklineColor = OxyColors.White,
                    ExtraGridlineColor = OxyColors.White,
                    MinorTicklineColor = OxyColors.White,
                    IntervalLength = 60
                };
            }

            public void initChart(IList<Recipe> recipes)
            {
                cleanChart();
                if (0 < recipes.Count)
                {
                    LegendUpdate = true;
                    uint accumTime = 0;
                    var series1 = plotModel.Series.OfType<LineSeries>().ElementAt(0);
                    series1.Points.Add(new DataPoint(accumTime, 0));
                    foreach (Recipe recipe in recipes)
                    {
                        try
                        {
                            float flowControllerValue = RecipeRunBottomDashBoardModel.GetFlowControllerValue(plotModel.Title, recipe) ?? (float)series1.Points.Last().Y;
                            accumTime += (uint)recipe.RTime;
                            series1.Points.Add(new DataPoint(TimeSpanAxis.ToDouble(TimeSpan.FromSeconds(accumTime)), flowControllerValue));
                            accumTime += (uint)recipe.HTime;
                            series1.Points.Add(new DataPoint(TimeSpanAxis.ToDouble(TimeSpan.FromSeconds(accumTime)), flowControllerValue));
                        }
                        catch(Exception)
                        {
                            //System.Diagnostics.Debug.WriteLine(excepetion.Message);
                            continue;
                        }
                    }
                    plotModel.Axes[0].Maximum = TimeSpanAxis.ToDouble(TimeSpan.FromSeconds(accumTime));
                }
                plotModel.InvalidatePlot(true);
            }

            protected void update(int value)
            {
                if(LegendUpdate == false)
                {
                    return;
                }
                var series2 = plotModel.Series.OfType<LineSeries>().ElementAt(1);

                DateTime now = DateTime.Now;
                TimeSpan timeSpan = (now - (firstTime ??= now));
                double x = TimeSpanAxis.ToDouble(timeSpan);
                series2.Points.Add(new DataPoint(x, (double)value));

                plotModel.InvalidatePlot(true);
            }

            public void cleanChart()
            {
                plotModel.Series.OfType<LineSeries>().ElementAt(0).Points.Clear();
                plotModel.Series.OfType<LineSeries>().ElementAt(1).Points.Clear();
                firstTime = null;
                LegendUpdate = false;
            }

            void IObserver<RecipeRunViewModel.RecipeUserState>.OnCompleted()
            {
                throw new NotImplementedException();
            }

            void IObserver<RecipeRunViewModel.RecipeUserState>.OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            void IObserver<RecipeRunViewModel.RecipeUserState>.OnNext(RecipeRunViewModel.RecipeUserState recipeRunState)
            {
                LegendUpdate = recipeRunState == RecipeRunViewModel.RecipeUserState.Run;
            }

            void IObserver<int>.OnCompleted()
            {
                throw new NotImplementedException();
            }

            void IObserver<int>.OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            void IObserver<int>.OnNext(int value)
            {
                update(value);
            }

            private DateTime? firstTime = null;
            public bool LegendUpdate { get; set; } = false;
        }

        public RecipeRunBottomDashBoardViewModel(): base("CurrentPLCState.RecipeRun")
        {
            initSeriesUpdater();
        }

        private void initSeriesUpdater()
        {
            foreach (var (id, index) in DeviceConfiguration.dIndexController)
            {
                ControlTargetValueSeriesUpdaterForRecipeRun controlCurrentValueSeriesUpdater = new ControlTargetValueSeriesUpdaterForRecipeRun(id);
                ObservableManager<int>.Subscribe("FlowControl." + id + ".ControlValue", controlCurrentValueSeriesUpdater);
                plotModels[index] = controlCurrentValueSeriesUpdater;
            }
        }

        public void resetFlowChart(IList<Recipe> recipes)
        {
            init();
            foreach (ControlTargetValueSeriesUpdaterForRecipeRun controlTargetValueSeriesUpdater in plotModels)
            {
                controlTargetValueSeriesUpdater.initChart(recipes);
            }
        }
    }
}
