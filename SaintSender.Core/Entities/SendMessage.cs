using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SaintSender.Core.Services;
using System;
using System.IO;

namespace SaintSender.Core.Entities
{
    public class SendMessage
    {
        private readonly UserData userData;
        private MimeMessage messageToSend;

        public SendMessage()
        {
            if (File.Exists(Environment.CurrentDirectory + "//credentials.json"))
            {
                userData = JsonService.DeserializeJsonFile(Environment.CurrentDirectory + "//credentials.json");

            }
        }

        public string To { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }


        /// <summary>
        /// Set subject, message and where to send the email
        /// </summary>
        private void SetSendmessageData()
        {

            messageToSend = new MimeMessage
            {
                Sender = new MailboxAddress("Ion Ionescu", userData.Email),
                Subject = Subject
            };

            messageToSend.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = Message
            };

            messageToSend.To.Add(address: new MailboxAddress(To));

        }

        /// <summary>
        /// Connect to gmail smtp and send message
        /// </summary>
        private async void ConnectAndSendEmail()
        {
            using (var smtp = new SmtpClient())
            {
                smtp.MessageSent += (sender, args) =>
                {
                    Console.WriteLine("Email was sent " + args.Response);
                };

                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(userData.Email, userData.Password);
                await smtp.SendAsync(messageToSend);
                await smtp.DisconnectAsync(true);
            }
        }

        /// <summary>
        /// Call SetMessageData() and ConnectAndSendEmail()
        /// </summary>
        public void SendMessageToEmail()
        {
            SetSendmessageData();
            ConnectAndSendEmail();
        }
    }
}
