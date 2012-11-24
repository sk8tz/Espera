﻿using Caliburn.Micro;
using Espera.Core.Management;
using Espera.Core.Settings;
using Espera.View.Properties;
using Espera.View.ViewModels;
using Ninject;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Espera.View
{
    internal class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        private static readonly string DirectoryPath;
        private static readonly string FilePath;
        private readonly WindowManager windowManager;
        private IKernel kernel;

        static AppBootstrapper()
        {
            DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Espera\");
            FilePath = Path.Combine(DirectoryPath, "Library.xml");
        }

        public AppBootstrapper()
        {
            this.windowManager = new WindowManager();

            // ReactiveUI binding convention
            Func<string, string> firstToLowerBehaviour = x =>
            {
                char[] arr = x.ToCharArray();
                arr[0] = char.ToLower(arr[0]);
                return new String(arr);
            };

            RxApp.GetFieldNameForPropertyNameFunc = firstToLowerBehaviour;
        }

        protected override void Configure()
        {
            this.kernel = new StandardKernel();

            this.kernel.Bind<IRemovableDriveWatcher>().To<RemovableDriveWatcher>();
            this.kernel.Bind<ILibraryReader>().To<LibraryFileReader>().WithConstructorArgument("sourcePath", FilePath);
            this.kernel.Bind<ILibraryWriter>().To<LibraryFileWriter>().WithConstructorArgument("targetPath", FilePath);
            this.kernel.Bind<ILibrarySettings>().To<LibrarySettingsWrapper>();
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return this.kernel.GetAll(serviceType);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            return this.kernel.Get(serviceType, key);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            this.kernel.Dispose();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            base.OnStartup(sender, e);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            this.Application.MainWindow.Hide();

            this.windowManager.ShowDialog(new CrashViewModel(e.Exception));

            e.Handled = true;

            Application.Current.Shutdown();
        }
    }
}