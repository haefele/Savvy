using System.ComponentModel.DataAnnotations;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Services.Loading;
using Savvy.Services.Navigation;
using Savvy.Services.SessionState;
using Savvy.Views.AddTransaction;
using Savvy.Views.AllTransactions;
using Savvy.Views.BudgetOverview;
using Savvy.Views.Shell;
using Savvy.YnabApiFileSystem;
using YnabApi;

namespace Savvy.States
{
    public class OpenBudgetApplicationState : ApplicationState
    {
        private readonly WinRTContainer _container;
        private readonly ISavvyNavigationService _navigationService;
        private readonly ILoadingService _loadingService;
        private readonly ISessionStateService _sessionStateService;

        private readonly NavigationItemViewModel _overviewItem;
        private readonly NavigationItemViewModel _transactionsItem;
        private readonly NavigationItemViewModel _addTransactionItem;
        private readonly NavigationItemViewModel _refreshItem;

        private readonly NavigationItemViewModel _changeBudgetItem;

        private Budget _budget;
        private RegisteredDevice _device;

        [Required]
        public string BudgetName { get; set; }

        public OpenBudgetApplicationState(WinRTContainer container, ISavvyNavigationService navigationService, ILoadingService loadingService, ISessionStateService sessionStateService)
        {
            this._container = container;
            this._navigationService = navigationService;
            this._loadingService = loadingService;
            this._sessionStateService = sessionStateService;

            this._overviewItem = new NavigationItemViewModel(this.Overview) { Label = "Overview", Symbol = Symbol.Globe };
            this._transactionsItem = new NavigationItemViewModel(this.Transactions) { Label = "Transactions", Symbol = Symbol.AllApps };
            this._addTransactionItem = new NavigationItemViewModel(this.AddTransaction) { Label = "Add transaction", Symbol = Symbol.Add };
            this._refreshItem = new NavigationItemViewModel(this.RefreshAsync) { Label = "Refresh", Symbol = Symbol.Refresh };

            this._changeBudgetItem = new NavigationItemViewModel(this.ChangeBudget) { Label = "Change budget", Symbol = Symbol.Switch };
        }

        public override async void Enter()
        {
            var api = await this._container.RegisterYnabApiAsync();

            this._budget = await api.GetBudgetAsync(this.BudgetName);
            this._device = await this._budget.RegisterDevice(Windows.Networking.Proximity.PeerFinder.DisplayName);

            this.Application.Actions.Add(this._overviewItem);
            this.Application.Actions.Add(this._transactionsItem);
            this.Application.Actions.Add(this._addTransactionItem);
            this.Application.Actions.Add(this._refreshItem);

            this.Application.SecondaryActions.Add(this._changeBudgetItem);

            this.Overview();
        }

        public override void Leave()
        {
            this._container.UnregisterYnabApi();

            this.Application.Actions.Remove(this._overviewItem);
            this.Application.Actions.Remove(this._transactionsItem);
            this.Application.Actions.Remove(this._addTransactionItem);
            this.Application.Actions.Remove(this._refreshItem);
            
            this.Application.SecondaryActions.Remove(this._changeBudgetItem);
        }

        private void Overview()
        {
            this._navigationService
                .For<BudgetOverviewViewModel>()
                .Navigate();
        }

        private void Transactions()
        {
            this._navigationService
                .For<AllTransactionsViewModel>()
                .Navigate();
        }

        private void AddTransaction()
        {
            this._navigationService
                .For<AddTransactionViewModel>()
                .Navigate();
        }
        
        private async void RefreshAsync()
        {
            using (this._loadingService.Show("Refreshing..."))
            {
                var fileSystem = this._container.GetInstance<HybridFileSystem>();
                await fileSystem.Synchronization.RefreshLocalStateAsync();
            }
        }

        private void ChangeBudget()
        {
            this._sessionStateService.BudgetName = null;
            this.Application.CurrentState = IoC.Get<LoggedInApplicationState>();
        }
    }
}