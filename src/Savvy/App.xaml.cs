﻿using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Savvy.Services.DropboxAuthentication;
using Savvy.Services.Loading;
using Savvy.Services.Navigation;
using Savvy.Services.SessionState;
using Savvy.Services.Settings;
using Savvy.States;
using Savvy.Views.AddTransaction;
using Savvy.Views.AllTransactions;
using Savvy.Views.Shell;
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
                .PerRequest<AddTransactionViewModel>()
                .PerRequest<AllTransactionsViewModel>();

            //ShellStates
            this._container
                .PerRequest<LoggedOutApplicationState>()
                .PerRequest<LoggedInApplicationState>()
                .PerRequest<OpenBudgetApplicationState>();

            //Services
            this._container
                .Singleton<ISettings, InMemorySettings>()
                .Singleton<IDropboxAuthenticationService, DropboxAuthenticationService>()
                .Singleton<ISessionStateService, SessionStateService>();
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

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState == ApplicationExecutionState.Running ||
                args.PreviousExecutionState == ApplicationExecutionState.Suspended)
                return;

            this.Initialize();

            var stateService = this._container.GetInstance<ISessionStateService>();
            await stateService.RestoreStateAsync();

            var view = new ShellView();
            this._container.Instance((ISavvyNavigationService)new SavvyNavigationService(view.ContentFrame));
            this._container.Instance((ILoadingService)new LoadingService(view.LoadingOverlay));
            
            var viewModel = IoC.Get<ShellViewModel>();
            ViewModelBinder.Bind(viewModel, view, null);

            this.RestoreApplicationState(stateService, viewModel);

            ScreenExtensions.TryActivate(viewModel);

            Window.Current.Content = view;
            Window.Current.Activate();
        }

        protected override async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            var stateService = this._container.GetInstance<ISessionStateService>();
            await stateService.SaveStateAsync();

            deferral.Complete();
        }

        protected override async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }
        
        private void RestoreApplicationState(ISessionStateService sessionStateService, ShellViewModel shell)
        {
            if (sessionStateService.DropboxAccessCode != null &&
                sessionStateService.DropboxUserId != null)
            {
                if (sessionStateService.BudgetName != null)
                {
                    var newShellState = IoC.Get<OpenBudgetApplicationState>();
                    newShellState.BudgetName = sessionStateService.BudgetName;

                    shell.CurrentState = newShellState;
                }
                else
                {
                    shell.CurrentState = IoC.Get<LoggedInApplicationState>();
                }
            }
            else
            {
                shell.CurrentState = IoC.Get<LoggedOutApplicationState>();
            }
        }
    }
}
