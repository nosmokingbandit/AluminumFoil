using System;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using Avalonia.Controls;
using System.Collections.Generic;

namespace AluminumFoil.Mac.ViewModels
{
    public class MainWindow : ReactiveObject
    {
        public MainWindow()
        {
            InstallNSP = ReactiveCommand.Create(() => _InstallNSP(), this.WhenAnyValue(vm => vm.OpenedNSP, vm => vm.AllowActions, (a, b) => a != null && b));
            OpenNSP = ReactiveCommand.Create(() => _OpenNSP(), this.WhenAnyValue(vm => vm.OpenNSPButtonEnable, vm => vm.AllowActions, (a, b) => a && b));
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

        private string _StatusBarIcon = "resm:AluminumFoil.Mac.Assets.Images.idle_24.png";
        public string StatusBarIcon
        {
            get => _StatusBarIcon;
            set
            {
                this.RaiseAndSetIfChanged(ref _StatusBarIcon, string.Format("resm:AluminumFoil.Mac.Assets.Images.{0}_24.png", value));
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
        private async void _OpenNSP()
        {
            var dlg = new OpenFileDialog();
            dlg.AllowMultiple = false;
            dlg.Filters.Add(new FileDialogFilter { Name = "Switch eShop Files (*.nsp)", Extensions = new List<string> { "nsp" } });
            string[] selectedFiles = await dlg.ShowAsync(App.Current.MainWindow);

            if (selectedFiles.Length == 0)
            {
                return;
            }

            try
            {
                OpenedNSP = new AluminumFoil.NSP.PFS0(System.Uri.UnescapeDataString(selectedFiles[0]));
            }
            catch (Exception e)
            {
                var errDlg = new Dialogs.Error("Corrupt NSP", e.Message);
                await errDlg.ShowDialog(App.Current.MainWindow);
                // OpenedNSP = null;
                return;
            }
            finally
            {
                StatusBar = "Idle";
                StatusBarIcon = "idle";
            }
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
                            installer = Mac.App.GoldLeaf.InstallNSP;
                            break;
                        case "TinFoil":
                            installer = Mac.App.TinFoil.InstallNSP;
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
                await finDlg.ShowDialog(App.Current.MainWindow);
            }
            catch (Exception e)
            {
                var errDlg = new Dialogs.Error("Installation Failed", e.Message);
                await errDlg.ShowDialog(App.Current.MainWindow);
            }
            finally
            {
                AllowActions = true;
            };
            return;
        }
    }
}