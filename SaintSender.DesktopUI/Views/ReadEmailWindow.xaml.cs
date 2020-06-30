using SaintSender.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SaintSender.DesktopUI.Views
{
    /// <summary>
    /// Interaction logic for ReadEmailWindow.xaml
    /// </summary>
    public partial class ReadEmailWindow : Window
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string Message { get; set; }
        public ReadEmailWindow(Email selectedEmail)
        {
            InitializeComponent();
            ReadSubject.Text = selectedEmail.Subject;
            ReadFrom.Text = selectedEmail.From.ToString();
            ReadDateReceived.Text = selectedEmail.DateReceived.ToString();
            ReadMessage.Text = selectedEmail.Message;

        }
    }
}
