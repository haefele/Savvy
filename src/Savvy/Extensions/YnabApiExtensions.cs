using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YnabApi;
using YnabApi.Extensions;
using YnabApi.Items;

namespace Savvy.Extensions
{
    public static class YnabApiExtensions
    {
        public static async Task<Budget> GetBudgetAsync(this YnabApi.YnabApi api, string budgetName)
        {
            var budgets = await api.GetBudgetsAsync();
            return budgets.FirstOrDefault(f => f.BudgetName == budgetName);
        }
    }

    public static class BudgetExtensions
    {
        public static async Task<RegisteredDevice> GetRegisteredDevice(this Budget budget, string deviceGuid)
        {
            var devices = await budget.GetRegisteredDevicesAsync();
            return devices.FirstOrDefault(f => f.DeviceGuid == deviceGuid);
        }

        public static async Task<RegisteredDevice> GetFullKnowledgeDevice(this Budget budget)
        {
            var devices = await budget.GetRegisteredDevicesAsync();
            return devices.FirstOrDefault(f => f.HasFullKnowledge);
        }
    }

    public static class RegisteredDeviceExtensions
    {
        public static async Task<IList<Category>> GetActiveCategoriesAsync(this RegisteredDevice registeredDevice)
        {
            var categories = await registeredDevice.GetCategoriesAsync();
            return categories.OnlyActive().ToList();
        }
    }
}