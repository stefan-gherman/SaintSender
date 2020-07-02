using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using SaintSender.Core.Entities;
using SaintSender.Core.Interfaces;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.ViewModels;
using SaintSender.DesktopUI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SaintSender.DesktopUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UserData userData;
        public ObservableCollection<Email> EmailsForDisplay { get; set; } = new ObservableCollection<Email>();
        public IWebConnectionService connectionChecker = new ConnectionService();
        public string SystemMessageBackend { get; set; }
        public bool RefreshAllowed { get; set; } = true;
        public MainWindow(UserData userData)
        {
            InitializeComponent();
            this.userData = userData;
            emailSource.ItemsSource = EmailsForDisplay;
            // if there is no connection then disable buttons
            if (!connectionChecker.NLMAPICheck())
            {
                InboxButton.IsEnabled = false;
                ComposeMessageButton.IsEnabled = false;
                BackupButton.IsEnabled = false;
                this.Title = "Saint Sender Offline Mode";
            }
        }

        /// <summary>
        /// Populates the list of 20 emails to be displayed by the UI.
        /// </summary>
        /// <returns></returns>
        public List<Email> PopulateEmailsForDisplay()
        {
            List<Email> tempBag = new List<Email>();
            // Connection Checker mock test
            if (!connectionChecker.NLMAPICheck())
            {
                MessageBox.Show("There was an error with the connection");
                tempBag = (List<Email>)DeserializeBackup("Backups", "backup_emails.txt");
                if (tempBag.Count == 0)
                {
                    MessageBox.Show("No suitable backup found!");
                }
                else
                {
                    MessageBox.Show($"Loaded last backup from {GetBackUpFileLastModified("Backups", "backup_emails.txt").ToString()}");
                }
                RefreshAllowed = false;
            }
            else
            {
                using (var client = new ImapClient())
                {
                    client.Connect("imap.gmail.com", 993, true);
                    client.Authenticate(userData.Email, userData.Password);
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadWrite);

                    Console.WriteLine("Total messages: {0}", inbox.Count);
                    Console.WriteLine("Recent messages: {0}", inbox.Recent);

                    var uids = client.Inbox.Search(SearchQuery.All);
                    var items = client.Inbox.Fetch(uids, MessageSummaryItems.Flags);

                    for (int i = inbox.Count - 1; i >= inbox.Count - 20; i--)
                    {
                        var message = inbox.GetMessage(i);
                        Console.WriteLine($"From: {message.From} - Subject: {message.Subject} Date:{message.Date} x: {items[i].Flags.Value} y:{message.MessageId} z:{items[i].Index}");
                        tempBag.Add(new Email()
                        {
                            Read = items[i].Flags.Value.ToString() == "None" ? "Unread" : "Read",
                            From = message.From.ToString(),
                            DateReceived = message.Date.DateTime,
                            Subject = message.Subject.ToString(),
                            Message = message.TextBody,
                            UniqueID = message.MessageId.ToString(),
                            Index = items[i].Index
                        });
                    }
                    client.Disconnect(true);
                }
            }
            return tempBag;
        }

        /// <summary>
        /// Refreshes the list of emails to be displayed by the UI by calling <seealso cref="PopulateEmailsForDisplay"/>.
        /// </summary>
        public async void RefreshInbox()
        {
            emailSource.ItemsSource = EmailsForDisplay;
            ObservableCollection<Email> backupEmailsForDisplay = new ObservableCollection<Email>();
            foreach (Email email in EmailsForDisplay)
            {
                backupEmailsForDisplay.Add(email);
            }
            emailSource.ItemsSource = backupEmailsForDisplay;
            EmailsForDisplay.Clear();
            await Task.Run(() =>
            {
                var tempBag = PopulateEmailsForDisplay();
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
            backupEmailsForDisplay.Clear();
            SystemMessage.Content = String.Empty;
            string backupPath = Environment.CurrentDirectory + @"\Backups\backup_emails.txt";
            if (!connectionChecker.NLMAPICheck())
            {
                if (File.Exists(backupPath))
                {
                    SystemMessage.Content = $"No connection. Loaded backup from {GetBackUpFileLastModified("Backups", "backup_emails.txt").ToString()}";

                }
                else
                {
                    SystemMessage.Content = "No Connection. No suitable backup found!";
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshInbox();
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();
        }

        /// <summary>
        /// Refresh mechanism part. Algorithm that is called by the dispatch timer in Window_Loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (RefreshAllowed)
            {
                RefreshInbox();
            }
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

        private void emailSource_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Email selectedEmail = (Email)emailSource.SelectedItems[0];
                ReadEmailWindow readEmailWindow = new ReadEmailWindow(selectedEmail);
                readEmailWindow.Show();
                if (selectedEmail.Read.Equals("Unread"))
                {
                    selectedEmail.Read = "Read";
                    EmailStatusChange(selectedEmail.Index, MessageFlags.Seen);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Backup_Button_Click(object sender, RoutedEventArgs e)
        {
            BackupEmails("Backups", "backup_emails.txt");
        }

        /// <summary>
        /// Encrypts and places the emails in the list to a file on disk.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        private void BackupEmails(string folderPath, string fileName)
        {
            string directoryPath = folderPath;
            string filePath = $@".\{directoryPath}\{fileName}";
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    SystemMessage.Content = "Folder does not exist, creating";
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

        /// <summary>
        /// Decrypts backed up emails from JSON file on disk and returns them.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the time when the last backup was made.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private DateTime GetBackUpFileLastModified(string folderPath, string filename)
        {
            string directoryPath = folderPath;
            string filePath = $@".\{directoryPath}\{filename}";
            DateTime lastBackUp = File.GetLastWriteTime(filePath);
            return lastBackUp;
        }

        private void ComposeMessage_Click(object sender, RoutedEventArgs e)
        {
            ComposeMessage composeMessageWindow = new ComposeMessage();
            composeMessageWindow.Show();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                executeSearch();
            }
        }

        private void Search_Button_Click(object sender, RoutedEventArgs e)
        {
            executeSearch();
        }

        /// <summary>
        /// Executes the search on emails based on the text in the Search Box.
        /// </summary>
        private void executeSearch()
        {
            RefreshAllowed = false;
            string searchString = SearchBox.Text;
            ObservableCollection<Email> searchResults = new ObservableCollection<Email>();
            string pattern = searchString;
            foreach (Email email in EmailsForDisplay)
            {
                string emailFrom = " ";
                if (email.From != null)
                {
                    emailFrom = email.From;
                }
                string emailSubject = " ";
                if (email.Subject != null)
                {
                    emailSubject = email.Subject;
                }
                string emailMessage = " ";
                if (email.Message != null)
                {
                    emailMessage = email.Message;
                }
                bool matchFrom = Regex.IsMatch(emailFrom, pattern, RegexOptions.IgnoreCase);
                bool matchSubject = Regex.IsMatch(emailSubject, pattern, RegexOptions.IgnoreCase);
                bool matchMessage = Regex.IsMatch(emailMessage, pattern, RegexOptions.IgnoreCase);
                if (matchFrom || matchSubject || matchMessage)
                {
                    searchResults.Add(email);
                }
            }
            if (searchResults.Count() > 0)
            {
                SystemMessage.Content = $"Displaying search results for: {searchString} ({searchResults.Count} results)";
                emailSource.ItemsSource = searchResults;
            }
            else
            {
                SystemMessage.Content = $"No search matches for {searchString}. Displaying regular inbox.";
                emailSource.ItemsSource = EmailsForDisplay;
                RefreshAllowed = true;
            }
        }

        /// <summary>
        /// Changes the status of an Unread email to Read and sends server post request.
        /// </summary>
        /// <param name="mailIndex"></param>
        /// <param name="flag"></param>
        private void EmailStatusChange(int mailIndex, MessageFlags flag)
        {
            using (var client = new ImapClient())
            {
                client.Connect("imap.gmail.com", 993, true);
                client.Authenticate(userData.Email, userData.Password);
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                inbox.AddFlagsAsync(mailIndex, flag, false);
                client.Disconnect(true);
            }
        }

        private void Inbox_Button_Click(object sender, RoutedEventArgs e)
        {
            if (connectionChecker.NLMAPICheck())
            {
                emailSource.ItemsSource = EmailsForDisplay;
                RefreshInbox();
                RefreshAllowed = true;
            }
            else
            {
                RefreshAllowed = false;
            }
        }

    }
}
