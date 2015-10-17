using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Windows.Devices.Enumeration;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Services.Loading;
using Savvy.Services.Navigation;
using Savvy.Views.BudgetOverview;
using YnabApi;
using YnabApi.DeviceActions;
using YnabApi.Extensions;
using YnabApi.Items;

namespace Savvy.Views.AddTransaction
{
    public class AddTransactionViewModel : Screen
    {
        private readonly YnabApi.YnabApi _api;
        private readonly ISavvyNavigationService _navigationService;
        private readonly ILoadingService _loadingService;
        private readonly IEventAggregator _eventAggregator;

        private Budget _budget;
        private RegisteredDevice _device;

        private Account _selectedAccount;
        private CategoryViewModel _selectedCategory;
        private string _selectedPayeeName;
        private bool _isOutflow;
        private string _amount;
        private string _memo;
        private bool _cleared;

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
        public BindableCollection<CategoryViewModel> Categories { get; }
        public CategoryViewModel SelectedCategory
        {
            get { return this._selectedCategory; }
            set { this.SetProperty(ref this._selectedCategory, value); }
        }
        public BindableCollection<Payee> Payees { get; }
        public string SelectedPayeeName
        {
            get { return this._selectedPayeeName; }
            set { this.SetProperty(ref this._selectedPayeeName, value); }
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
        public bool Cleared
        {
            get { return this._cleared; }
            set { this.SetProperty(ref this._cleared, value); }
        }

        public AddTransactionViewModel(YnabApi.YnabApi api, ISavvyNavigationService navigationService, ILoadingService loadingService, IEventAggregator eventAggregator)
        {
            this._api = api;
            this._navigationService = navigationService;
            this._loadingService = loadingService;
            this._eventAggregator = eventAggregator;

            this.Payees = new BindableCollection<Payee>();
            this.Accounts = new BindableCollection<Account>();
            this.Categories = new BindableCollection<CategoryViewModel>();

            this.IsOutflow = true;
        }

        protected override async void OnInitialize()
        {
            using (this._loadingService.Show("Loading data..."))
            {
                this._budget = await this._api.GetBudgetAsync(this.BudgetName);
                this._device = await this._budget.GetRegisteredDevice(this.DeviceGuid);
                
                var payees = await this._device.GetPayeesAsync();
                this.Payees.AddRange(payees.OnlyActive().WithoutTransfers());

                var accounts = await this._device.GetAccountsAsync();
                this.Accounts.AddRange(accounts);

                var masterCategories = await this._device.GetCategoriesAsync();
                this.Categories.AddRange(masterCategories.OnlyActive().SelectMany(f => f.SubCategories.OnlyActive().Select(d => new CategoryViewModel(f, d))));
            }
        }

        public async void Save()
        {
            decimal? parsedAmount = this.Amount.ToDecimal();

            if (this.SelectedAccount == null ||
                this.SelectedCategory == null ||
                this.SelectedPayeeName == null ||
                parsedAmount == null)
                return;
            
            using (this._loadingService.Show("Saving transaction..."))
            {
                List<IDeviceAction> actionsToExecute = new List<IDeviceAction>();

                IHavePayeeId payee = this.Payees.FirstOrDefault(f => string.Equals(f.Name, this.SelectedPayeeName, StringComparison.OrdinalIgnoreCase));

                if (payee == null)
                {
                    payee = new CreatePayeeDeviceAction {Name = this.SelectedPayeeName};
                    actionsToExecute.Add((IDeviceAction)payee);
                }

                var action = new CreateTransactionDeviceAction
                {
                    Account = this.SelectedAccount,
                    Category = this.SelectedCategory.Category,
                    Payee = payee,
                    Amount = this.IsOutflow ? -1 * parsedAmount.Value : parsedAmount.Value,
                    Memo = this.Memo,
                    Cleared = this.Cleared
                };
                actionsToExecute.Add(action);

                await this._device.ExecuteActions(actionsToExecute.ToArray());
                this._device.ClearCache();

                this._navigationService
                    .For<BudgetOverviewViewModel>()
                    .WithParam(f => f.BudgetName, this.BudgetName)
                    .WithParam(f => f.DeviceGuid, this.DeviceGuid)
                    .Navigate();
            }
        }
    }
}