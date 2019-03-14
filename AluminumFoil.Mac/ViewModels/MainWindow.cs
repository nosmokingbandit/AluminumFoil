using System;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AluminumFoil.Mac.ViewModels
{
    public class MainWindow : ReactiveObject
    {
        public MainWindow()
        {
            InstallNSP = ReactiveCommand.Create(_InstallNSP, this.WhenAnyValue(vm => vm.OpenedNSP.Count, vm => vm.AllowActions, (a, b) => a != 0 && b));
            OpenFileDialog = ReactiveCommand.Create(_OpenFileDialog, this.WhenAnyValue(vm => vm.AllowActions));
            RemoveNSP = ReactiveCommand.Create<string, bool>((f) => _RemoveNSP(f), this.WhenAnyValue(vm => vm.AllowActions));
        }

        #region properties
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

        private string _StatusBarIcon = "resm:AluminumFoil.Mac.Assets.Images.idle_24.png";
        public string StatusBarIcon
        {
            get => _StatusBarIcon;
            set
            {
                this.RaiseAndSetIfChanged(ref _StatusBarIcon, string.Format("resm:AluminumFoil.Mac.Assets.Images.{0}_24.png", value));
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
                    fnames = new string[] { fnames[0] };
                    OpenedNSP.Clear();
                }

                foreach (string nameUri in fnames)
                {
                    string filename = System.Uri.UnescapeDataString(nameUri);
                    if (OpenedNSP.Any(nsp => nsp.FilePath == filename))
                    {
                        Console.WriteLine(filename + "already opened, skipping");
                        continue;
                    }
                    OpenedNSP.Add(new NSP(filename));

                }
            }
            catch (Exception e)
            {
                OpenedNSP.Clear();
                var errDlg = new Dialogs.Error("Corrupt NSP", e.Message, e.Source);
                errDlg.ShowDialog(App.Current.MainWindow);
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
        public ReactiveCommand<Unit, Unit> OpenFileDialog { get; set; }
        private async void _OpenFileDialog()
        {
            Console.WriteLine("Opening NSP");
            var dlg = new OpenFileDialog();
            dlg.AllowMultiple = (InstallationTarget == "TinFoil");

            dlg.Filters.Add(new FileDialogFilter { Name = "Switch eShop Files (*.nsp)", Extensions = new List<string> { "nsp" } });

            string[] selectedFiles = await dlg.ShowAsync(App.Current.MainWindow);

            if (selectedFiles.Length == 0)
            {
                Console.WriteLine("Dialog returned empty array.");
                return;
            }

            OpenNSPs(selectedFiles);
        }

        public ReactiveCommand<string, bool> RemoveNSP { get; set; }
        // TODO return type Unit throws an error -- can this be fixed?
        // There is nothing wrong with returning bool, its just kind of pointless
        private bool _RemoveNSP(string fileName)
        {
            Console.WriteLine(string.Format("Removing {0} from list", fileName ));
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
        private async void _InstallNSP()
        {
            AllowActions = false;
            try
            {
                await Task.Run(() =>
                {
                    Func<ObservableCollection<NSP>, IEnumerable<Tuple<string, string>>> installer = null;
                    switch (InstallationTarget)
                    {
                        case "GoldLeaf":
                            installer = App.GoldLeaf.InstallNSP;
                            break;
                        case "TinFoil":
                            installer = App.TinFoil.InstallNSP;
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
                var finDlg = new Dialogs.Success("Installation Finished", "");
                await finDlg.ShowDialog(App.Current.MainWindow);
            }
            catch (Exception e)
            {
                var errDlg = new Dialogs.Error("Installation Failed", e.Message);
                await errDlg.ShowDialog(App.Current.MainWindow);
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