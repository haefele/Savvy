using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Services.DropboxAuthentication;
using Savvy.Services.Navigation;
using Savvy.Services.SessionState;
using Savvy.Views.Shell;
using Savvy.Views.Welcome;

namespace Savvy.States
{
    public class LoggedOutApplicationState : ApplicationState
    {
        private readonly ISavvyNavigationService _navigationService;
        private readonly IDropboxAuthenticationService _dropboxAuthenticationService;
        private readonly ISessionStateService _sessionStateService;

        private readonly NavigationItemViewModel _loginItem;

        public LoggedOutApplicationState(ISavvyNavigationService navigationService, IDropboxAuthenticationService dropboxAuthenticationService, ISessionStateService sessionStateService)
        {
            this._navigationService = navigationService;
            this._dropboxAuthenticationService = dropboxAuthenticationService;
            this._sessionStateService = sessionStateService;

            this._loginItem = new NavigationItemViewModel(this.Login) { Label = "Login", Symbol = Symbol.NewWindow };
        }

        public override void Enter()
        {
            this._navigationService.For<WelcomeViewModel>().Navigate();

            this.Application.Actions.Add(this._loginItem);
        }

        public override void Leave()
        {
            this.Application.Actions.Remove(this._loginItem);
        }

        private async void Login()
        {
            var auth = await this._dropboxAuthenticationService.LoginAndGetAccessCodeAsync();

            if (auth == null)
                return;

            this._sessionStateService.DropboxUserId = auth.UserId;
            this._sessionStateService.DropboxAccessCode = auth.AccessCode;

            this.Application.CurrentState = IoC.Get<LoggedInApplicationState>();
        }
    }
}