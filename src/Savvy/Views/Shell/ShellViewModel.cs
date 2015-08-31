using System;
using System.Linq;
using System.Windows.Input;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Caliburn.Micro;
using Savvy.Services.Settings;
using Savvy.Views.Shell.States;

namespace Savvy.Views.Shell
{
    public class ShellViewModel : Screen
    {
        private readonly INavigationService _navigationService;

        private ShellState _currentState;

        public BindableCollection<NavigationItemViewModel> Actions { get; }
        public BindableCollection<NavigationItemViewModel> SecondaryActions { get; }

        public ShellState CurrentState
        {
            get { return this._currentState; }
            set
            {
                this._currentState?.Leave();

                this._currentState = value;
                this._currentState.ViewModel = this;

                this._currentState?.Enter();

                this._navigationService.BackStack.Clear();
            }
        }

        public ShellViewModel(INavigationService navigationService)
        {
            this._navigationService = navigationService;

            this.Actions = new BindableCollection<NavigationItemViewModel>();
            this.SecondaryActions = new BindableCollection<NavigationItemViewModel>();
        }
        
        protected override void OnActivate()
        {
            this.CurrentState = IoC.Get<LoggedOutShellState>();
        }
    }
}