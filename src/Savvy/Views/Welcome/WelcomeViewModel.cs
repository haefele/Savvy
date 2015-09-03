using System;
using System.Globalization;
using System.Threading.Tasks;
using Caliburn.Micro;
using Savvy.Extensions;
using Savvy.Services.Loading;

namespace Savvy.Views.Welcome
{
    public class WelcomeViewModel : Screen
    {
        private readonly ILoadingService _loadingService;

        public WelcomeViewModel(ILoadingService loadingService)
        {
            this._loadingService = loadingService;
        }

        public async Task Show()
        {
            using (this._loadingService.Show("hallo welt"))
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}