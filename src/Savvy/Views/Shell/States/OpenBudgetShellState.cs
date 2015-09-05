using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Services.DropboxAuthentication;
using Savvy.Services.Loading;
using Savvy.Views.AddTransaction;
using Savvy.Views.BudgetOverview;
using Savvy.YnabApiFileSystem;
using YnabApi;

namespace Savvy.Views.Shell.States
{
    public class OpenBudgetShellState : ShellState
    {
        private readonly WinRTContainer _container;
        private readonly INavigationService _navigationService;
        private readonly ILoadingService _loadingService;

        private readonly NavigationItemViewModel _overviewItem;
        private readonly NavigationItemViewModel _addTransactionItem;
        private readonly NavigationItemViewModel _refreshItem;

        private readonly NavigationItemViewModel _changeBudgetItem;

        private Budget _budget;
        private RegisteredDevice _device;

        [Required]
        public DropboxAuth Auth  { get; set; }
        [Required]
        public string BudgetName { get; set; }

        public OpenBudgetShellState(WinRTContainer container, INavigationService navigationService, ILoadingService loadingService)
        {
            this._container = container;
            this._navigationService = navigationService;
            this._loadingService = loadingService;

            this._overviewItem = new NavigationItemViewModel(this.Overview) { Label = "Overview", Symbol = Symbol.Globe };
            this._addTransactionItem = new NavigationItemViewModel(this.AddTransaction) { Label = "Add transaction", Symbol = Symbol.Add };
            this._refreshItem = new NavigationItemViewModel(this.RefreshAsync) { Label = "Refresh", Symbol = Symbol.Refresh };

            this._changeBudgetItem = new NavigationItemViewModel(this.ChangeBudget) { Label = "Change budget", Symbol = Symbol.Switch };
        }

        public override async void Enter()
        {
            var api = await this._container.RegisterYnabApiAsync(this.Auth);

            this._budget = await api.GetBudgetAsync(this.BudgetName);
            this._device = await this._budget.RegisterDevice(Windows.Networking.Proximity.PeerFinder.DisplayName);

            this.ViewModel.Actions.Add(this._overviewItem);
            this.ViewModel.Actions.Add(this._addTransactionItem);
            this.ViewModel.Actions.Add(this._refreshItem);

            this.ViewModel.SecondaryActions.Add(this._changeBudgetItem);

            this.Overview();
        }

        public override void Leave()
        {
            this._container.UnregisterYnabApi();

            this.ViewModel.Actions.Remove(this._overviewItem);
            this.ViewModel.Actions.Remove(this._addTransactionItem);
            this.ViewModel.Actions.Remove(this._refreshItem);
            
            this.ViewModel.SecondaryActions.Remove(this._changeBudgetItem);
        }

        private void Overview()
        {
            this._navigationService
                .For<BudgetOverviewViewModel>()
                .WithParam(f => f.BudgetName, this._budget.BudgetName)
                .WithParam(f => f.DeviceGuid, this._device.DeviceGuid)
                .Navigate();
        }

        private void AddTransaction()
        {
            this._navigationService
                .For<AddTransactionViewModel>()
                .WithParam(f => f.BudgetName, this._budget.BudgetName)
                .WithParam(f => f.DeviceGuid, this._device.DeviceGuid)
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
            var newState = IoC.Get<LoggedInShellState>();
            newState.Auth = this.Auth;

            this.ViewModel.CurrentState = newState;
        }
    }
}