using System.Threading.Tasks;

namespace Savvy.Services.SessionState
{
    public interface ISessionStateService
    {
        string DropboxUserId { get; set; }
        string DropboxAccessCode { get; set; }
        string BudgetName { get; set; }
        string DeviceGuid { get; set; }

        Task SaveStateAsync();
        Task RestoreStateAsync();
    }
}