using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Windows.Devices.Enumeration;
using Caliburn.Micro;
using Savvy.Extensions;
using YnabApi;
using YnabApi.Items;

namespace Savvy.Views.AddTransaction
{
    public class AddTransactionViewModel : Screen
    {
        private readonly YnabApi.YnabApi _api;

        private Budget _budget;
        private RegisteredDevice _device;

        [Required]
        public string BudgetName { get; set; }
        [Required]
        public string DeviceGuid { get; set; }

        public BindableCollection<Payee> Payees { get; }
        public BindableCollection<Account> Accounts { get; }
        public BindableCollection<Category> Categories { get; }

        public AddTransactionViewModel(YnabApi.YnabApi api)
        {
            this._api = api;

            this.Payees = new BindableCollection<Payee>();
            this.Accounts = new BindableCollection<Account>();
            this.Categories = new BindableCollection<Category>();
        }

        protected override async void OnInitialize()
        {
            this._budget = await this._api.GetBudgetAsync(this.BudgetName);
            this._device = await this._budget.GetRegisteredDevice(this.DeviceGuid);

            var fullKnowledgeDevice = await this._budget.GetFullKnowledgeDevice();

            this.Payees.AddRange(await fullKnowledgeDevice.GetPayeesAsync());
            this.Accounts.AddRange(await fullKnowledgeDevice.GetAccountsAsync());
            this.Categories.AddRange(await fullKnowledgeDevice.GetCategoriesAsync());
        }
    }
}