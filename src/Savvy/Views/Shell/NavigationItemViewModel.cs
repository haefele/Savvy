using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Extensions;
using Action = System.Action;

namespace Savvy.Views.Shell
{
    public class NavigationItemViewModel : PropertyChangedBase
    {
        private string _label;
        private Symbol _symbol;

        public NavigationItemViewModel(Action execute)
        {
            this.Execute = execute;
        }

        #region Properties
        public Action Execute { get; }
        public Symbol Symbol
        {
            get { return this._symbol; }
            set { this.SetProperty(ref this._symbol, value); }
        }
        public string Label
        {
            get { return this._label; }
            set { this.SetProperty(ref this._label, value); }
        }

        #endregion
    }
}