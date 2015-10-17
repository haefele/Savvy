using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Services.Loading;
using Savvy.Services.Navigation;
using Savvy.Services.SessionState;
using Savvy.Views.AllBudgetsOverview;
using Savvy.Views.Shell;
using Savvy.YnabApiFileSystem;
using YnabApi;

namespace Savvy.States
{
    public class LoggedInApplicationState : ApplicationState
    {
        private readonly WinRTContainer _container;
        private readonly ISavvyNavigationService _navigationService;
        private readonly ILoadingService _loadingService;
        private readonly ISessionStateService _sessionStateService;

        private readonly NavigationItemViewModel _refreshItem;
        private readonly NavigationItemViewModel _logoutItem;

        private IList<NavigationItemViewModel> _budgetItems;
        
        public LoggedInApplicationState(WinRTContainer container, ISavvyNavigationService navigationService, ILoadingService loadingService, ISessionStateService sessionStateService)
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
            this.Application.SecondaryActions.Add(this._logoutItem);

            this._budgetItems = budgets
                .Select(f => new NavigationItemViewModel(() => this.OpenBudget(f)) { Label = f.BudgetName, Symbol = Symbol.Folder })
                .ToList();

            this.Application.Actions.AddRange(this._budgetItems);
            this.Application.Actions.Add(this._refreshItem);
        }

        private void RemoveActions()
        {
            this.Application.SecondaryActions.Remove(this._logoutItem);
            
            foreach (var budgetItem in this._budgetItems ?? new List<NavigationItemViewModel>())
            {
                this.Application.Actions.Remove(budgetItem);
            }
            this.Application.Actions.Remove(this._refreshItem);
        }
        
        private void OpenBudget(Budget budget)
        {
            this._sessionStateService.BudgetName = budget.BudgetName;

            var newState = IoC.Get<OpenBudgetApplicationState>();
            newState.BudgetName = budget.BudgetName;

            this.Application.CurrentState = newState;
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

            this.Application.CurrentState = IoC.Get<LoggedOutApplicationState>();
        }
    }
}