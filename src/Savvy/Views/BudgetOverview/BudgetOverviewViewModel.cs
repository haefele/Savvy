using System.ComponentModel.DataAnnotations;
using Caliburn.Micro;

namespace Savvy.Views.BudgetOverview
{
    public class BudgetOverviewViewModel : Screen
    {
        [Required]
        public string BudgetName { get; set; }
        [Required]
        public string DeviceGuid { get; set; }
    }
}