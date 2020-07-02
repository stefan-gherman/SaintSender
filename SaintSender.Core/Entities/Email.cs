using System;
using System.ComponentModel;

namespace SaintSender.Core.Entities
{
    public class Email : INotifyPropertyChanged
    {
        public string Read { get; set; }
        public string From { get; set; }
        public DateTime DateReceived { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string UniqueID { get; set; }
        public int Index { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
