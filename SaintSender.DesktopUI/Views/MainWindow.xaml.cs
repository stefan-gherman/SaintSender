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

namespace SaintSender.DesktopUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GreetBtn_Click(object sender, RoutedEventArgs e)
        {
            var service = new GreetService();
            var name = NameTxt.Text;
            var greeting = service.Greet(name);
            ResultTxt.Text = greeting;

            
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var client = new ImapClient())
            {
                client.Connect("imap.gmail.com", 993, true);

                client.Authenticate("ionionescu2020demo@gmail.com", "***");

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

                for (int i = 0; i < inbox.Count; i++)
                {
                    var message = inbox.GetMessage(i);
                    Console.WriteLine($"From: {message.From} - Subject: {message.Subject} x: {items[i].Flags.Value}");
                }

                Console.WriteLine("*************************************");
                

                client.Disconnect(true);
            }
        }

        
    }

}
