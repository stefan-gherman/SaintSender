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
using System.Threading;
using System.Collections.Concurrent;
using System.Windows.Threading;
using SaintSender.DesktopUI.Views;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace SaintSender.DesktopUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Email> EmailsForDisplay { get; set; } = new ObservableCollection<Email>();
        public ConnectionService connectionChecker = new ConnectionService();
        public MainWindow()
        {

            InitializeComponent();
            emailSource.ItemsSource = EmailsForDisplay;


        }

        public List<Email> PopulateEmailsForDisplay()
        {
            List<Email> tempBag = new List<Email>();
            // Connection Checker mock test
            if (!connectionChecker.NLMAPICheck())
            {
                MessageBox.Show("There was an error with the connection");
                tempBag = (List<Email>)DeserializeBackup("Backups", "backup_emails.txt");
                if(tempBag.Count == 0)
                {
                    //SearchBox.Text = "No backup found"; thread error
                    MessageBox.Show("No suitable backup found!");
                } 
                else
                {
                    //SearchBox.Text = GetBackUpFileLastModified("Backups", "backup_emails.txt").ToString(); thread error
                    MessageBox.Show($"Loaded last backup from{GetBackUpFileLastModified("Backups", "backup_emails.txt").ToString()}");
                }
            }
            // Remove if- else block to return to previous version
            else
            {
                using (var client = new ImapClient())
                {
                    client.Connect("imap.gmail.com", 993, true);

                    client.Authenticate("ionionescu2020demo@gmail.com", "demoaccountpassword");

                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadWrite);

                    Console.WriteLine("Total messages: {0}", inbox.Count);
                    Console.WriteLine("Recent messages: {0}", inbox.Recent);

                    var uids = client.Inbox.Search(SearchQuery.All);
                    var items = client.Inbox.Fetch(uids, MessageSummaryItems.Flags);



                    for (int i = inbox.Count - 1; i >= inbox.Count - 20; i--)
                    {
                        var message = inbox.GetMessage(i);
                        Console.WriteLine($"From: {message.From} - Subject: {message.Subject} Date:{message.Date} x: {items[i].Flags.Value}");

                        tempBag.Add(new Email()
                        {
                            Read = items[i].Flags.Value.ToString(),
                            From = message.From.ToString(),
                            DateReceived = message.Date.DateTime,
                            Subject = message.Subject.ToString(),
                            Message = message.TextBody,
                            UniqueID = message.MessageId.ToString()
                        });
                    }
                    client.Disconnect(true);
                }
            }
            return tempBag;
        }

        public void AutoRefreshInbox()
        {
            while (true)
            {
                Thread.Sleep(5000);
                RefreshInbox();
            }

        }
        public async void RefreshInbox()
        {
            emailSource.ItemsSource = EmailsForDisplay;
            EmailsForDisplay.Clear();

            await Task.Run(() =>
            {
                var tempBag = PopulateEmailsForDisplay();
                Application.Current.Dispatcher.BeginInvoke(
                      DispatcherPriority.Background,
                      new Action(() =>
                      {
                          EmailsForDisplay.Clear();
                      }));
                foreach (var email in tempBag)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                      DispatcherPriority.Background,
                      new Action(() =>
                      {
                          EmailsForDisplay.Add(email);
                      }));
                }
            });
            emailSource.ItemsSource = EmailsForDisplay;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EmailsForDisplay.Add(new Email() { Read = "Seen", DateReceived = new DateTime(2000, 12, 22), From = "dd", Message = "test", Subject = "test", UniqueID = "testid" });
            emailSource.ItemsSource = EmailsForDisplay;
            RefreshInbox();
        }

        private void SearchBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (SearchBox.Text.Equals("Search email"))
            {
                SearchBox.Text = String.Empty;
            }

        }

        private void SearchBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (SearchBox.Text.Equals(""))
            {
                SearchBox.Text = "Search email";
            }
        }



        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Email> latestInbox = EmailsForDisplay;
            string searchTerm = SearchBox.Text;
            ObservableCollection<Email> searchResultsEmails = new ObservableCollection<Email>();
            foreach (var email in EmailsForDisplay)
            {
                if (email.From.Contains(searchTerm) || email.Subject.Contains(searchTerm) || email.Message.Contains(searchTerm))
                {
                    searchResultsEmails.Add(email);
                }
            }
            if (searchResultsEmails.Count() == 0)
            {
                MessageBox.Show("Sorry, no emails matched your search criteria. \n Displaying the regular inbox messages.");
                Console.WriteLine("Displaying regular inbox");
                emailSource.ItemsSource = latestInbox;

            }
            else
            {
                emailSource.ItemsSource = searchResultsEmails;
                Console.WriteLine("Displaying search results");
            }
            // emailSource.ItemsSource = EmailsForDisplay;
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("single click");
        }


        private void emailSource_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Email selectedEmail = (Email)emailSource.SelectedItems[0];
                ReadEmailWindow readEmailWindow = new ReadEmailWindow(selectedEmail);
                readEmailWindow.Show();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BackupEmails("Backups", "backup_emails.txt");
        }

        private void BackupEmails(string folderPath, string fileName)
        {
            string directoryPath = folderPath;
            string filePath = $@".\{directoryPath}\{fileName}";
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine("Folder does not exist, creating");
                    Directory.CreateDirectory(directoryPath);
                }
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using (StreamWriter writer = File.CreateText(filePath))
                {

                    var jsonString = JsonSerializer.Serialize(EmailsForDisplay, new JsonSerializerOptions() { WriteIndented = true });
                    writer.WriteLine(Eramake.eCryptography.Encrypt(jsonString));
                }



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private IEnumerable<Email> DeserializeBackup(string folderPath, string filename)
        {
            IEnumerable<Email> backupList = new List<Email>();
            string directoryPath = folderPath;
            string filePath = $@".\{directoryPath}\{filename}";

            if (!Directory.Exists(directoryPath))
            {
                return backupList;
            }
            else
            {
                if (!File.Exists(filePath))
                {
                    return backupList;
                }
                else
                {
                    string fs = File.ReadAllText(filePath);
                    backupList = JsonSerializer.Deserialize<List<Email>>(Eramake.eCryptography.Decrypt(fs));

                    foreach (var email in backupList)
                    {
                        Console.WriteLine(email.Message);
                    }
                }

            }

            return backupList;
        }

        private DateTime GetBackUpFileLastModified(string folderPath, string filename)
        {
            string directoryPath = folderPath;
            string filePath = $@".\{directoryPath}\{filename}";
            DateTime lastBackUp = File.GetLastWriteTime(filePath);
            return lastBackUp;
        }
            
    }

}
