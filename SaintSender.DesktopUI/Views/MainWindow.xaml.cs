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
using ActiveUp.Net.Mail;

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
            var emailList = getEmailsAsync();
            foreach (Message email in emailList)
            {
                Console.WriteLine($"From: {email.From}, Subject: {email.Subject}");
                //Console.WriteLine("<p>{0}: {1}</p><p>{2}</p>", email.From, email.Subject, email.BodyHtml.Text);
                //if (email.Attachments.Count > 0)
                //{
                //    foreach (MimePart attachment in email.Attachments)
                //    {
                //        Console.WriteLine("<p>Attachment: {0} {1}</p>", attachment.ContentName, attachment.ContentType.MimeType);

                //    }
                //}
            }
        }

        public IEnumerable<Message> getEmailsAsync()
        {
            var mailRepository = new MailRepository(
                            "imap.gmail.com",
                            993,
                            true,
                            "ionionescu2020demo@gmail.com",
                            "demoaccountpassword"
                        );

            var emailList = mailRepository.GetAllMails("inbox");
            return emailList;
        }
    }

}
