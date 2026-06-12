using Avalonia.Data.Converters;
using FlowDesk.Core.Enums;
using System;
using System.Globalization;

namespace FlowDesk.Desktop.Converters;

public class TaskStatusToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FlowDesk.Core.Enums.TaskStatus status)
        {
            return status == FlowDesk.Core.Enums.TaskStatus.Done;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isChecked)
        {
            return isChecked ? FlowDesk.Core.Enums.TaskStatus.Done : FlowDesk.Core.Enums.TaskStatus.ToDo;
        }
        return FlowDesk.Core.Enums.TaskStatus.ToDo;
    }
}
