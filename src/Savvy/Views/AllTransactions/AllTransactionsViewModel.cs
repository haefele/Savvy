using System.ComponentModel.DataAnnotations;
using System.Linq;
using Caliburn.Micro;
using Savvy.Extensions;
using YnabApi;
using YnabApi.Items;

namespace Savvy.Views.AllTransactions
{
    public class AllTransactionsViewModel : Screen
    {
        private readonly YnabApi.YnabApi _ynabApi;

        private Budget _budget;
        private RegisteredDevice _device;
        
        [Required]
        public string BudgetName { get; set; }
        [Required]
        public string DeviceGuid { get; set; }

        public BindableCollection<TransactionsByMonth> Transactions { get; }

        public AllTransactionsViewModel(YnabApi.YnabApi ynabApi)
        {
            this._ynabApi = ynabApi;

            this.Transactions = new BindableCollection<TransactionsByMonth>();
        }

        protected override async void OnInitialize()
        {
            this._budget = await this._ynabApi.GetBudgetAsync(this.BudgetName);
            this._device = await this._budget.GetRegisteredDevice(this.DeviceGuid);
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