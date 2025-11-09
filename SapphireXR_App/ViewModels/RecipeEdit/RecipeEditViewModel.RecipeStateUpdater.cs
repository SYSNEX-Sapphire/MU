using SapphireXR_App.Bases;
using SapphireXR_App.Common;
using SapphireXR_App.Models;
using System.ComponentModel;

namespace SapphireXR_App.ViewModels
{
    public partial class RecipeEditViewModel : ViewModelBase
    {
        internal class RecipeStateUpader: IDisposable
        {
            internal class ValveStateSubscriber : IObserver<bool>
            {
                internal ValveStateSubscriber(RecipeStateUpader recipeStateUpdater, string valveIDAssociated)
                {
                    stateUpdater = recipeStateUpdater;
                    valveID = valveIDAssociated;
                }

                void IObserver<bool>.OnCompleted()
                {
                    throw new NotImplementedException();
                }

                void IObserver<bool>.OnError(Exception error)
                {
                    throw new NotImplementedException();
                }

                void IObserver<bool>.OnNext(bool value)
                {
                    if (stateUpdater.getValveState(valveID) != value)
                    {
                        stateUpdater.updateValve(valveID, value);
                    }
                }

                private string valveID;
                private RecipeStateUpader stateUpdater;
            }

            internal class ControlValueSubscriber : IObserver<float?>
            {
                internal ControlValueSubscriber(RecipeStateUpader recipeStateUpdater, string flowControllerIDAssociated)
                {
                    stateUpdater = recipeStateUpdater;
                    flowControllerID = flowControllerIDAssociated;
                }

                void IObserver<float?>.OnCompleted()
                {
                    throw new NotImplementedException();
                }

                void IObserver<float?>.OnError(Exception error)
                {
                    throw new NotImplementedException();
                }

                void IObserver<float?>.OnNext(float? value)
                {
                    if (stateUpdater.getControlValue(flowControllerID) != value)
                    {
                        stateUpdater.updateControlValue(flowControllerID, value);
                    }
                }

                private string flowControllerID;
                private RecipeStateUpader stateUpdater;
            }

            private void initializePublishSubscribe()
            {
                foreach ((string valveID, int index) in PLCService.ValveIDtoOutputSolValveIdx)
                {
                    string topicName = "Valve.OnOff." + valveID + ".CurrentRecipeStep";

                    valveStatePublishers[valveID] = ObservableManager<bool>.Get(topicName);

                    ValveStateSubscriber valveStateSubscriber = new ValveStateSubscriber(this, valveID);
                    unsubscribers.Add(ObservableManager<bool>.Subscribe(topicName, valveStateSubscriber));
                    valveStateSubscribers.Add(valveStateSubscriber);
                }
               
                foreach ((string flowControllerID, int index) in PLCService.dIndexController)
                {
                    string topicName = "FlowControl." + flowControllerID + ".CurrentValue.CurrentRecipeStep";
                    flowValuePublishers[flowControllerID] = ObservableManager<float?>.Get(topicName);

                    ControlValueSubscriber controlValueSubscriber = new ControlValueSubscriber(this, flowControllerID);
                    unsubscribers.Add(ObservableManager<float?>.Subscribe(topicName, controlValueSubscriber));
                    controlStateSubscribers.Add(controlValueSubscriber);
                }
            }

            private static string GetFlowControllerID(string flowControlState)
            {
                switch (flowControlState)
                {
                    case "STemp":
                        return "R01";

                    case "RPress":
                        return "R02";

                    case "SRotation":
                        return "R03";
                }

                throw new ArgumentException($"{flowControlState} is not valid property in recipe");
            }

            private void onRecipePropertyChanged(object? sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName != null)
                {
                    switch (args.PropertyName)
                    {
                        case "STemp":
                        case "RPress":
                        case "SRotation":
                            {
                                string flowControllerID = GetFlowControllerID(args.PropertyName);
                                propagateControlValue(flowControllerID, getControlValue(flowControllerID));
                            }
                            break;

                        default:
                            if (2 <= args.PropertyName.Length)
                            {
                                switch (args.PropertyName[0])
                                {
                                    case 'V':
                                        if (CheckDigit(args.PropertyName, 1) == true)
                                        {
                                            propagateValveState(args.PropertyName, getValveState(args.PropertyName));
                                        }
                                        break;

                                    case 'E':
                                    case 'M':
                                        {
                                            if (CheckDigit(args.PropertyName, 1) == true)
                                            {
                                                propagateControlValue(args.PropertyName, getControlValue(args.PropertyName));
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                    }

                }
            }

            public void setSelectedRecipeStep(Recipe recipe)
            {
                if(currentSelected != null)
                {
                    currentSelected.PropertyChanged -= onRecipePropertyChanged;
                }
                currentSelected = recipe;
                currentSelected.PropertyChanged += onRecipePropertyChanged;
                if(publishSubscribeInitialized == false)
                {
                    initializePublishSubscribe();
                    publishSubscribeInitialized = true;
                }
                propageSelectedRecipeStep(recipe);
            }

            private void propagateValveState(string valveID, bool isOpen)
            {
                valveStatePublishers[valveID].Publish(isOpen);
            }

            private void propagateControlValue(string flowControllerID, float? value)
            {
                if (flowControllerID != string.Empty)
                {
                    flowValuePublishers[flowControllerID].Publish(value);
                }
            }

            static private bool CheckDigit(string str, int startIndex)
            {
                for (int letter = startIndex; letter < str.Length; ++letter)
                {
                    if (char.IsNumber(str[letter]) == false)
                    {
                        return false;
                    }
                }
                return true;
            }

            private void propageSelectedRecipeStep(Recipe value)
            {
                valveStatePublishers["V01"].Publish(value.V01);
                valveStatePublishers["V02"].Publish(value.V02);
                valveStatePublishers["V03"].Publish(value.V03);
                valveStatePublishers["V04"].Publish(value.V04);
                valveStatePublishers["V05"].Publish(value.V05);
                valveStatePublishers["V06"].Publish(value.V06);
                valveStatePublishers["V07"].Publish(value.V07);
                valveStatePublishers["V08"].Publish(value.V08);
                valveStatePublishers["V09"].Publish(value.V09);
                valveStatePublishers["V10"].Publish(value.V10);
                valveStatePublishers["V11"].Publish(value.V11);
                valveStatePublishers["V12"].Publish(value.V12);
                valveStatePublishers["V13"].Publish(value.V13);
                valveStatePublishers["V14"].Publish(value.V14);
                valveStatePublishers["V15"].Publish(value.V15);
                valveStatePublishers["V16"].Publish(value.V16);
                valveStatePublishers["V17"].Publish(value.V17);
                valveStatePublishers["V18"].Publish(value.V18);
                valveStatePublishers["V19"].Publish(value.V19);
                valveStatePublishers["V20"].Publish(value.V20);

                flowValuePublishers["M01"].Publish(value.M01);
                flowValuePublishers["M02"].Publish(value.M02);
                flowValuePublishers["M03"].Publish(value.M03);
                flowValuePublishers["M04"].Publish(value.M04);
                flowValuePublishers["M05"].Publish(value.M05);
                flowValuePublishers["M06"].Publish(value.M06);
                flowValuePublishers["M07"].Publish(value.M07);
                flowValuePublishers["M08"].Publish(value.M08);
                flowValuePublishers["M09"].Publish(value.M09);
                flowValuePublishers["M10"].Publish(value.M10);
                flowValuePublishers["M11"].Publish(value.M11);
                flowValuePublishers["M12"].Publish(value.M12);
                flowValuePublishers["E01"].Publish(value.E01);
                flowValuePublishers["E02"].Publish(value.E02);
                flowValuePublishers["E03"].Publish(value.E03);
                flowValuePublishers["E04"].Publish(value.E04);
                flowValuePublishers["R01"].Publish(value.STemp);
                flowValuePublishers["R02"].Publish(value.RPress);
                flowValuePublishers["R03"].Publish(value.SRotation);
            }

            public void clean()
            {
                ObservableManager<bool>.Get("Reset.CurrentRecipeStep").Publish(true);
                dispose();
            }

            private void dispose()
            {
                foreach(var unsubscriber in unsubscribers)
                {
                    unsubscriber.Dispose();
                }
                unsubscribers.Clear();
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        dispose();
                    }

                    disposedValue = true;
                }
            }

            void IDisposable.Dispose()
            {
                // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            private void updateValve(string valveID, bool isOpen)
            {
                if(currentSelected == null)
                {
                    return;
                }
                switch(valveID)
                {
                    case "V01":
                        currentSelected!.V01 = isOpen;
                        break;

                    case "V02":
                        currentSelected!.V02 = isOpen;
                        break;

                    case "V03":
                        currentSelected!.V03 = isOpen;
                        break;

                    case "V04":
                        currentSelected!.V04 = isOpen;
                        break;

                    case "V05":
                        currentSelected!.V05 = isOpen;
                        break;

                    case "V06":
                        currentSelected!.V06 = isOpen;
                        break;

                    case "V07":
                        currentSelected!.V07 = isOpen;
                        break;

                    case "V08":
                        currentSelected!.V08 = isOpen;
                        break;

                    case "V09":
                        currentSelected!.V09 = isOpen;
                        break;

                    case "V10":
                        currentSelected!.V10 = isOpen;
                        break;

                    case "V11":
                        currentSelected!.V11 = isOpen;
                        break;

                    case "V12":
                        currentSelected!.V12 = isOpen;
                        break;

                    case "V13":
                        currentSelected!.V13 = isOpen;
                        break;

                    case "V14":
                        currentSelected!.V14 = isOpen;
                        break;

                    case "V15":
                        currentSelected!.V15 = isOpen;
                        break;

                    case "V16":
                        currentSelected!.V16 = isOpen;
                        break;

                    case "V17":
                        currentSelected!.V17 = isOpen;
                        break;

                    case "V18":
                        currentSelected!.V18 = isOpen;
                        break;

                    case "V19":
                        currentSelected!.V19 = isOpen;
                        break;

                    case "V20":
                        currentSelected!.V20 = isOpen;
                        break;
                }
            }

            private bool getValveState(string valveID)
            {
                if (currentSelected == null)
                {
                    throw new Exception("RecipeStateUpdater: currentSelected is null in getValveState()");
                }
                switch (valveID)
                {
                    case "V01":
                        return currentSelected!.V01;

                    case "V02":
                        return currentSelected!.V02;

                    case "V03":
                        return currentSelected!.V03;

                    case "V04":
                        return currentSelected!.V04;

                    case "V05":
                        return currentSelected!.V05;

                    case "V06":
                        return currentSelected!.V06;

                    case "V07":
                        return currentSelected!.V07;

                    case "V08":
                        return currentSelected!.V08;

                    case "V09":
                        return currentSelected!.V09;

                    case "V10":
                        return currentSelected!.V10;

                    case "V11":
                        return currentSelected!.V11;

                    case "V12":
                        return currentSelected!.V12;

                    case "V13":
                        return currentSelected!.V13;

                    case "V14":
                        return currentSelected!.V14;

                    case "V15":
                        return currentSelected!.V15;

                    case "V16":
                        return currentSelected!.V16;

                    case "V17":
                        return currentSelected!.V17;

                    case "V18": 
                        return currentSelected!.V18;

                    case "V19":
                        return currentSelected!.V19;

                    case "V20":
                        return currentSelected!.V20;

                    default:
                        throw new Exception("RecipeStateUpdater: " + valveID + " is invalid valve name in getValveState()");
                }
            }

            private void updateControlValue(string flowControllerID, float? value)
            {
                if (currentSelected == null)
                {
                    return;
                }

                switch (flowControllerID)
                {
                    case "M01":
                        currentSelected.M01 = value;
                        break;

                    case "M02":
                        currentSelected.M02 = value;
                        break;

                    case "M03":
                        currentSelected.M03 = value;
                        break;

                    case "M04":
                        currentSelected.M04 = value;
                        break;

                    case "M05":
                        currentSelected.M05 = value;
                        break;

                    case "M06":
                        currentSelected.M06 = value;
                        break;

                    case "M07":
                        currentSelected.M07 = value;
                        break;

                    case "M08":
                        currentSelected.M08 = value;
                        break;

                    case "M09":
                        currentSelected.M09 = value;
                        break;

                    case "M10":
                        currentSelected.M10 = value;
                        break;

                    case "M11":
                        currentSelected.M11 = value;
                        break;

                    case "M12":
                        currentSelected.M12 = value;
                        break;

                    case "E01":
                        currentSelected.E01 = value;
                        break;

                    case "E02":
                        currentSelected.E02 = value;
                        break;

                    case "E03":
                        currentSelected.E03 = value;
                        break;

                    case "E04":
                        currentSelected.E04 = value;
                        break;

                    case "R01":
                        currentSelected.STemp = value;
                        break;

                    case "R02":
                        currentSelected.RPress = value;
                        break;

                    case "R03":
                        currentSelected.SRotation = value;
                        break;

                    default:
                        throw new Exception("RecipeStateUpdater: currentSelected is null in updateControlValue()");
                }
            }

            private float? getControlValue(string flowControllerID)
            {
                if (currentSelected == null)
                {
                    throw new Exception("RecipeStateUpdater: currentSelected is null in getControlValue()");
                }

                switch (flowControllerID)
                {
                    case "M01":
                        return currentSelected.M01;

                    case "M02":
                        return currentSelected.M02;

                    case "M03":
                        return currentSelected.M03;

                    case "M04":
                        return currentSelected.M04;

                    case "M05":
                        return currentSelected.M05;

                    case "M06":
                        return currentSelected.M06;

                    case "M07":
                        return currentSelected.M07;

                    case "M08":
                        return currentSelected.M08;

                    case "M09":
                        return currentSelected.M09;

                    case "M10":
                        return currentSelected.M10;

                    case "M11":
                        return currentSelected.M11;

                    case "M12":
                        return currentSelected.M12;

                    case "E01":
                        return currentSelected.E01;

                    case "E02":
                        return currentSelected.E02;

                    case "E03":
                        return currentSelected.E03;

                    case "E04":
                        return currentSelected.E04;

                    case "R01":
                        return currentSelected.STemp;
                        

                    case "R02":
                        return currentSelected.RPress;
                        

                    case "R03":
                        return currentSelected.SRotation;
                        

                    default:
                        throw new Exception("RecipeStateUpdater: return currentSelected is null in getControlValue()");
                }
            }

            private Dictionary<string, ObservableManager<bool>.Publisher> valveStatePublishers = new Dictionary<string, ObservableManager<bool>.Publisher>();
            private Dictionary<string, ObservableManager<float?>.Publisher> flowValuePublishers = new Dictionary<string, ObservableManager<float?>.Publisher>();
            private IList<IObserver<bool>> valveStateSubscribers = new List<IObserver<bool>>();
            private IList<IObserver<float?>> controlStateSubscribers = new List<IObserver<float?>>();
            private IList<IDisposable> unsubscribers = new List<IDisposable>();

            public Recipe? currentSelected = null;
            private bool disposedValue = false;
            private bool publishSubscribeInitialized = false;
        }
    }
}
