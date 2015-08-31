using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using YnabApi.Dropbox;

namespace Savvy.Views.Shell.States
{
    public class LoggedInShellState : ShellState
    {
        private readonly WinRTContainer _container;
        private readonly INavigationService _navigationService;

        private readonly NavigationItemViewModel _logoutItem;

        public string AccessCode { get; set; }

        public LoggedInShellState(WinRTContainer container, INavigationService navigationService)
        {
            this._container = container;
            this._navigationService = navigationService;

            this._logoutItem = new NavigationItemViewModel(this.Logout) { Label = "Logout", Symbol = Symbol.LeaveChat };
        }

        public override void Enter()
        {
            this._container.Instance(new YnabApi.YnabApi(new DropboxFileSystem(this.AccessCode)));

            this.ViewModel.BottomActions.Add(this._logoutItem);
        }

        public override void Leave()
        {
            this._container.UnregisterHandler(typeof(YnabApi.YnabApi), null);
            this.ViewModel.BottomActions.Remove(this._logoutItem);
        }

        private void Logout()
        {
            this.ViewModel.CurrentState = IoC.Get<LoggedOutShellState>();
        }
    }
}