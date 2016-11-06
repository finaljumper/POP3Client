using System;
using System.Net.Sockets;
using System.Collections;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Globalization;

namespace lab5
{
	public class POP3EmailMessage {
		public long msgNumber;
		public long msgSize;
		public bool msgReceived;
		public string msgContent;
	}

	public class POP3:TcpClient
	{
        public SslStream sslStream;
        public void ConnectPOP(string sServerName, string sUserName, string sPassword)
		{
			string sMessage;
			string sResult;
			Connect (sServerName, 995);
            sslStream = new SslStream(GetStream());
            // The server name must match the name on the server certificate.
            try
            {
                sslStream.AuthenticateAsClient(sServerName);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                Close();
                return;
            }
            sResult = Response (sslStream);
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sResult);
			sMessage = "USER " + sUserName + "\r\n";
			Write (sMessage, sslStream);
			sResult = Response (sslStream);
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sResult);
			sMessage = "PASS " + sPassword + "\r\n";
			Write (sMessage, sslStream);
			sResult = Response (sslStream);
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sResult);
		}

		public void DisconnectPOP() {
			string sMessage;
			string sResult;
			sMessage = "QUIT\r\n";
			Write (sMessage, sslStream);
			sResult = Response (sslStream);
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sMessage);
		}

		public ArrayList ListMessages() {
			string sMessage;
			string sResult;
			ArrayList returnValue = new ArrayList ();
			sMessage = "LIST\r\n";
			Write (sMessage, sslStream);
			sResult = Response (sslStream);
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sMessage);
			while (true) {
				sResult = Response (sslStream);
				if (sResult == ".\r\n")
					return returnValue;
				else {
					POP3EmailMessage oMailMessage = new POP3EmailMessage ();

					char[] sep = {' '};
					string[] values = sResult.Split (sep);
					oMailMessage.msgNumber = Int32.Parse (values [0]);
					oMailMessage.msgSize = Int32.Parse (values [1]);
					oMailMessage.msgReceived = false;
					returnValue.Add (oMailMessage);
					continue;
				}
			}
		}

		public POP3EmailMessage RetrieveMessage(POP3EmailMessage msgRetr) {
			string sMessage;
			string sResult;
            int result = 0;
			POP3EmailMessage oMailMessage = new POP3EmailMessage ();
			oMailMessage.msgSize = msgRetr.msgSize;
			oMailMessage.msgNumber = msgRetr.msgNumber;
			sMessage = "RETR " + msgRetr.msgNumber + "\r\n";
			Write (sMessage, sslStream);
			sResult = Response (sslStream);
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sMessage);
			oMailMessage.msgReceived = true;
			while (true) {
				sResult = Response (sslStream);
                if (sResult.Contains(".\r\n"))
                    break;
                else
                {
                    if (sResult.Contains("Date: "))
                    {
                        DateTime parsedDate;
                        string newDate = sResult.Remove(0, 6);
                        newDate = newDate.Substring(0, 32);
                        parsedDate = DateTime.Parse(newDate);
                        DateTime yesterday = DateTime.Now;
                        yesterday = yesterday.AddHours(-24);
                        result = DateTime.Compare(yesterday, parsedDate);
                    }
                    oMailMessage.msgContent += sResult;
                }
			}
            if (result > 0)
                return null;
            else
                return oMailMessage;
		}


		public void Write(string sMessage, SslStream NetStream) {
			System.Text.UTF8Encoding oEncodedData = new System.Text.UTF8Encoding ();
			byte[] WriteBuffer = new byte[1024];
			WriteBuffer = oEncodedData.GetBytes (sMessage);
			NetStream.Write (WriteBuffer, 0, WriteBuffer.Length);
		}

		private string Response(SslStream NetStream) {
			byte[] ServerBuffer = new byte[1024];
            int count = 0;
            int bytes = -1;
            bytes = sslStream.Read(ServerBuffer, 0, ServerBuffer.Length);
            return Encoding.UTF8.GetString(ServerBuffer, 0, bytes);
        }


	}

	public class POPException : System.ApplicationException
	{
		public POPException(string str) : base(str)
		{
		}
	}

	class MainClass
	{
		public static void Main (string[] args)
		{
			try
			{
				POP3 oPOP = new POP3();
				string user, pass;
                Console.WriteLine("Connecting pop.yandex.ru...");
                //user = "potatoe2016@yandex.ru";
                //pass = "qwerty123456";
                Console.WriteLine("Удостоверьтесь, что POP3 включен в натройках почты.");
                Console.WriteLine("Введите логин на yandex mail:");
                user = Console.ReadLine();
                Console.WriteLine("Введите пароль:");
                pass = Console.ReadLine();
                oPOP.ConnectPOP("pop.yandex.ru", user, pass);
                Console.WriteLine("Login successful.");
                Console.WriteLine("Retrieving messages.");
                ArrayList MessageList = oPOP.ListMessages();
				foreach (POP3EmailMessage POPMsg in MessageList) {
					POP3EmailMessage POPMsgContent = oPOP.RetrieveMessage(POPMsg);
                    if (POPMsgContent == null)
                        break;
					System.Console.WriteLine("Message {0}: {1}", POPMsgContent.msgNumber, POPMsgContent.msgContent);
                }
				oPOP.DisconnectPOP();
			}
			catch(POPException e) {
				System.Console.WriteLine (e.ToString ());
			}
			catch(System.Exception e) {
				System.Console.WriteLine (e.ToString());
			}
            Console.WriteLine("Это все сообщения за последние 24 часа.");
            Console.ReadLine();
        }
	}
}


namespace POPEmailException
{
	public class POPException : System.ApplicationException
	{
		public POPException(string str) : base(str)
		{
		}
	}
}
