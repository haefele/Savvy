﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Services.DropboxAuthentication;
using Savvy.Services.Loading;
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

        private readonly NavigationItemViewModel _refreshItem;
        private readonly NavigationItemViewModel _logoutItem;

        private IList<NavigationItemViewModel> _budgetItems;

        [Required]
        public DropboxAuth Auth { get; set; }

        public LoggedInShellState(WinRTContainer container, INavigationService navigationService, ILoadingService loadingService)
        {
            this._container = container;
            this._navigationService = navigationService;
            this._loadingService = loadingService;

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

                var api = await this._container.RegisterYnabApiAsync(this.Auth);

                IList<Budget> budgets = await api.GetBudgetsAsync();

                if (budgets.Count == 0)
                    await this.RefreshAsync();

                budgets = await api.GetBudgetsAsync();
                this.AddActions(budgets);
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
                .Select(f => new NavigationItemViewModel(() => this.OpenBudget(f)) { Label = f.BudgetName, Symbol = Symbol.OpenLocal })
                .ToList();

            this.ViewModel.Actions.AddRange(this._budgetItems);
            this.ViewModel.Actions.Add(this._refreshItem);
        }

        private void RemoveActions()
        {
            this.ViewModel.SecondaryActions.Remove(this._logoutItem);

            foreach (var budgetItem in this._budgetItems)
            {
                this.ViewModel.Actions.Remove(budgetItem);
            }
            this.ViewModel.Actions.Remove(this._refreshItem);
        }
        
        private void OpenBudget(Budget budget)
        {
            var newState = IoC.Get<OpenBudgetShellState>();
            newState.Auth = this.Auth;
            newState.BudgetName = budget.BudgetName;

            this.ViewModel.CurrentState = newState;
        }

        private async Task RefreshAsync()
        {
            using (this._loadingService.Show("Refreshing..."))
            {
                var api = this._container.GetInstance<YnabApi.YnabApi>();
                var fileSystem = this._container.GetInstance<HybridFileSystem>();

                await fileSystem.Synchronization.RefreshLocalStateAsync();
                
                this.RemoveActions();
                this.AddActions(await api.GetBudgetsAsync());
            }
        }

        private void Logout()
        {
            this.ViewModel.CurrentState = IoC.Get<LoggedOutShellState>();
        }
    }
}