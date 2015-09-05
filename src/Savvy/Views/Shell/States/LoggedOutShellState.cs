using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Services.DropboxAuthentication;
using Savvy.Views.Welcome;
using Savvy.YnabApiFileSystem;

namespace Savvy.Views.Shell.States
{
    public class LoggedOutShellState : ShellState
    {
        private readonly INavigationService _navigationService;
        private readonly IDropboxAuthenticationService _dropboxAuthenticationService;

        private readonly NavigationItemViewModel _loginItem;

        public LoggedOutShellState(INavigationService navigationService, IDropboxAuthenticationService dropboxAuthenticationService)
        {
            this._navigationService = navigationService;
            this._dropboxAuthenticationService = dropboxAuthenticationService;

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
            
            var newState = IoC.Get<LoggedInShellState>();
            newState.Auth = auth;

            this.ViewModel.CurrentState = newState;
        }
    }
}