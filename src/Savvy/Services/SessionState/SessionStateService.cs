using System.Threading.Tasks;
using Windows.Storage;

namespace Savvy.Services.SessionState
{
    public class SessionStateService : ISessionStateService
    {
        public string DropboxUserId { get; set; }
        public string DropboxAccessCode { get; set; }
        public string BudgetName { get; set; }

        public Task SaveStateAsync()
        {
            var container = this.GetSettingsContainer();

            container.Values[nameof(this.DropboxUserId)] = this.DropboxUserId;
            container.Values[nameof(this.DropboxAccessCode)] = this.DropboxAccessCode;
            container.Values[nameof(this.BudgetName)] = this.BudgetName;

            return Task.CompletedTask;
        }

        public Task RestoreStateAsync()
        {
            var container = this.GetSettingsContainer();

            if (container.Values.ContainsKey(nameof(this.DropboxUserId)))
                this.DropboxUserId = (string)container.Values[nameof(this.DropboxUserId)];

            if (container.Values.ContainsKey(nameof(this.DropboxAccessCode)))
                this.DropboxAccessCode = (string)container.Values[nameof(this.DropboxAccessCode)];

            if (container.Values.ContainsKey(nameof(this.BudgetName)))
                this.BudgetName = (string)container.Values[nameof(this.BudgetName)];

            return Task.CompletedTask;
        }

        private ApplicationDataContainer GetSettingsContainer()
        {
            return ApplicationData.Current.LocalSettings.CreateContainer("Savvy.SessionState", ApplicationDataCreateDisposition.Always);
        }
    }
}