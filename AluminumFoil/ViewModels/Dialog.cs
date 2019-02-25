using ReactiveUI;

namespace AluminumFoil.ViewModels
{
    public class Dialog : ReactiveObject
    // Base vm for dialogs to set title and message

    {
        public Dialog(string title, string message)
        {
            Title = title;
            Message = message;
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set
            {
                this.RaiseAndSetIfChanged(ref _Title, value);
            }
        }

        private string _Message;
        public string Message
        {
            get => _Message;
            set
            {
                this.RaiseAndSetIfChanged(ref _Message, value);
            }
        }
    }
}
