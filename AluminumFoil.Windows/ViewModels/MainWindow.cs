using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using ReactiveUI;
using System.Reactive;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace AluminumFoil.Windows.ViewModels
{
    public class MainWindow : ReactiveObject
    {
        public MainWindow()
        {
            InstallNSP = ReactiveCommand.Create( () => { this.InstallationSubscription = StartInstallation.Execute().Subscribe(); },
                this.WhenAnyValue(vm => vm.OpenedNSP.Count, vm => vm.AllowActions, (a, b) => a != 0 && b));

            StartInstallation = ReactiveCommand.Create(_StartInstallation);
            OpenFileDialog = ReactiveCommand.Create(_OpenFileDialog, this.WhenAnyValue(vm => vm.AllowActions));
            RemoveNSP = ReactiveCommand.Create<string, bool>((f) => _RemoveNSP(f), this.WhenAnyValue(vm => vm.AllowActions));
            AskCancelInstall = ReactiveCommand.Create(_AskCancelInstall);
        }

        #region properties
        private IDisposable InstallationSubscription { get; set; }

        private string _InstallationTarget = "GoldLeaf";
        public string InstallationTarget
        {
            get => _InstallationTarget;
            set
            {
                if (value == _InstallationTarget) return;

                Console.WriteLine("Changing InstallationTarget to:" + value);

                if (value == "GoldLeaf")
                {
                    Console.WriteLine("InstallationTarget is GoldLeaf, removing all but first OpenedNSP");
                    if (OpenedNSP.Count > 1)
                    {
                        NSP first = OpenedNSP[0];
                        OpenedNSP.Clear();
                        OpenedNSP.Add(first);
                    }
                }
                this.RaiseAndSetIfChanged(ref _InstallationTarget, value);
            }
        }

        private string _StatusBar = "Idle";
        public string StatusBar
        {
            get => _StatusBar;
            set
            {
                this.RaiseAndSetIfChanged(ref _StatusBar, value);
            }
        }

        private bool _AllowActions = true;
        public bool AllowActions
        // Disables all buttons and prevents closing main window;
        {
            get => _AllowActions;
            set
            {
                this.RaiseAndSetIfChanged(ref _AllowActions, value);
            }
        }

        private string _StatusBarIcon = "/AluminumFoil.Windows;component/Assets/Images/idle_24.png";
        public string StatusBarIcon
        {
            get => _StatusBarIcon;
            set
            {
                this.RaiseAndSetIfChanged(ref _StatusBarIcon, string.Format("/AluminumFoil.Windows;component/Assets/Images/{0}_24.png", value));
            }
        }

        private ObservableCollection<NSP> _OpenedNSP = new ObservableCollection<NSP>();
        public ObservableCollection<NSP> OpenedNSP
        {
            get => _OpenedNSP;
            set
            {
                this.RaiseAndSetIfChanged(ref _OpenedNSP, value);
            }
        }
        #endregion

        #region methods
        public void OpenNSPs(string[] fnames)
        {
            try
            {
                if (InstallationTarget == "GoldLeaf")
                {
                    Console.WriteLine("Clearing OpenedNSP list");
                    OpenedNSP.Clear();
                    fnames = new string[] { fnames[0] };
                }
                foreach (string fname in fnames)
                {
                    Console.WriteLine("Opening " + fname);
                    if (OpenedNSP.Any(nsp => nsp.FilePath == fname)){
                        Console.WriteLine(fname + "already opened, skipping");
                        continue;
                    }
                    OpenedNSP.Add(new NSP(fname));

                }
            }
            catch (Exception e)
            {
                OpenedNSP.Clear();
                var errDlg = new Dialogs.Error("Corrupt NSP", e.Message, e.Source);
                errDlg.ShowDialog();
                return;
            }
            finally
            {
                StatusBar = "Idle";
                StatusBarIcon = "idle";
            }
        }
        #endregion

        #region button commands
        public ReactiveCommand<Unit, Unit> AskCancelInstall { get; set; }
        private void _AskCancelInstall()
        {
            var dlg = new Dialogs.CancelInstall();
            dlg.ShowDialog();
            if ((bool)dlg.DialogResult)
            {
                InstallationSubscription.Dispose();
                InstallationSubscription = null;
            }
        }

        public ReactiveCommand<Unit, Unit> OpenFileDialog { get; set; }
        private void _OpenFileDialog()
        {
            Console.WriteLine("Opening NSP");
            var dlg = new OpenFileDialog();
            if (InstallationTarget == "TinFoil")
            {
                dlg.Multiselect = true;
            }
            dlg.Filter = "Switch eShop Files (*.nsp)|*.nsp";

            if(dlg.ShowDialog() == false){
                return;
            }

            OpenNSPs(dlg.FileNames);
        }

        public ReactiveCommand<string, bool> RemoveNSP { get; set; }
        // TODO return type Unit throws an error -- can this be fixed?
        // There is nothing wrong with returning bool, its just kind of pointless
        private bool _RemoveNSP(string fileName)
        {
            for (var i = 0; i < OpenedNSP.Count; i++)
            {
                if (OpenedNSP[i].FilePath == fileName)
                {
                    OpenedNSP.RemoveAt(i);
                    break;
                }
            }
            return true;
        }


        public ReactiveCommand<Unit, Unit> InstallNSP { get; set; }

        private ReactiveCommand<Unit, Unit> StartInstallation { get; set; }
        private async void _StartInstallation()
        {
            AllowActions = false;
            try
            {
                await Task.Run(() =>
                {
                    Func<ObservableCollection<NSP>, IEnumerable<(string, string)>> installer = null;
                    switch (InstallationTarget)
                    {
                        case "GoldLeaf":
                            installer = Windows.App.GoldLeaf.InstallNSP;
                            break;
                        case "TinFoil":
                            installer = Windows.App.TinFoil.InstallNSP;
                            break;
                    }

                    if (installer == null)
                    {
                        return;
                    };

                    foreach ((string text, string icon) in installer(OpenedNSP))
                    {
                        StatusBar = text;
                        StatusBarIcon = icon;
                    }
                });
                var finDlg = new Dialogs.Success("Installation Finished", "Neat");
                finDlg.ShowDialog();
            }
            catch (Exception e)
            {
                var errDlg = new Dialogs.Error("Installation Failed", e.Message);
                errDlg.ShowDialog();
            }
            finally
            {
                StatusBar = "Idle";
                StatusBarIcon = "idle";
                AllowActions = true;
            };
            return;
        }
        #endregion
    }
}