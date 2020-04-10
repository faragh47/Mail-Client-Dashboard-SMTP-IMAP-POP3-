using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace mrAllamehProject.Class
{
    class POP3
    {
        private ListBox listbox;
        public POP3(ListBox listbox)
        {

            this.listbox = listbox;
        }

        public void main(string server, string acount,string pass ,int port)
        {

            Console.WriteLine("POP3 MIME Client Demo");
            Console.WriteLine("=====================");
            Console.WriteLine();

            //prepare pop client
            // TODO: Replace server address, username and password with your own credentials.
            Pop3MimeClient DemoClient = new Pop3MimeClient(server, port, true, acount,pass);

            DemoClient.Trace += new TraceHandler(Console.WriteLine);
            DemoClient.ReadTimeout = 60000; //give pop server 60 seconds to answer

            //establish connection
            DemoClient.Connect();

            //get mailbox stats
            int numberOfMailsInMailbox, mailboxSize;
            DemoClient.GetMailboxStats(out numberOfMailsInMailbox, out mailboxSize);

            //get at most the xx first emails
            RxMailMessage mm;
            int downloadNumberOfEmails;
            int maxDownloadEmails = 99;
            if (numberOfMailsInMailbox < maxDownloadEmails)
            {
                downloadNumberOfEmails = numberOfMailsInMailbox;
            }
            else
            {
                downloadNumberOfEmails = maxDownloadEmails;
            }
            for (int i = 1; i <= downloadNumberOfEmails; i++)
            {
                DemoClient.GetEmail(i, out mm);
                if (mm == null)
                {
                    Console.WriteLine("Email " + i.ToString() + " cannot be displayed.");
                    listbox.Items.Add("Email " + i.ToString() + " cannot be displayed.");
                }
                else
                {
                  
                    Console.WriteLine(mm.MailStructure());
                    listbox.Items.Add(mm.MailStructure());

                }
            }

            DemoClient.Disconnect();

            Console.WriteLine();
            Console.WriteLine("======== Press Enter to end program");
            Console.ReadLine();



        }


    }
}
