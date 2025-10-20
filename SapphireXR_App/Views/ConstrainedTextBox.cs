using SapphireXR_App.Common;
using System.Windows.Controls;
using System.Windows;

namespace SapphireXR_App.Views
{   public class NumberBox : TextBox
    {
        public NumberBox(FlowControllerTextBoxValidater.NumberType numberType) : base()
        {
            TextChanged += OnlyAllowNumber;
            flowControllerTextBoxValidaterOnlyNumber = new FlowControllerTextBoxValidaterOnlyNumber(numberType);
            if (numberType == FlowControllerTextBoxValidater.NumberType.Float)
            {
                LostFocus += Util.TrimLastDotOnLostFocus;
            }
        }

        protected void OnlyAllowNumber(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox != null)
            {
                string validatedFlowControllerValue = flowControllerTextBoxValidaterOnlyNumber.valdiate(textBox);
                if (validatedFlowControllerValue != textBox.Text)
                {
                    int caretIndex = Math.Max(textBox.CaretIndex - 1, 0);
                    textBox.Text = validatedFlowControllerValue;
                    textBox.CaretIndex = caretIndex;
                }
                
            }
        }

        private FlowControllerTextBoxValidaterOnlyNumber flowControllerTextBoxValidaterOnlyNumber;
    }

    public class IntegerBox: NumberBox
    {
        public IntegerBox(): base(FlowControllerTextBoxValidater.NumberType.Integer)
        { }
    }

    public class FloatingPointBox : NumberBox
    {
        public FloatingPointBox() : base(FlowControllerTextBoxValidater.NumberType.Float)
        { }
    }

    public class NumberBoxWithMax : TextBox
    {
        public NumberBoxWithMax(FlowControllerTextBoxValidater.NumberType numberType) : base()
        {
            flowControllerTextBoxValidater = new FlowControllerTextBoxValidaterMaxValue(numberType);
            TextChanged += onlyAllowNumberWithMax;
            if(numberType == FlowControllerTextBoxValidater.NumberType.Float)
            {
                LostFocus += Util.TrimLastDotOnLostFocus;
            }
        }

        protected void onlyAllowNumberWithMax(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox != null)
            {
                (string validatedFlowControllerValue, FlowControllerTextBoxValidaterMaxValue.Result result) = flowControllerTextBoxValidater.valdiate(textBox, (uint)MaxValue);
                switch(result)
                {
                    case FlowControllerTextBoxValidaterMaxValue.Result.NotNumber:
                    case FlowControllerTextBoxValidaterMaxValue.Result.ExceedMax:
                        int caretIndex = Math.Max(0, textBox.CaretIndex - 1);
                        textBox.Text = validatedFlowControllerValue;
                        textBox.CaretIndex = caretIndex;
                        break;
                }
            }
        }

        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        private static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(NumberBoxWithMax), new PropertyMetadata(int.MinValue));
        private FlowControllerTextBoxValidaterMaxValue flowControllerTextBoxValidater;
    }

    public class IntegerBoxWithMax : NumberBoxWithMax
    {
        public IntegerBoxWithMax() : base(FlowControllerTextBoxValidater.NumberType.Integer)
        { }
    }

    public class FloatingPointBoxWithMax : NumberBoxWithMax
    {
        public FloatingPointBoxWithMax() : base(FlowControllerTextBoxValidater.NumberType.Float)
        { }
    }
}
