using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Caliburn.Micro;

namespace Savvy.Extensions
{
    public static class PropertyChangedBaseExtensions
    {
        public static bool SetProperty<T>(this PropertyChangedBase viewModel, ref T field, T newValue, [CallerMemberName]string propertyName = null)
        {
            if (Equals(field, newValue))
                return false;

            field = newValue;
            viewModel.NotifyOfPropertyChange(propertyName);

            return true;
        }
    }
}