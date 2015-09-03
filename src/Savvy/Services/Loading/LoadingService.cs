using System;
using Windows.UI.Xaml.Controls;
using Savvy.Common;
using Savvy.Controls;

namespace Savvy.Services.Loading
{
    public class LoadingService : ILoadingService
    {
        private readonly LoadingOverlay _overlay;

        public LoadingService(LoadingOverlay overlay)
        {
            this._overlay = overlay;
        }

        public IDisposable Show(string message)
        {
            this._overlay.Message = message;
            this._overlay.IsActive = true;

            return new DisposableAction(() =>
            {
                this._overlay.IsActive = false;
            });
        }
    }
}