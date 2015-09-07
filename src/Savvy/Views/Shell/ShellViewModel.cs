using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Caliburn.Micro;
using Savvy.Services.SessionState;
using Savvy.Services.Settings;
using Savvy.Views.Shell.States;

namespace Savvy.Views.Shell
{
    public class ShellViewModel : Screen
    {
        private readonly INavigationService _navigationService;
        private readonly ISessionStateService _sessionStateService;

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

        public ShellViewModel(INavigationService navigationService, ISessionStateService sessionStateService)
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
                    var newShellState = IoC.Get<OpenBudgetShellState>();
                    newShellState.BudgetName = this._sessionStateService.BudgetName;

                    this.CurrentState = newShellState;
                }
                else
                {
                    this.CurrentState = IoC.Get<LoggedInShellState>();
                }
            }
            else
            {
                this.CurrentState = IoC.Get<LoggedOutShellState>();
            }
        }
    }
}