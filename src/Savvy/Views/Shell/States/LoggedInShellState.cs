using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Services.Loading;
using YnabApi;
using YnabApi.Dropbox;

namespace Savvy.Views.Shell.States
{
    public class LoggedInShellState : ShellState
    {
        private readonly WinRTContainer _container;
        private readonly INavigationService _navigationService;
        private readonly ILoadingService _loadingService;

        private readonly NavigationItemViewModel _logoutItem;

        private IList<NavigationItemViewModel> _budgetItems;
            
        [Required]
        public string AccessCode { get; set; }

        public LoggedInShellState(WinRTContainer container, INavigationService navigationService, ILoadingService loadingService)
        {
            this._container = container;
            this._navigationService = navigationService;
            this._loadingService = loadingService;

            this._logoutItem = new NavigationItemViewModel(this.Logout) { Label = "Logout", Symbol = Symbol.LeaveChat };
        }

        public override async void Enter()
        {
            using (this._loadingService.Show("Loading budgets..."))
            { 
                var api = this._container.RegisterYnabApi(this.AccessCode);
            
                this.ViewModel.SecondaryActions.Add(this._logoutItem);

                IList<Budget> budgets = await api.GetBudgetsAsync();

                this._budgetItems = budgets
                    .Select(f => new NavigationItemViewModel(() => this.OpenBudget(f)) {Label = f.BudgetName, Symbol = Symbol.Account})
                    .ToList();

                this.ViewModel.Actions.AddRange(this._budgetItems);
            }
        }

        public override void Leave()
        {
            this._container.UnregisterYnabApi();

            this.ViewModel.SecondaryActions.Remove(this._logoutItem);

            foreach (var budgetItem in this._budgetItems)
            {
                this.ViewModel.Actions.Remove(budgetItem);
            }
        }
        
        private void OpenBudget(Budget budget)
        {
            var newState = IoC.Get<OpenBudgetShellState>();
            newState.AccessCode = this.AccessCode;
            newState.BudgetName = budget.BudgetName;

            this.ViewModel.CurrentState = newState;
        }

        private void Logout()
        {
            this.ViewModel.CurrentState = IoC.Get<LoggedOutShellState>();
        }
    }
}