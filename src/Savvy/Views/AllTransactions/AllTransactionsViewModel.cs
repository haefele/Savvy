using System.ComponentModel.DataAnnotations;
using System.Linq;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Services.SessionState;
using YnabApi;
using YnabApi.Items;

namespace Savvy.Views.AllTransactions
{
    public class AllTransactionsViewModel : Screen
    {
        private readonly YnabApi.YnabApi _ynabApi;
        private readonly ISessionStateService _sessionStateService;

        private Budget _budget;
        private RegisteredDevice _device;
        
        public BindableCollection<TransactionsByMonth> Transactions { get; }

        public AllTransactionsViewModel(YnabApi.YnabApi ynabApi, ISessionStateService sessionStateService)
        {
            this._ynabApi = ynabApi;
            this._sessionStateService = sessionStateService;

            this.Transactions = new BindableCollection<TransactionsByMonth>();
        }

        protected override async void OnInitialize()
        {
            this._budget = await this._ynabApi.GetBudgetAsync(this._sessionStateService.BudgetName);
            this._device = await this._budget.GetRegisteredDevice(this._sessionStateService.DeviceGuid);
        }

        protected override async void OnActivate()
        {
            var allTransactions = await this._device.GetTransactionsAsync();

            this.Transactions.Clear();
            this.Transactions.AddRange(TransactionsByMonth.Create(allTransactions));
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }
    }
}