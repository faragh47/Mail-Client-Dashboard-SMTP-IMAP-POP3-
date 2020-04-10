using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Windows.Controls;

namespace mrAllamehProject.Class
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>


    public class SmtpCmdFailedException : Exception
    {
        public int ReplyCode;

        public SmtpCmdFailedException(int code, string message)
            : base(message)
        {
            ReplyCode = code;
        }
    }
        public class SocksHelper
        {

            string replyText = "";
        private string resp = "";
        private int state = -1;
        private System.IO.StreamReader reader;
            private System.IO.StreamWriter writer;
        private ListBox tbConsole;
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        public SocksHelper(ListBox listbox)
        {
            tbConsole = listbox;
        }
            private int readResponse(string replyText, params int[] expectedReplyCodes)
            {
               string line = reader.ReadLine();
               resp = line;
               GetFullResponse();
                  tbConsole.Items.Add(resp);

            if (line == null)
                    throw new EndOfStreamException();

                // extract the 3-digit reply code
                string replyCodeStr = line.Substring(0, 3);

                // extract the text message, if any
                replyText = line.Substring(4);
                // check for a multi-line response
                if ((line.Length > 3) && (line[3] == '-'))
                {
                    // keep reading until the final line is received
                    string contStr = replyCodeStr + "-";
                    do
                    {
                        line = reader.ReadLine();
                        if (line == null)
                            throw new EndOfStreamException();
                        replyText += "\n" + line.Substring(4);
                    }
                    while (line.StartsWith(contStr));
                }

                int replyCode = Int32.Parse(replyCodeStr);

                // if the caller expects specific reply code(s), check
                // for a match and throw an exception if not found...
                if (expectedReplyCodes.Length > 0)
                {
                    if (Array.IndexOf(expectedReplyCodes, replyCode) == -1)
                        throw new SmtpCmdFailedException(replyCode, replyText);
                }
                this.replyText = replyText;

                // return the actual reply code that was received
                return replyCode;
            }

            private int readResponse(params int[] expectedReplyCodes)
            {

               int str= readResponse(replyText, expectedReplyCodes);
           
                return str;
            }

            private int sendCommand(string command, string replyText, params int[] expectedReplyCodes)
            {
                writer.WriteLine(command);
                writer.Flush();
                return readResponse(replyText, expectedReplyCodes);
            }

            private int sendCommand(string command, params int[] expectedReplyCodes)
            {
                return sendCommand(command, replyText, expectedReplyCodes);
            }

            void doAuthLogin(string username, string password)
            {
                // an authentication command returns 235 if authentication
                // is finished successfully, or 334 to prompt for more data.
                // Anything else is an error...

                int replyCode = sendCommand("AUTH LOGIN", replyText, 235, 334);

                if (replyCode == 334)
                {
                    // in the original spec for LOGIN (draft-murchison-sasl-login-00.txt), the
                    // username prompt is defined as 'User Name' and the password prompt is
                    // defined as 'Password'. However, the spec also mentions that there is at
                    // least one widely deployed client that expects 'Username:' and 'Password:'
                    // instead, and those are the prompts that most 3rd party documentations
                    // of LOGIN describe.  So we will look for all known prompts and act accordingly.
                    // Also throwing in 'Username' just for good measure, as that one has been seen
                    // in the wild, too...
                    string[] challenges = new string[] { "Username:", "User Name", "Username", "Password:", "Password" };

                    do
                    {
                        string challenge = Encoding.UTF8.GetString(Convert.FromBase64String(replyText));

                        switch (Array.IndexOf(challenges, challenge))
                        {
                            case 0:
                            case 1:
                            case 2:
                                replyCode = sendCommand(Convert.ToBase64String(Encoding.UTF8.GetBytes(username)), replyText, 235, 334);
                                break;

                            case 3:
                            case 4:
                                replyCode = sendCommand(Convert.ToBase64String(Encoding.UTF8.GetBytes(password)), replyText, 235, 334);
                                break;

                            default:
                                throw new SmtpCmdFailedException(replyCode, replyText);
                        }
                    }
                    while (replyCode == 334);
                }
            }
        public void startConection()
        {
            TcpClient tcpclient = new TcpClient();
            tcpclient.Connect("smtp.gmail.com", 465);
            // implicit SSL is always used on SMTP port 465
            System.Net.Security.SslStream sslstream = new SslStream(tcpclient.GetStream());
            sslstream.AuthenticateAsClient("smtp.gmail.com");
            //bool flag = sslstream.IsAuthenticated;   // check flag
            writer = new StreamWriter(sslstream);
            reader = new StreamReader(sslstream);


            // read the server's initial greeting
            readResponse(220);

        }
        public void conect(string authUser,string authPassword,string toEmail,string fromEmail,string Subject,string data)
            {

            string[] capabilities = new string[100];
            string[] authTypes = new string[100];

            // identify myself and get the server's capabilities
            if (sendCommand("EHLO myClientName", replyText) == 250)
                {
                    // parse capabilities
                    capabilities = replyText.Split(new Char[] { '\n' });
                    string auth = Array.Find(capabilities, s => s.StartsWith("AUTH ", true, null));
                    authTypes = auth.Substring(5).Split(new Char[] { ' ' });

                    // authenticate as needed...
                    if (Array.IndexOf(authTypes, "LOGIN") != -1)
                        doAuthLogin(authUser, authPassword);
                }
                else
                {
                    // EHLO not supported, have to use HELO instead, but then
                    // the server's capabilities are unknown...

                    capabilities = new string[] { };
                    authTypes = new string[] { };

                    sendCommand("HELO myclientname", 250);

                    // try to authenticate anyway...
                    doAuthLogin(authUser, authPassword);
                }

                // check for pipelining support... (OPTIONAL!!!)
                if (Array.IndexOf(capabilities, "PIPELINING") != -1)
                {
                    // can pipeline...

                    // send all commands first without reading responses in between
                    writer.WriteLine("MAIL FROM:<" + fromEmail + ">");
                    writer.WriteLine("RCPT TO:<" + toEmail + ">");
                    writer.WriteLine("DATA");
                    writer.Flush();

                    // now read the responses...

                    Exception e = null;

                    // MAIL FROM
                    int replyCode = readResponse(replyText);
                    if (replyCode != 250)
                        e = new SmtpCmdFailedException(replyCode, replyText);

                    // RCPT TO
                    replyCode = readResponse(replyText);
                    if ((replyCode != 250) && (replyCode != 251) && (e == null))
                        e = new SmtpCmdFailedException(replyCode, replyText);

                    // DATA
                    replyCode = readResponse(replyText);
                    if (replyCode == 354)
                    {
                        // DATA accepted, must send email followed by "."
                        writer.WriteLine("Subject:"+ Subject);
                        writer.WriteLine(data);
                        writer.WriteLine(".");
                        writer.Flush();

                        // read the response
                        replyCode = readResponse(replyText);
                        if ((replyCode != 250) && (e == null))
                            e = new SmtpCmdFailedException(replyCode, replyText);
                    }
                    else
                    {
                        // DATA rejected, do not send email
                        if (e == null)
                            e = new SmtpCmdFailedException(replyCode, replyText);
                    }

                    if (e != null)
                    {
                        // if any command failed, reset the session
                        sendCommand("RSET");
                        throw e;
                    }
                }
                else
                {
                    // not pipelining, MUST read each response before sending the next command...

                    sendCommand("MAIL FROM:<" + "farghadani4747@gmail.com" + ">", 250);
                    try
                    {
                        sendCommand("RCPT TO:<" + "farghadani4747@gmail.com" + ">", 250, 251);
                        sendCommand("DATA", 354);
                        writer.WriteLine("Subject: Email test");
                        writer.WriteLine("");
                        writer.WriteLine("Test 1 2 3");
                        writer.Flush();
                        sendCommand(".", 250);
                    }
                    catch (SmtpCmdFailedException e)
                    {
                        // if any command failed, reset the session
                        sendCommand("RSET");
                        throw;
                    }
                }

            // all done


            sendCommand("QUIT", 221);
            }
        public string GetFullResponse()
        {
            //stringBuilder.Append(RecvResponse());
            //stringBuilder.Append("\r\n");
            ////while (HaveNextResponse())
            ////{
            stringBuilder.Append(RecvResponse());
            stringBuilder.Append("\r\n");
          //  }
            return stringBuilder.ToString();
        }

        public bool HaveNextResponse()
        {
            if (GetResponseState() > -1)
            {
                if (resp.Length >= 4 && resp[3] != ' ')
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        public string RecvResponse()
        {
            if (GetResponseState() != 221)
                if (resp == "sdg")
                {

                    // client.st
                }
                else
                {


                }
            else
                resp = "221 closed!";

            return resp;
        }

        public int GetResponseState()
        {
            if (resp == null) return 0;

            if (resp.Length >= 3 && IsNumber(resp[0]) && IsNumber(resp[1]) && IsNumber(resp[2]))
                state = Convert.ToInt32(resp.Substring(0, 3));

            return state;
        }

        private bool IsNumber(char c)
        {
            return c >= '0' && c <= '9';
        }

    }





}
