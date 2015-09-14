using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using YnabApi.Items;

namespace Savvy.Views.AllTransactions
{
    public class TransactionsByMonth
    {
        public DateTime YearAndMonth { get; }    
        public BindableCollection<Transaction> Transactions { get; }

        public TransactionsByMonth(DateTime yearAndMonth, BindableCollection<Transaction> transactions)
        {
            this.YearAndMonth = yearAndMonth;
            this.Transactions = transactions;
        }

        public static IEnumerable<TransactionsByMonth> Create(IEnumerable<Transaction> transactions)
        {
            var result =
                from transaction in transactions
                orderby transaction.Date descending
                group transaction by new {transaction.Date.Year, transaction.Date.Month} into g
                select new TransactionsByMonth(new DateTime(g.Key.Year, g.Key.Month, 1), new BindableCollection<Transaction>(g));

            return result;
        } 
    }
}