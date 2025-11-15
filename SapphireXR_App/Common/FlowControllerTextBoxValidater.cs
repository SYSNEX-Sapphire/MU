using CommunityToolkit.Mvvm.ComponentModel;
using SapphireXR_App.Models;
using SapphireXR_App.ViewModels;
using SapphireXR_App.WindowServices;
using System.ComponentModel;
using System.Windows.Controls;

namespace SapphireXR_App.Common
{
    public abstract class FlowControllerTextBoxValidater
    {
        public enum NumberType { Integer = 0, Float = 1 };

        public FlowControllerTextBoxValidater(NumberType numType)
        {
            isTextNumeric = (numType == NumberType.Integer ? Util.IsTextNumeric : Util.IsTextFloatintgPoint);
        }

        protected string prevText = "";
        protected Func<string, bool> isTextNumeric;
    }
    internal class FlowControllerTextBoxValidaterOnlyNumber: FlowControllerTextBoxValidater
    {
        public FlowControllerTextBoxValidaterOnlyNumber(NumberType numType): base(numType)
        {
        }

        public string valdiate(TextBox textBox)
        {
            if (textBox.Text == "" || isTextNumeric(textBox.Text) == true)
            {
                prevText = textBox.Text;
            }

            return prevText;
        }
    }

    internal class FlowControllerTextBoxValidaterMaxValue: FlowControllerTextBoxValidater
    {
        public enum Result { Valid = 0, NotNumber = 1, ExceedMax, Undefined };

        public FlowControllerTextBoxValidaterMaxValue(NumberType numTypeVal): base(numTypeVal)
        {
            numType = numTypeVal;
        }

        public (string, Result) valdiate(TextBox textBox, uint maxValue)
        {
            try
            {
                if (textBox.Text == "")
                {
                    prevText = textBox.Text;
                    return (prevText, Result.Valid);
                }

                if (isTextNumeric(textBox.Text) == false)
                {
                    return (prevText, Result.NotNumber);
                }

                switch (numType)
                {
                    case NumberType.Integer:
                        if (maxValue < uint.Parse(textBox.Text))
                        {
                            return (prevText, Result.ExceedMax);
                        }
                        break;
                    case NumberType.Float:
                        string currentValue = textBox.Text;
                        if (currentValue.EndsWith('.') == true)
                        {
                            currentValue = currentValue[..^1];
                        }
                        if (maxValue < float.Parse(currentValue))
                        {
                            return (prevText, Result.ExceedMax);
                        }
                        break;
                }
            }
            catch (Exception)
            {
                return (prevText, Result.NotNumber);
            }

            prevText = textBox.Text;
            return (prevText, Result.Valid);
        }

        public (string, Result) valdiate(TextBox textBox, string flowControllerID)
        {
            int? maxValue = SettingViewModel.ReadMaxValue(flowControllerID);
            if (maxValue != null)
            {
                return valdiate(textBox, (uint)maxValue);
            }
            else
            {
                throw new Exception("Failure happend in validating max value in numeric value of the constrained text box. Logic error in FlowControllerTextBoxValidaterMaxValue.validate: " +
                    "the value of \"flowControllerID\", the second argument of the validate method \"" + flowControllerID + "\" is not valid");
            }
        }

        private NumberType numType;
    }
    
    internal abstract class FlowControllerDataGridTextColumnTextBoxValidater
    {
        internal FlowControllerDataGridTextColumnTextBoxValidater(ObservableObject viewModel, string recipesPropertyName)
        {
            viewModel.PropertyChanged += (object? sender, PropertyChangedEventArgs args) =>
            {
                if (args.PropertyName == recipesPropertyName)
                {
                    prevTexts.Clear();
                }
            };
        }

        protected abstract FlowControllerTextBoxValidater CreateValidater(FlowControllerTextBoxValidater.NumberType numberType);

        protected FlowControllerTextBoxValidater get(TextBox textBox, FlowControllerTextBoxValidater.NumberType numberType)
        {
            FlowControllerTextBoxValidater? validater = null;
            if (prevTexts.TryGetValue(textBox, out validater) == false)
            {
                validater = CreateValidater(numberType);
                prevTexts.Add(textBox, validater);
            }
            return validater;
        }

        protected Dictionary<TextBox, FlowControllerTextBoxValidater> prevTexts = new Dictionary<TextBox, FlowControllerTextBoxValidater>();
    }
    internal class FlowControllerDataGridTextColumnTextBoxValidaterOnlyNumber : FlowControllerDataGridTextColumnTextBoxValidater
    {
        internal FlowControllerDataGridTextColumnTextBoxValidaterOnlyNumber(ObservableObject viewModel, string recipesPropertyName) : base(viewModel, recipesPropertyName) { }

        protected override FlowControllerTextBoxValidater CreateValidater(FlowControllerTextBoxValidater.NumberType numberType)
        {
            return new FlowControllerTextBoxValidaterOnlyNumber(numberType);
        }

        public string validate(TextBox textBox, FlowControllerTextBoxValidater.NumberType numberType)
        {
            return ((FlowControllerTextBoxValidaterOnlyNumber)get(textBox, numberType)).valdiate(textBox);
        }
    }
    internal class FlowControllerDataGridTextColumnTextBoxValidaterMaxValue: FlowControllerDataGridTextColumnTextBoxValidater
    {
        internal FlowControllerDataGridTextColumnTextBoxValidaterMaxValue(ObservableObject viewModel, string recipesPropertyName): base(viewModel, recipesPropertyName) { }

        protected override FlowControllerTextBoxValidater CreateValidater(FlowControllerTextBoxValidater.NumberType numberType)
        {
            return new FlowControllerTextBoxValidaterMaxValue(numberType);
        }

        public (string, FlowControllerTextBoxValidaterMaxValue.Result) validate(TextBox textBox, string flowControllerID, FlowControllerTextBoxValidater.NumberType numberType)
        {
            return ((FlowControllerTextBoxValidaterMaxValue)get(textBox, numberType)).valdiate(textBox, flowControllerID);
        }

        public (string, FlowControllerTextBoxValidaterMaxValue.Result) validate(TextBox textBox, uint maxValue, FlowControllerTextBoxValidater.NumberType numberType)
        {
            return ((FlowControllerTextBoxValidaterMaxValue)get(textBox, numberType)).valdiate(textBox, maxValue);
        }

        public (string?, FlowControllerTextBoxValidaterMaxValue.Result) validate(TextBox textBox, TextChangedEventArgs e, FlowControllerTextBoxValidater.NumberType numberType)
        {
            DataGridCell? dataGridCell = textBox.Parent as DataGridCell;
            if (dataGridCell != null)
            {
                string? flowControlField = dataGridCell.Column.Header as string;
                if (flowControlField != null)
                {
                    switch(flowControlField)
                    {
                        case "Susceptor Temp.":
                        case "Reactor Press.":
                        case "Sus. Rotation":
                            string? controllerID;
                            if (DeviceConfiguration.RecipeReactorColumnHeaderToControllerID.TryGetValue(flowControlField, out controllerID) == true)
                            {
                                flowControlField = controllerID;
                            }
                            else
                            {
                                flowControlField = null;
                            }
                            break;

                    }
                    if (flowControlField != null)
                    {
                        return validate(textBox, flowControlField, numberType);
                    }
                }
            }
            return (null, FlowControllerTextBoxValidaterMaxValue.Result.Undefined);
        }
    }
}
