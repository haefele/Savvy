using Caliburn.Micro;
using YnabApi;
using YnabApi.Dropbox;

namespace Savvy.Extensions
{
    public static class WinRTContainerExtensions
    {
        public static YnabApi.YnabApi RegisterYnabApi(this WinRTContainer container, string accessCode)
        {
            var settings = new YnabApiSettings(new DropboxFileSystem(accessCode));
            var api = new YnabApi.YnabApi(settings);

            container.Instance(api);

            return api;
        }

        public static void UnregisterYnabApi(this WinRTContainer container)
        {
            container.UnregisterHandler(typeof(YnabApi.YnabApi), null);
        }
    }
}