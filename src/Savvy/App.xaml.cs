using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Caliburn.Micro;
using Savvy.Services.DropboxAuthentication;
using Savvy.Services.Settings;
using Savvy.Views.Shell;
using Savvy.Views.Shell.States;
using Savvy.Views.Welcome;

namespace Savvy
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : CaliburnApplication
    {
        private WinRTContainer _container;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void Configure()
        {
            this.ConfigureLogging();
            this.ConfigureContainer();
        }
        
        private void ConfigureLogging()
        {
        }

        private void ConfigureContainer()
        {
            //Common
            this._container = new WinRTContainer();
            this._container.RegisterWinRTServices();

            //ViewModels
            this._container.PerRequest<ShellViewModel>();
            this._container.PerRequest<WelcomeViewModel>();

            //ShellStates
            this._container
                .PerRequest<LoggedOutShellState>()
                .PerRequest<LoggedInShellState>();

            //Services
            this._container.Singleton<ISettings, InMemorySettings>();
            this._container.Singleton<IDropboxAuthenticationService, DropboxAuthenticationService>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return this._container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return this._container.GetAllInstances(service);
        }
        
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            this.Initialize();

            var view = new ShellView();
            this._container.RegisterNavigationService(view.ContentFrame);

            var viewModel = IoC.Get<ShellViewModel>();
            ViewModelBinder.Bind(viewModel, view, null);

            ScreenExtensions.TryActivate(viewModel);

            Window.Current.Content = view;
            Window.Current.Activate();
        }
    }
}
