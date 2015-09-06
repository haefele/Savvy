using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Caliburn.Micro;
using Savvy.Services.DropboxAuthentication;
using Savvy.Services.Loading;
using Savvy.Services.Settings;
using Savvy.Views.AddTransaction;
using Savvy.Views.Shell;
using Savvy.Views.Shell.States;
using Savvy.Views.Welcome;

namespace Savvy
{
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
            this._container
                .Singleton<ShellViewModel>()
                .PerRequest<WelcomeViewModel>()
                .PerRequest<AddTransactionViewModel>();

            //ShellStates
            this._container
                .PerRequest<LoggedOutShellState>()
                .PerRequest<LoggedInShellState>()
                .PerRequest<OpenBudgetShellState>();

            //Services
            this._container
                .Singleton<ISettings, InMemorySettings>()
                .Singleton<IDropboxAuthenticationService, DropboxAuthenticationService>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return this._container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return this._container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            this._container.BuildUp(instance);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
                return;

            this.Initialize();

            var view = new ShellView();
            this._container.RegisterNavigationService(view.ContentFrame);
            this._container.Instance((ILoadingService)new LoadingService(view.LoadingOverlay));

            var viewModel = IoC.Get<ShellViewModel>();
            ViewModelBinder.Bind(viewModel, view, null);

            ScreenExtensions.TryActivate(viewModel);

            Window.Current.Content = view;
            Window.Current.Activate();
        }
    }
}
