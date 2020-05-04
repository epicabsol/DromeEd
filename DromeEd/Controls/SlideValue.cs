using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd.Controls
{
    /// <summary>
    /// Represents a floating-point value that transitions to a target value over time.
    /// </summary>
    public class SlideValue
    {
        public float CurrentValue;
        public float TargetValue;

        public float Speed = 0.6f;
        public float Threshold = 1.5f;

        public SlideValue()
        {

        }

        public SlideValue(float currentValue, float targetValue)
        {
            CurrentValue = currentValue;
            TargetValue = targetValue;
        }

        public void Update()
        {
            CurrentValue = TargetValue * Speed + CurrentValue * (1.0f - Speed);
            if (CurrentValue - TargetValue > -Threshold && CurrentValue - TargetValue < Threshold)
            {
                CurrentValue = TargetValue;
            }
        }
    }
}
