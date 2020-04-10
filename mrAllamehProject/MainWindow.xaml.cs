using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using mrAllamehProject.Class;
using System.Threading;

namespace mrAllamehProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SMTP mSMTP = new SMTP();
        private GmailAccount mGmailAccount = new GmailAccount();
        private POP3 mPOP3;
        private ImapClient ic;
        private SocksHelper socksHelperforSmtp;
        public MainWindow()
        {
            InitializeComponent();
            mPOP3 = new POP3(ListBoxEvents);
            ic = new ImapClient(ListBoxEvents);
            socksHelperforSmtp = new SocksHelper(ListBoxEvents);

        }

      

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.WindowState == WindowState.Normal)
            {
                mainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                mainWindow.WindowState = WindowState.Normal;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(500);
            App.Current.Shutdown();
        }

        private void ButtonConnect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GridLogin.Visibility = Visibility.Hidden;
            GridMain.Visibility = Visibility.Visible;
            mGmailAccount.UserName = TextBoxUserName.Text;
            mGmailAccount.Password = TextBoxPassword.Password;
            TextBlockGmailAcountName.Text = mGmailAccount.UserName;
            TextboxFromSmtp.Text = TextBoxUserName.Text;
            TextBoxGmailPOP3.Text = mGmailAccount.UserName;

        }
        private void imap(int caseSwitch)
        {
              switch (caseSwitch)
                    {
                        case 0:         // exit
                            if (ic.userAvailable) { ic.DcFromGmail(); }
                            caseSwitch = -1;
                            break;

                        case 1:         // log in
                            if (!ic.userAvailable) { ic.ConnectToGmail(TextBoxIMAPUser.Text,TextBoxIMAPPass.Password); }
                            else { ListBoxEvents.Items.Add("ERR : User has already logged in"); }
                            break;

                        case 2:         // log off
                            if (ic.userAvailable) { ic.DcFromGmail(); }
                            else { ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 3:         // inbox status
                            if (ic.userAvailable) { ic.DefaultStatus(); }
                            else { ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 4:         // Show all mailboxes
                            if (ic.userAvailable) { ic.SelectMailboxes(); }
                            else { ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 5:         // Select mailbox
                            if (ic.userAvailable) { ic.SelectMailbox(); }
                            else { ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 6:         // Close mailbox
                            if (ic.userAvailable) { ic.ClosetMailbox(); }
                            else { ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 7:         // Fetch Header
                            if (ic.userAvailable) { ic.FetchHead(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 8:         // Fetch date
                            if (ic.userAvailable) { ic.FetchDate(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 9:         // Flags: mark all as read
                            if (ic.userAvailable) { ic.SetAllRead(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 10:        // Flags: mark 1x as unread
                            if (ic.userAvailable) { ic.SetAsUnread(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 11:        // Expunge e-mail
                            if (ic.userAvailable) { ic.DeleteLetter(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 12:        // Search by criteria
                            if (ic.userAvailable) { ic.SearchByText(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 13:        // Search unseen UIDs
                            if (ic.userAvailable) { ic.SearchUnseenUIDs(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 14:        // Copy e-mail
                            if (ic.userAvailable) { ic.Copy(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 15:        // Create new mailbox
                            if (ic.userAvailable) { ic.CreateNew(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 16:        // Rename mailbox
                            if (ic.userAvailable) { ic.RenameMail(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 17:        // Delete mailbox
                            if (ic.userAvailable) { ic.DeleteMail(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 18:        // Check answer's validity
                            if (ic.userAvailable) { ic.DoNothing(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 19:        // Examine mailbox
                            if (ic.userAvailable) { ic.ExamineMailbox(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 20:        // Subscribe to a mailbox
                            if (ic.userAvailable) { ic.SubscribeMailbox(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 21:        // Unubscribe from a mailbox
                            if (ic.userAvailable) { ic.UnsubscribeMailbox(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 22:        // Check subscribes mailboxes
                            if (ic.userAvailable) { ic.CheckSubscribed(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 23:        // All e-mail IDs
                            if (ic.userAvailable) { ic.UIDSearch(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        case 24:        // All e-mail flags
                            if (ic.userAvailable) { ic.GetFlags(); }
                            else {ListBoxEvents.Items.Add("ERR : User is not logged in"); }
                            break;

                        default:
                           ListBoxEvents.Items.Add("Wrong choice.");
                            break;
                    }
             

        }
        private void TextBoxIPCammera_IsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (TextBoxUserName != null)
            {
                if (TextBoxUserName.Text.Equals("User Name"))
                {
                    TextBoxUserName.Text = "";
                }
                else
                {

                    //  TextBoxIPCammera.Text = "IP Address";

                }
            }
        }

        private void TextBoxIPCammera_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxUserName.Text.Equals("User Name"))
            {
                TextBoxUserName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABADB3"));
            }
            else
            {

                TextBoxUserName.Foreground = new SolidColorBrush(Colors.Black);

                //  TextBoxIPCammera.Text = "IP Address";

            }
        }

      
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //mSMTP.connectToSMTP(TextBoxUserName.Text,TextboxToSmtp.Text,TextboxSubjectSmtp.Text,TextboxMessageSMTP.Text,mGmailAccount.Password);
            socksHelperforSmtp.conect(mGmailAccount.UserName,mGmailAccount.Password,
                TextboxToSmtp.Text,TextboxFromSmtp.Text,TextboxSubjectSmtp.Text,TextboxMessageSMTP.Text);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            mPOP3.main(TextBoxServerPOP3.Text, mGmailAccount.UserName,mGmailAccount.Password,int.Parse(TextBoxPortPOP3.Text));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ListBoxEvents.Items.Count > 0) ListBoxEvents.Items.Clear();
        }

      

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            imap(1);
        }

        private void btnStartConection_Click(object sender, RoutedEventArgs e)
        {
            socksHelperforSmtp.startConection();
        }

        private void btnLogoff_Click(object sender, RoutedEventArgs e)
        {
            imap(2);
        }

        private void btnInboxStatus_Click(object sender, RoutedEventArgs e)
        {
            imap(3);
        }

        private void btnShowAllmails_Click(object sender, RoutedEventArgs e)
        {
            imap(4);
        }

        private void btnSelectMails_Click(object sender, RoutedEventArgs e)
        {
            imap(5);
        }

        private void btnMarkAllRead_Click(object sender, RoutedEventArgs e)
        {
            imap(9);
        }

        private void btnDeleteMailBox_Click(object sender, RoutedEventArgs e)
        {
            imap(17);
        }

        private void btnAllemailsId_Click(object sender, RoutedEventArgs e)
        {
            imap(23);
        }

        private void btnCloseMailbox_Click(object sender, RoutedEventArgs e)
        {
            imap(6);
        }
    }
}
