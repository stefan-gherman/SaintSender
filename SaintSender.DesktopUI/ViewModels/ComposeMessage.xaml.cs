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



    }
}
