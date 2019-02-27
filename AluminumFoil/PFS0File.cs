using System.ComponentModel;

namespace AluminumFoil.NSP
{
    // TODO Can this be watched for changes without inheriting from ReactiveUI
    // and calling Notify events? It would be nice to have the PFS0 file not
    // using reactiveui and have the viewmodel take care of everything.
    public class PFS0File : INotifyPropertyChanged
    {
        public string Name { get; set; }    // Name of content eg 123456789.nca
        public ulong Offset { get; set; }   // Start of file in nsp counting from byte-0x0
        public ulong Size { get; set; }     // Size of the file in bytes
        private ulong _Transferred;
        public ulong Transferred
        {
            get => _Transferred;
            set
            {
                _Transferred = value;
                NotifyPropertyChanged("Transferred");
            }
        }

        public string HumanSize { get; set; }
        public bool Finished { get; set; }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
