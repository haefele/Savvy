using Caliburn.Micro;
using Savvy.Services.Navigation;
using Savvy.Services.SessionState;
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

                this._navigationService.ClearBackStack();
            }
        }

        public ShellViewModel(ISavvyNavigationService navigationService, ISessionStateService sessionStateService)
        {
            this._navigationService = navigationService;
            this._sessionStateService = sessionStateService;

            this.Actions = new BindableCollection<NavigationItemViewModel>();
            this.SecondaryActions = new BindableCollection<NavigationItemViewModel>();
        }
    }
}