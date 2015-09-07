using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Services.DropboxAuthentication;
using Savvy.Services.Loading;
using Savvy.Services.SessionState;
using Savvy.Views.AllBudgetsOverview;
using Savvy.YnabApiFileSystem;
using YnabApi;

namespace Savvy.Views.Shell.States
{
    public class LoggedInShellState : ShellState
    {
        private readonly WinRTContainer _container;
        private readonly INavigationService _navigationService;
        private readonly ILoadingService _loadingService;
        private readonly ISessionStateService _sessionStateService;

        private readonly NavigationItemViewModel _refreshItem;
        private readonly NavigationItemViewModel _logoutItem;

        private IList<NavigationItemViewModel> _budgetItems;
        
        public LoggedInShellState(WinRTContainer container, INavigationService navigationService, ILoadingService loadingService, ISessionStateService sessionStateService)
        {
            this._container = container;
            this._navigationService = navigationService;
            this._loadingService = loadingService;
            this._sessionStateService = sessionStateService;

            this._refreshItem = new NavigationItemViewModel(async () => await this.RefreshAsync()) { Label = "Refresh", Symbol = Symbol.Refresh };
            this._logoutItem = new NavigationItemViewModel(this.Logout) { Label = "Logout", Symbol = Symbol.LeaveChat };
        }

        public override async void Enter()
        {
            using (this._loadingService.Show("Loading budgets..."))
            {
                this._navigationService
                    .For<AllBudgetsOverviewViewModel>()
                    .Navigate();

                var api = await this._container.RegisterYnabApiAsync();

                IList<Budget> budgets = await api.GetBudgetsAsync();

                if (budgets.Any())
                {
                    this.AddActions(budgets);
                }
                else
                {
                    await this.RefreshAsync();
                }
            }
        }

        public override void Leave()
        {
            this._container.UnregisterYnabApi();
            this.RemoveActions();
        }

        private void AddActions(IList<Budget> budgets)
        {
            this.ViewModel.SecondaryActions.Add(this._logoutItem);

            this._budgetItems = budgets
                .Select(f => new NavigationItemViewModel(() => this.OpenBudget(f)) { Label = f.BudgetName, Symbol = Symbol.Folder })
                .ToList();

            this.ViewModel.Actions.AddRange(this._budgetItems);
            this.ViewModel.Actions.Add(this._refreshItem);
        }

        private void RemoveActions()
        {
            this.ViewModel.SecondaryActions.Remove(this._logoutItem);
            
            foreach (var budgetItem in this._budgetItems ?? new List<NavigationItemViewModel>())
            {
                this.ViewModel.Actions.Remove(budgetItem);
            }
            this.ViewModel.Actions.Remove(this._refreshItem);
        }
        
        private void OpenBudget(Budget budget)
        {
            this._sessionStateService.BudgetName = budget.BudgetName;

            var newState = IoC.Get<OpenBudgetShellState>();
            newState.BudgetName = budget.BudgetName;

            this.ViewModel.CurrentState = newState;
        }

        private async Task RefreshAsync()
        {
            using (this._loadingService.Show("Refreshing..."))
            {
                this._container.UnregisterYnabApi();

                var api = await this._container.RegisterYnabApiAsync();
                var fileSystem = this._container.GetInstance<HybridFileSystem>();

                await fileSystem.Synchronization.RefreshLocalStateAsync();
                
                this.RemoveActions();
                this.AddActions(await api.GetBudgetsAsync());
            }
        }

        private void Logout()
        {
            this._sessionStateService.DropboxAccessCode = null;
            this._sessionStateService.DropboxUserId = null;

            this.ViewModel.CurrentState = IoC.Get<LoggedOutShellState>();
        }
    }
}