using System;
using System.Threading.Tasks;
using Windows.Storage;
using Caliburn.Micro;
using Savvy.Services.SessionState;
using Savvy.YnabApiFileSystem;
using YnabApi;

namespace Savvy.Extensions
{
    public static class WinRTContainerExtensions
    {
        public static async Task<YnabApi.YnabApi> RegisterYnabApiAsync(this WinRTContainer container)
        {
            var sessionStateService = container.GetInstance<ISessionStateService>();

            var rootFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Dropbox", CreationCollisionOption.OpenIfExists);
            
            var fileSystem = new HybridFileSystem(rootFolder, sessionStateService);
            var api = new YnabApi.YnabApi(new YnabApiSettings(fileSystem, "Savvy", "Windows 10 Universal"));

            container.Instance(fileSystem);
            container.Instance(api);

            return api;
        }

        public static void UnregisterYnabApi(this WinRTContainer container)
        {
            container.UnregisterHandler(typeof(HybridFileSystem), null);
            container.UnregisterHandler(typeof(YnabApi.YnabApi), null);
        }
    }
}