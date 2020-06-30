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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SaintSender.Core.Services;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
using System.Collections.ObjectModel;
using SaintSender.Core.Entities;
using SaintSender.DesktopUI.ViewModels;

namespace SaintSender.DesktopUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private UserData userData;

        public ObservableCollection<Email> EmailsForDisplay { get; set; } = new ObservableCollection<Email>();

        public MainWindow(UserData userData)
        {
            InitializeComponent();
            this.userData = userData;
            
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            {
                using (var client = new ImapClient())
                {
                    client.Connect("imap.gmail.com", 993, true);

                    client.Authenticate(userData.Email, userData.Password);

                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadWrite);

                    Console.WriteLine("Total messages: {0}", inbox.Count);
                    Console.WriteLine("Recent messages: {0}", inbox.Recent);

                    var uids = client.Inbox.Search(SearchQuery.All);
                    var items = client.Inbox.Fetch(uids, MessageSummaryItems.Flags);
                    //foreach (var item in items)
                    //{
                    //    Console.WriteLine("Message # {0} has flags: {1}", item.Index, item.Flags.Value);
                    //    if (item.Flags.Value.HasFlag(MessageFlags.Seen))
                    //        Console.WriteLine("The message has been read.");
                    //    else
                    //        Console.WriteLine("The message has not been read.");
                    //}

                    for (int i = inbox.Count - 1; i >= inbox.Count - 20; i--)
                    {
                        var message = inbox.GetMessage(i);
                        Console.WriteLine($"From: {message.From} - Subject: {message.Subject} Date:{message.Date} x: {items[i].Flags.Value}");
                        EmailsForDisplay.Add(new Email()
                        {
                            Read = items[i].Flags.Value.ToString(),
                            From = message.From.ToString(),
                            DateReceived = message.Date.DateTime,
                            Subject = message.Subject.ToString(),
                            Message = message.Body.ToString(),
                            UniqueID = message.MessageId.ToString()
                        });
                    }

                    client.Disconnect(true);

                    emailSource.ItemsSource = EmailsForDisplay;
                }

            }

        }
    }
}
