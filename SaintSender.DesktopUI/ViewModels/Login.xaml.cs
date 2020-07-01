using MailKit.Net.Imap;
using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace SaintSender.DesktopUI.ViewModels
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private readonly UserData userData;

        public Login()
        {
            InitializeComponent();
            if(File.Exists(Environment.CurrentDirectory + "//credentials.json"))
            {
                userData = JsonService.DeserializeJsonFile(Environment.CurrentDirectory + "//credentials.json");
                email.Text = userData.Email;
                password.Password = userData.Password;
            }

        }

        /// <summary>
        /// Close login window when clicking Close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseLoginWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // refactor in multiple methods
        private void SignInClick(object sender, RoutedEventArgs e)
        {
            string validatedGmail = ValidateGmail(email.Text);

            UserData userData = new UserData()
            {
                Email = validatedGmail.Length != 0 ? validatedGmail : null,
                Password = password.Password.ToString().Length != 0 ? password.Password.ToString() : null
            };


            //todo Connect to gmail in this try and catch. Call MainWindow method and if connection is not succesfully display user a message.
            try
            {
                using (var client = new ImapClient())
                {
                    client.Connect("imap.gmail.com", 993, true);
                    client.Authenticate(userData.Email, userData.Password);

                    if (error.Visibility == Visibility.Visible)
                    {
                        error.Visibility = Visibility.Hidden;
                        success.Visibility = Visibility.Visible;
                    }


                }

                //TODO execute this method only when user connects to gmail successfully
                JsonService.SerializeToJson(userData);

                success.Visibility = Visibility.Visible;
                MainWindow mainWindow = new MainWindow(userData);
                mainWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                error.Visibility = Visibility.Visible;
            }
        }

        //todo create unit test
        /// <summary>
        /// Validate is a Gmail account using regex
        /// </summary>
        /// <param name="email">email as string</param>
        /// <returns>validated gmail as string</returns>
        private string ValidateGmail(string email)
        {
            Regex rx = new Regex(@"^[\w.+\-]+@gmail\.com$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (rx.IsMatch(email))
            {
                return email;
            }

            return "";
        }

        /// <summary>
        /// Clear placeholder text when you click the email text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearEmailOnTextBoxClick(object sender, RoutedEventArgs e)
        {
            email.Text = "";
            //email.Focus();
        }
    }
}