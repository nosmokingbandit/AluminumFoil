using System;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using Microsoft.Win32;
using System.Collections.Generic;

namespace AluminumFoil.ViewModels
{
    public class MainWindow : ReactiveObject
    {
        public MainWindow()
        {
            InstallNSP = ReactiveCommand.Create(() => _InstallNSP(), this.WhenAnyValue(vm => vm.OpenedNSP, vm => vm.AllowActions, (a, b) => a != null && b));
            OpenNSP = ReactiveCommand.Create(() => _OpenNSP(), this.WhenAnyValue(vm => vm.OpenNSPButtonEnable, vm => vm.AllowActions, (a, b) => a && b ));
            CloseNSP = ReactiveCommand.Create(() => _CloseNSP(), this.WhenAnyValue(vm => vm.OpenNSPButtonEnable, vm => vm.AllowActions, (a, b) => a && b));
        }

        private string _InstallationTarget = "GoldLeaf";
        public string InstallationTarget
        {
            get => _InstallationTarget;
            set
            {
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

        private bool _OpenNSPButtonEnable = true;
        public bool OpenNSPButtonEnable
        {
            get => _OpenNSPButtonEnable;
            set
            {
                this.RaiseAndSetIfChanged(ref _OpenNSPButtonEnable, value);
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
        
        private AluminumFoil.NSP.PFS0 _OpenedNSP;
        public AluminumFoil.NSP.PFS0 OpenedNSP
        {
            get => _OpenedNSP;
            set
            {
                this.RaiseAndSetIfChanged(ref _OpenedNSP, value);
            }
        }

        //
        //======== Button Commands
        //
        public ReactiveCommand<Unit, Unit> OpenNSP { get; set; }
        private void _OpenNSP()
        {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "Switch eShop Files (*.nsp)|*.nsp";

            if(dlg.ShowDialog() == false){
                return;
            }

            try
            {
                OpenedNSP = new AluminumFoil.NSP.PFS0(dlg.FileName);
            } 
            catch (Exception e)
            {
                OpenedNSP = null;
                var errDlg = new Dialogs.Error("Corrupt NSP", e.Message);
                errDlg.ShowDialog();
                return;
            }
            finally
            {
                StatusBar = "Idle";
                StatusBarIcon = "idle";
            }
        }

        public ReactiveCommand<Unit, Unit> CloseNSP { get; set; }
        private void _CloseNSP()
        {
            OpenedNSP = null;
        }

        public ReactiveCommand<Unit, Unit> InstallNSP { get; set; }
        private async void _InstallNSP()
        {
            AllowActions = false;
            try
            {
                await Task.Run(() =>
                {
                    Func<NSP.PFS0, IEnumerable<Tuple<string, string>>> installer = null;
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

                    foreach (Tuple<string, string> statusUpdate in installer(OpenedNSP))
                    {
                        StatusBar = statusUpdate.Item1;
                        StatusBarIcon = statusUpdate.Item2;
                    }
                });
                var finDlg = new Dialogs.Success("Installation Finished", OpenedNSP.BaseName + " Installation Finished");
                finDlg.ShowDialog();
            }
            catch (Exception e)
            {
                var errDlg = new Dialogs.Error("Installation Failed", e.Message);
                errDlg.ShowDialog();
            }
            finally
            {
                AllowActions = true;
            };
            return;
        }
    }
}