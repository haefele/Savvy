using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Views.AddTransaction;
using Savvy.Views.BudgetOverview;
using YnabApi;
using YnabApi.Dropbox;

namespace Savvy.Views.Shell.States
{
    public class OpenBudgetShellState : ShellState
    {
        private readonly WinRTContainer _container;
        private readonly INavigationService _navigationService;

        private readonly NavigationItemViewModel _overviewItem;
        private readonly NavigationItemViewModel _addTransactionItem;
        private readonly NavigationItemViewModel _refreshItem;

        private readonly NavigationItemViewModel _logoutItem;

        private Budget _budget;
        private RegisteredDevice _device;

        [Required]
        public string AccessCode { get; set; }
        [Required]
        public string BudgetName { get; set; }

        public OpenBudgetShellState(WinRTContainer container, INavigationService navigationService)
        {
            this._container = container;
            this._navigationService = navigationService;

            this._overviewItem = new NavigationItemViewModel(this.Overview) { Label = "Overview", Symbol = Symbol.Globe };
            this._addTransactionItem = new NavigationItemViewModel(this.AddTransaction) { Label = "Add transaction", Symbol = Symbol.Add };
            this._refreshItem = new NavigationItemViewModel(() => this.RefreshAsync()) { Label = "Refresh", Symbol = Symbol.Refresh };

            this._logoutItem = new NavigationItemViewModel(this.Logout) { Label = "Logout", Symbol = Symbol.LeaveChat };
        }

        public override async void Enter()
        {
            await this.RefreshAsync();

            this.ViewModel.Actions.Add(this._overviewItem);
            this.ViewModel.Actions.Add(this._addTransactionItem);
            this.ViewModel.Actions.Add(this._refreshItem);

            this.ViewModel.SecondaryActions.Add(this._logoutItem);

            this.Overview();
        }

        public override void Leave()
        {
            this._container.UnregisterHandler(typeof(YnabApi.YnabApi), null);

            this.ViewModel.Actions.Remove(this._overviewItem);
            this.ViewModel.Actions.Remove(this._addTransactionItem);
            this.ViewModel.Actions.Remove(this._refreshItem);
            
            this.ViewModel.SecondaryActions.Remove(this._logoutItem);
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
        
        private async Task RefreshAsync()
        {
            var newApi = new YnabApi.YnabApi(new DropboxFileSystem(this.AccessCode));
            var tempBudget = await newApi.GetBudgetAsync(this.BudgetName);
            var tempDevice = await tempBudget.RegisterDevice(Windows.Networking.Proximity.PeerFinder.DisplayName);
            
            this._container.UnregisterHandler(typeof(YnabApi.YnabApi), null);
            this._container.Instance(new YnabApi.YnabApi(new DropboxFileSystem(this.AccessCode)));

            this._budget = tempBudget;
            this._device = tempDevice;

            this._overviewItem.Label = $"{this._budget.BudgetName} overview";
        }

        private void Logout()
        {
            this.ViewModel.CurrentState = IoC.Get<LoggedOutShellState>();
        }
    }
}