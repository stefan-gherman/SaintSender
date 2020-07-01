using SaintSender.Core.Entities;
using System;
using System.Windows;
using System.Windows.Input;

namespace SaintSender.DesktopUI.ViewModels
{
    /// <summary>
    /// Interaction logic for ComposeMessage.xaml
    /// </summary>
    public partial class ComposeMessage : Window
    {
        public ComposeMessage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close Compose Message window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseComposeMesssageWindow_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to discard the message ?", "My App", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Close();
                    break;

                case MessageBoxResult.No:
                    break;
            }
        }

        /// <summary>
        /// Send email when button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            SendMessage sendMessageObject = new SendMessage()
            {
                To = SendTo.Text,
                Subject = SendSubject.Text,
                Message = SendMessage.Text
            };
            sendMessageObject.SendMessageToEmail();
            Close();
        }

        #region Clear default email text box on hover

        private void SendTo_Enter(object sender, MouseEventArgs e)
        {
            if (SendTo.Text.Equals("email@gmail.com")) SendTo.Text = String.Empty;
        }

        private void SendTo_Leave(object sender, MouseEventArgs e)
        {
            if (SendTo.Text.Equals("")) SendTo.Text = "email@gmail.com";
        }

        #endregion Clear default email text box on hover

        #region Clear default subject text box on hover

        /// <summary>
        /// If text box has default email then clear it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendSubject_Enter(object sender, MouseEventArgs e)
        {
            if (SendSubject.Text.Equals("Subject")) SendSubject.Text = "";
        }

        /// <summary>
        /// If text box is empty then write default subject text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendSubject_Leave(object sender, MouseEventArgs e)
        {
            if (SendSubject.Text.Equals("")) SendSubject.Text = "Subject";
        }

        #endregion Clear default subject text box on hover

        #region Clear default message text box on hover

        /// <summary>
        /// If message box has default message then clear it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendMessage_Enter(object sender, MouseEventArgs e)
        {
            if (SendMessage.Text.Equals("Write your message.")) SendMessage.Text = "";
        }

        /// <summary>
        /// If message box is empty then write default message text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendMessage_Leave(object sender, MouseEventArgs e)
        {
            if (SendMessage.Text.Equals("")) SendMessage.Text = "Write your message.";
        }

        #endregion Clear default message text box on hover
    }
}