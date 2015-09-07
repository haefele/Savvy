using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Services.DropboxAuthentication;
using Savvy.Services.SessionState;
using Savvy.Views.Welcome;
using Savvy.YnabApiFileSystem;

namespace Savvy.Views.Shell.States
{
    public class LoggedOutShellState : ShellState
    {
        private readonly INavigationService _navigationService;
        private readonly IDropboxAuthenticationService _dropboxAuthenticationService;
        private readonly ISessionStateService _sessionStateService;

        private readonly NavigationItemViewModel _loginItem;

        public LoggedOutShellState(INavigationService navigationService, IDropboxAuthenticationService dropboxAuthenticationService, ISessionStateService sessionStateService)
        {
            this._navigationService = navigationService;
            this._dropboxAuthenticationService = dropboxAuthenticationService;
            this._sessionStateService = sessionStateService;

            this._loginItem = new NavigationItemViewModel(this.Login) { Label = "Login", Symbol = Symbol.NewWindow };
        }

        public override void Enter()
        {
            this._navigationService.For<WelcomeViewModel>().Navigate();

            this.ViewModel.Actions.Add(this._loginItem);
        }

        public override void Leave()
        {
            this.ViewModel.Actions.Remove(this._loginItem);
        }

        private async void Login()
        {
            var auth = await this._dropboxAuthenticationService.LoginAndGetAccessCodeAsync();

            if (auth == null)
                return;

            this._sessionStateService.DropboxUserId = auth.UserId;
            this._sessionStateService.DropboxAccessCode = auth.AccessCode;
            
            this.ViewModel.CurrentState = IoC.Get<LoggedInShellState>();
        }
    }
}