﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ChatClient.Converter;

public class ActualWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double actualWidth = (double)value;
        double subtractValue = double.Parse(parameter.ToString());
        return actualWidth - subtractValue;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
