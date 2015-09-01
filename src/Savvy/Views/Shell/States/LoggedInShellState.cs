using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        private IList<NavigationItemViewModel> _budgetItems;
            
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

            this._budgetItems = budgets
                .Select(f => new NavigationItemViewModel(() => this.OpenBudget(f)) {Label = f.BudgetName, Symbol = Symbol.Account})
                .ToList();

            this.ViewModel.Actions.AddRange(this._budgetItems);
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

            foreach (var budgetItem in this._budgetItems)
            {
                this.ViewModel.Actions.Remove(budgetItem);
            }
        }

        private void Logout()
        {
            this.ViewModel.CurrentState = IoC.Get<LoggedOutShellState>();
        }
    }
}