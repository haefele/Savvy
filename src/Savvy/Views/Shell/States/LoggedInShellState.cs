using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using YnabApi;
using YnabApi.Dropbox;

namespace Savvy.Views.Shell.States
{
    public class LoggedInShellState : ShellState
    {
        private readonly WinRTContainer _container;
        private readonly INavigationService _navigationService;

        private readonly NavigationItemViewModel _logoutItem;

        [Required]
        public string AccessCode { get; set; }

        public LoggedInShellState(WinRTContainer container, INavigationService navigationService)
        {
            this._container = container;
            this._navigationService = navigationService;

            this._logoutItem = new NavigationItemViewModel(this.Logout) { Label = "Logout", Symbol = Symbol.LeaveChat };
        }

        public override async void Enter()
        {
            var api = new YnabApi.YnabApi(new DropboxFileSystem(this.AccessCode));
            this._container.Instance(api);
            
            this.ViewModel.SecondaryActions.Add(this._logoutItem);

            IList<Budget> budgets = await api.GetBudgetsAsync();

            foreach (var budget in budgets)
            {
                this.ViewModel.Actions.Add(new NavigationItemViewModel(() => this.OpenBudget(budget)) { Label = budget.BudgetName, Symbol = Symbol.Account });
            }
        }

        private void OpenBudget(Budget budget)
        {
            var newState = IoC.Get<OpenBudgetShellState>();
            newState.AccessCode = this.AccessCode;
            newState.BudgetName = budget.BudgetName;

            this.ViewModel.CurrentState = newState;
        }

        public override void Leave()
        {
            this._container.UnregisterHandler(typeof(YnabApi.YnabApi), null);
            this.ViewModel.SecondaryActions.Remove(this._logoutItem);
            this.ViewModel.Actions.Clear();
        }

        private void Logout()
        {
            this.ViewModel.CurrentState = IoC.Get<LoggedOutShellState>();
        }
    }
}