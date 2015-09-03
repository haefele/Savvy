using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Windows.Devices.Enumeration;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Services.Loading;
using Savvy.Views.BudgetOverview;
using YnabApi;
using YnabApi.DeviceActions;
using YnabApi.Items;

namespace Savvy.Views.AddTransaction
{
    public class AddTransactionViewModel : Screen
    {
        private readonly YnabApi.YnabApi _api;
        private readonly INavigationService _navigationService;
        private readonly ILoadingService _loadingService;

        private Budget _budget;
        private RegisteredDevice _device;

        private Account _selectedAccount;
        private Category _selectedCategory;
        private Payee _selectedPayee;
        private bool _isOutflow;
        private string _amount;
        private string _memo;

        [Required]
        public string BudgetName { get; set; }
        [Required]
        public string DeviceGuid { get; set; }

        public BindableCollection<Account> Accounts { get; }
        public Account SelectedAccount
        {
            get { return this._selectedAccount; }
            set { this.SetProperty(ref this._selectedAccount, value); }
        }
        public BindableCollection<Category> Categories { get; }
        public Category SelectedCategory
        {
            get { return this._selectedCategory; }
            set { this.SetProperty(ref this._selectedCategory, value); }
        }
        public BindableCollection<Payee> Payees { get; }
        public Payee SelectedPayee
        {
            get { return this._selectedPayee; }
            set { this.SetProperty(ref this._selectedPayee, value); }
        }
        public bool IsOutflow
        {
            get { return this._isOutflow; }
            set { this.SetProperty(ref this._isOutflow, value); }
        }
        public string Amount
        {
            get { return this._amount; }
            set { this.SetProperty(ref this._amount, value); }
        }
        public string Memo
        {
            get { return this._memo; }
            set { this.SetProperty(ref this._memo, value); }
        }
        
        public AddTransactionViewModel(YnabApi.YnabApi api, INavigationService navigationService, ILoadingService loadingService)
        {
            this._api = api;
            this._navigationService = navigationService;
            this._loadingService = loadingService;

            this.Payees = new BindableCollection<Payee>();
            this.Accounts = new BindableCollection<Account>();
            this.Categories = new BindableCollection<Category>();

            this.IsOutflow = true;
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

        public async void Save()
        {
            decimal? parsedAmount = this.Amount.ToDecimal();

            if (this.SelectedAccount == null ||
                this.SelectedCategory == null ||
                this.SelectedPayee == null ||
                parsedAmount == null)
                return;

            using (this._loadingService.Show("Saving transaction..."))
            { 
                var action = new CreateTransactionDeviceAction
                {
                    Account = this.SelectedAccount,
                    Category = this.SelectedCategory,
                    Payee = this.SelectedPayee,
                    Amount = this.IsOutflow ? -1 * parsedAmount.Value : parsedAmount.Value,
                    Memo = this.Memo
                };

                await this._device.ExecuteActions(action);
            }

            this._navigationService
                .For<BudgetOverviewViewModel>()
                .WithParam(f => f.BudgetName, this.BudgetName)
                .WithParam(f => f.DeviceGuid, this.DeviceGuid)
                .Navigate();
        }
    }
}