using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ControlUnit.Controller.App.Converter
{
    public class DalayConverter : IValueConverter
    {
        private bool _isDelayed = false;
        private object _lastValue;
        private object _currentValue;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;

            if (!_isDelayed)
            {
                _lastValue = value;
                _isDelayed = true;

                Task.Run(async () =>
                {
                    await Task.Delay(1111);
                    _isDelayed = false;
                    _currentValue = _lastValue;
                });

                return value;
            }
            else
            {
                _currentValue = value;
                return _lastValue;
            }
        }
    }
}
