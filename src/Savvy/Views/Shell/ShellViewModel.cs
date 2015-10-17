using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Caliburn.Micro;
using Savvy.Services.Navigation;
using Savvy.Services.SessionState;
using Savvy.Services.Settings;
using Savvy.States;

namespace Savvy.Views.Shell
{
    public class ShellViewModel : Screen, IApplication
    {
        private readonly ISavvyNavigationService _navigationService;
        private readonly ISessionStateService _sessionStateService;

        private ApplicationState _currentState;

        public BindableCollection<NavigationItemViewModel> Actions { get; }
        public BindableCollection<NavigationItemViewModel> SecondaryActions { get; }

        public ApplicationState CurrentState
        {
            get { return this._currentState; }
            set
            {
                this._currentState?.Leave();

                this._currentState = value;
                this._currentState.Application = this;

                this._currentState?.Enter();
            }
        }

        public ShellViewModel(ISavvyNavigationService navigationService, ISessionStateService sessionStateService)
        {
            this._navigationService = navigationService;
            this._sessionStateService = sessionStateService;

            this.Actions = new BindableCollection<NavigationItemViewModel>();
            this.SecondaryActions = new BindableCollection<NavigationItemViewModel>();
        }
        
        protected override void OnActivate()
        {
            if (this._sessionStateService.DropboxAccessCode != null &&
                   this._sessionStateService.DropboxUserId != null)
            {
                if (this._sessionStateService.BudgetName != null)
                {
                    var newShellState = IoC.Get<OpenBudgetApplicationState>();
                    newShellState.BudgetName = this._sessionStateService.BudgetName;

                    this.CurrentState = newShellState;
                }
                else
                {
                    this.CurrentState = IoC.Get<LoggedInApplicationState>();
                }
            }
            else
            {
                this.CurrentState = IoC.Get<LoggedOutApplicationState>();
            }
        }
    }
}