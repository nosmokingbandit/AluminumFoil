using ReactiveUI;

namespace AluminumFoil.ViewModels
{
    public class Dialog : ReactiveObject
    // Base vm for dialogs to set title and message

    {
        public Dialog(string title, string message, string errcode="")
        {
            Title = title;
            Message = message;
            ErrCode = errcode;
        }

        private string _ErrCode;
        public string ErrCode
        {
            get => _ErrCode;
            set
            {
                this.RaiseAndSetIfChanged(ref _ErrCode, value);
            }
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
