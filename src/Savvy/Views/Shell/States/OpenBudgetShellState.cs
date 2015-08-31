using System.ComponentModel.DataAnnotations;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Views.AddTransaction;
using YnabApi;
using YnabApi.Dropbox;

namespace Savvy.Views.Shell.States
{
    public class OpenBudgetShellState : ShellState
    {
        private readonly WinRTContainer _container;
        private readonly INavigationService _navigationService;

        private readonly NavigationItemViewModel _addTransactionItem;
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

            this._addTransactionItem = new NavigationItemViewModel(this.AddTransaction) { Label = "Add transaction", Symbol = Symbol.Add };
            this._logoutItem = new NavigationItemViewModel(this.Logout) { Label = "Logout", Symbol = Symbol.LeaveChat };
        }

        public override async void Enter()
        {
            var api = new YnabApi.YnabApi(new DropboxFileSystem(this.AccessCode));
            this._container.Instance(api);
            
            this._budget = await api.GetBudgetAsync(this.BudgetName);
            this._device = await this._budget.RegisterDevice(Windows.Networking.Proximity.PeerFinder.DisplayName);

            this.ViewModel.Actions.Add(this._addTransactionItem);

            this.ViewModel.SecondaryActions.Add(this._logoutItem);
        }

        public override void Leave()
        {
            this._container.UnregisterHandler(typeof(YnabApi.YnabApi), null);

            this.ViewModel.Actions.Remove(this._addTransactionItem);
            this.ViewModel.SecondaryActions.Remove(this._logoutItem);
        }

        private void AddTransaction()
        {
            this._navigationService
                .For<AddTransactionViewModel>()
                .WithParam(f => f.BudgetName, this._budget.BudgetName)
                .WithParam(f => f.DeviceGuid, this._device.DeviceGuid)
                .Navigate();
        }

        private void Logout()
        {
            this.ViewModel.CurrentState = IoC.Get<LoggedOutShellState>();
        }
    }
}