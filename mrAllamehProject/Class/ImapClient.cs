using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;

namespace mrAllamehProject.Class
{
    public class ImapClient
    {
        private System.Net.Sockets.TcpClient tcpc = null;
        private System.Net.Security.SslStream ssl = null;

        private int bytes = -1;
        private byte[] buffer;
        private byte[] commandTemplate;
        private ListBox ListBoxEvents;
        private string path;
        private System.IO.StreamWriter sw = null;

        public bool userAvailable = false;

        private bool pTaggedMessage = false;
        private string answer = null, previousAnswer = null;

        public int commandNumber = 1;
        public string prefix = "A1";
        private int index;

        public ImapClient(ListBox listbox)
        {
            ListBoxEvents = listbox;
            CreateLogFile();
        }

        private void CreateLogFile()
        {
            path = Environment.CurrentDirectory + "\\emailresponse.txt";
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            sw = new System.IO.StreamWriter(System.IO.File.Create(path));
        }

        public void ConnectToGmail(string user,string pass)      //---#1 log in
        {
         


            try
            {
                tcpc = new System.Net.Sockets.TcpClient("imap.gmail.com", 993);   // PORTS: TCP - 143. TCP/SSL - 993.
                ssl = new System.Net.Security.SslStream(tcpc.GetStream());
                ssl.AuthenticateAsClient("imap.gmail.com");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Environment.Exit(0);
            }

            ReceiveResponse("");
            ReceiveResponse(prefix + " LOGIN " + user + " " + pass + " \r\n");

            if (answer.Contains("OK"))
            {
                userAvailable = true;
            }
            else
            {
                userAvailable = false;
            }
        }

        public void DcFromGmail()         //---#2 log off
        {
            ReceiveResponse(prefix + " LOGOUT\r\n");

            if (answer.Contains("OK"))
            {
                userAvailable = false;
            }
            else
            {
                userAvailable = true;
            }

            if (ssl != null)
            {
                ssl.Close();
                ssl.Dispose();
            }
            if (tcpc != null)
            {
                tcpc.Close();
            }
        }

        public void DefaultStatus()        //---#3: default inbox status (seen + unseen message count)
        {
            ReceiveResponse(prefix + " STATUS INBOX (MESSAGES UNSEEN)\r\n");
            ReceiveResponse(prefix + " CAPABILITY\r\n");
        }

        public void SelectMailboxes()        //---#4: All mailboxes
        {
            ReceiveResponse(prefix + " LIST " + "\"\"" + " %" + "\r\n");
            ReceiveResponse(prefix + " LIST " + "\"\"" + " [Gmail]/%" + "\r\n");
        }

        public void SelectMailbox()         //---#5: select mailbox
        {
            ListBoxEvents.Items.Add("Mailbox? : ");
            string parameter = "0";

            ReceiveResponse(prefix + " SELECT " + parameter + "\r\n");
        }

        public void ClosetMailbox()         //---#6: close mailbox
        {
            ReceiveResponse(prefix + " CLOSE\r\n");
        }

        public void FetchHead()             //---#7: Fetch header
        {
            ListBoxEvents.Items.Add("Letter's UID? : ");
            try
            {
                index = Convert.ToInt32(Console.ReadLine());
            }
            catch (System.OverflowException e)
            {
                Debug.WriteLine(e.Message);
                Environment.Exit(7);
            }
            catch (System.FormatException ex)
            {
                Debug.WriteLine(ex.Message);
                Environment.Exit(7);
            }

            ReceiveResponse(prefix + " FETCH " + index + " (BODY.PEEK[HEADER])\r\n");
        }

        public void FetchDate()             //---#8: Fetch internal date
        {
            ListBoxEvents.Items.Add("Letter's UID? : ");
            try
            {
                index = Convert.ToInt32(Console.ReadLine());
            }
            catch (System.OverflowException e)
            {
                Debug.WriteLine(e.Message);
                Environment.Exit(8);
            }
            catch (System.FormatException ex)
            {
                Debug.WriteLine(ex.Message);
                Environment.Exit(8);
            }

            ReceiveResponse(prefix + " FETCH " + index + " INTERNALDATE\r\n");
        }

        public void SetAllRead()            //---#9: set all messages: read
        {
            string partOne, partTwo;
            partOne = prefix + @" STORE 1:* +FLAGS \Seen";
            partTwo = "\r\n";

            ReceiveResponse(partOne + partTwo);
        }

        public void SetAsUnread()           //---#10: set message as unread
        {
            ListBoxEvents.Items.Add("Letter's UID to mark as UNSEEN? : ");
            try
            {
                index = Convert.ToInt32(Console.ReadLine());
            }
            catch (System.OverflowException e)
            {
                Debug.WriteLine(e.Message);
                Environment.Exit(10);
            }
            catch (System.FormatException ex)
            {
                Debug.WriteLine(ex.Message);
                Environment.Exit(10);
            }

            string partOne, partTwo;
            partOne = prefix + " STORE " + index + @" -FLAGS \Seen";
            partTwo = "\r\n";

            ReceiveResponse(partOne + partTwo);
        }

        public void DeleteLetter()            //---#11: Expunge e-mail
        {
            ListBoxEvents.Items.Add("Letter's UID to DELETE : ");
            try
            {
                index = Convert.ToInt32(Console.ReadLine());
            }
            catch (System.OverflowException e)
            {
                Debug.WriteLine(e.Message);
                Environment.Exit(11);
            }
            catch (System.FormatException ex)
            {
                Debug.WriteLine(ex.Message);
                Environment.Exit(11);
            }

            string partOne, partTwo;
            partOne = prefix + " STORE " + index + @" +FLAGS \Deleted";
            partTwo = "\r\n";

            ReceiveResponse(partOne + partTwo);
            ReceiveResponse(prefix + " EXPUNGE\r\n");
        }

        public void SearchByText()            //---#12: Search by criteria
        {
            ListBoxEvents.Items.Add("Text? : ");
            string parameter = Console.ReadLine();
            ReceiveResponse(prefix + " SEARCH BODY \"" + parameter + "\"\r\n");
        }

        public void SearchUnseenUIDs()        //---#13: Get unseen e-mail IDs
        {
            ReceiveResponse(prefix + " SEARCH UNSEEN\r\n");
        }

        public void Copy()                    //---#14: Copy e-mail
        {
            ListBoxEvents.Items.Add("Letter's UID to COPY : ");
            try
            {
                index = Convert.ToInt32(Console.ReadLine());
            }
            catch (System.OverflowException e)
            {
                Debug.WriteLine(e.Message);
                Environment.Exit(14);
            }
            catch (System.FormatException ex)
            {
                Debug.WriteLine(ex.Message);
                Environment.Exit(14);
            }

            ListBoxEvents.Items.Add("COPY to which mailbox? : ");
            string parameter = Console.ReadLine();

            ReceiveResponse(prefix + " COPY " + index + " " + parameter + "\r\n");
        }

        public void CreateNew()               //---#15: Create new mailbox
        {
            ListBoxEvents.Items.Add("New Mailbox? : ");
            string parameter = Console.ReadLine();

            ReceiveResponse(prefix + " CREATE " + parameter + "\r\n");
        }

        public void RenameMail()               //---#16: Rename mailbox
        {
            ListBoxEvents.Items.Add("Mailbox? : ");
            string parameter = Console.ReadLine();

            ListBoxEvents.Items.Add("Rename into? : ");
            string parameter2 = Console.ReadLine();

            ReceiveResponse(prefix + " RENAME " + parameter + " " + parameter2 + "\r\n");
        }

        public void DeleteMail()               //---#17: Delete mailbox
        {
            ListBoxEvents.Items.Add("Mailbox? : ");
            string parameter = Console.ReadLine();

            ReceiveResponse(prefix + " DELETE " + parameter + "\r\n");
        }

        public void DoNothing()                //---#18: For checking answer's validity
        {
            ReceiveResponse(prefix + " NOOP\r\n");
            ReceiveResponse(prefix + " NOOP\r\n");
            ReceiveResponse(prefix + " NOOP\r\n");
        }

        //--
        public void ExamineMailbox()           //---#19: Examine mailbox (= select in read-only state)
        {
            ListBoxEvents.Items.Add("Mailbox? : ");
            string parameter = Console.ReadLine();

            ReceiveResponse(prefix + " EXAMINE " + parameter + "\r\n");
        }

        public void SubscribeMailbox()           //---#20: Examine mailbox (= select in read-only state)
        {
            ListBoxEvents.Items.Add("Mailbox? : ");
            string parameter = Console.ReadLine();

            ReceiveResponse(prefix + " SUBSCRIBE " + parameter + "\r\n");
        }

        public void UnsubscribeMailbox()         //---#21: Examine mailbox (= select in read-only state)
        {
            ListBoxEvents.Items.Add("Mailbox? : ");
            string parameter = Console.ReadLine();

            ReceiveResponse(prefix + " UNSUBSCRIBE " + parameter + "\r\n");
        }

        public void CheckSubscribed()            //---#22: Examine mailbox (= select in read-only state) (enter  * + * to see all)
        {
            ListBoxEvents.Items.Add("Reference? : ");
            string parameter = Console.ReadLine();

            ListBoxEvents.Items.Add("Mailbox? : ");
            string parameter2 = Console.ReadLine();

            ReceiveResponse(prefix + " LSUB \"" + parameter + "\" " + parameter2 + "\r\n");
        }

        public void UIDSearch()                  //---#23: All available e-mails
        {
            ReceiveResponse(prefix + " UID SEARCH ALL" + "\r\n");
        }

        public void GetFlags()                   //---#24: Fetch <ID> flags
        {
            ListBoxEvents.Items.Add("Letter's ID to Fetch flags of : ");
            try
            {
                index = Convert.ToInt32(Console.ReadLine());
            }
            catch (System.OverflowException e)
            {
                Debug.WriteLine(e.Message);
                Environment.Exit(14);
            }
            catch (System.FormatException ex)
            {
                Debug.WriteLine(ex.Message);
                Environment.Exit(14);
            }
            ReceiveResponse(prefix + " FETCH " + index + " FLAGS" + "\r\n");
        }

        //----- Talking to the server -------------------------------------------------
        private void ReceiveResponse(string command)
        {
            try
            {
                if (command != "")
                {
                    if (tcpc.Connected)
                    {
                        commandTemplate = Encoding.ASCII.GetBytes(command);
                        ssl.Write(commandTemplate, 0, commandTemplate.Length);
                    }
                    else
                    {
                        throw new ApplicationException("TCP CONNECTION DISCONNECTED");
                    }
                }
                ssl.Flush();

                ListBoxEvents.Items.Add(command);
                sw.WriteLine(command);
                bytes = -1;

                while (bytes != 0)
                {
                    previousAnswer = answer;

                    buffer = new byte[2048];
                    bytes = ssl.Read(buffer, 0, 2048);

                    answer = Encoding.ASCII.GetString(buffer);

                    ListBoxEvents.Items.Add(answer.Substring(0, bytes));
                    sw.WriteLine(answer.Substring(0, bytes));

                    bool endMessage = answer.Contains("\r\n");

                    if (commandNumber <= 1)
                    {
                        pTaggedMessage = false;
                    }
                    else
                    {
                        pTaggedMessage = previousAnswer.Contains(prefix);
                    }

                    if (command != "")
                    {
                        bool taggedEndMessage = answer.Contains(prefix);

                        if ((endMessage && taggedEndMessage) || bytes == 0 || (pTaggedMessage && endMessage))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if ( endMessage || bytes == 0)
                        {
                            break;
                        }
                    }
                }

                if(command != "")
                {
                    commandNumber += 1;
                    prefix = "A" + commandNumber;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Environment.Exit(0);
            }
        }
    }
}
