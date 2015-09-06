using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YnabApi.Items;
using Savvy.Extensions;

namespace Savvy.Views.AddTransaction
{
    public sealed partial class AddTransactionView : Page
    {
        public AddTransactionView()
        {
            this.InitializeComponent();
        }

        public AddTransactionViewModel ViewModel => this.DataContext as AddTransactionViewModel;

        private void PayeesAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            sender.ItemsSource = this.ViewModel.Payees
                .Where(f => f.Name.Contains(sender.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
