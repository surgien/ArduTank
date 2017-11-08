using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ControlUnit.Controller.App.Controls
{
    public class CustomSlider : Slider
    {
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            var steps = 10;
            if (newValue - oldValue > steps || newValue - oldValue < -steps)
            {
                Task.Run(async () =>
                {
                    var rest = newValue % steps;
                    var iterations = (newValue - rest) / steps;

                    var tmpStep = 0;

                    for (double tmpValue = oldValue; tmpValue != newValue; tmpValue += tmpStep)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            base.OnValueChanged(oldValue, tmpValue);
                        });

                        if (newValue - tmpValue < steps) tmpValue += newValue - tmpValue;
                        else tmpValue += steps;
                        await Task.Delay(300);
                    }
                });
            }
            else
            {
                base.OnValueChanged(oldValue, newValue);
            }
        }
    }
}
