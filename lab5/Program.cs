using System;
using System.Net.Sockets;
using System.Collections;

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
		public void ConnectPOP(string sServerName, string sUserName, string sPassword)
		{
			string sMessage;
			string sResult;
			Connect (sServerName, 995);
			sResult = Response ();
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sResult);
			sMessage = "USER " + sUserName + "\r\n";
			Write (sMessage);
			sResult = Response ();
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sResult);
			sMessage = "PASS " + sPassword + "\r\n";
			Write (sMessage);
			sResult = Response ();
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sResult);
		}

		public void DisconnectPOP() {
			string sMessage;
			string sResult;
			sMessage = "QUIT\r\n";
			Write (sMessage);
			sResult = Response ();
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sMessage);
		}

		public ArrayList ListMessages() {
			string sMessage;
			string sResult;
			ArrayList returnValue = new ArrayList ();
			sMessage = "LIST\r\n";
			Write (sMessage);
			sResult = Response ();
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sMessage);
			while (true) {
				sResult = Response ();
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
			POP3EmailMessage oMailMessage = new POP3EmailMessage ();
			oMailMessage.msgSize = msgRetr.msgSize;
			oMailMessage.msgNumber = msgRetr.msgNumber;
			sMessage = "RETR" + msgRetr.msgNumber + "\r\n";
			Write (sMessage);
			sResult = Response ();
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sMessage);
			oMailMessage.msgReceived = true;
			while (true) {
				sResult = Response ();
				if (sResult == ".\r\n")
					break;
				else
					oMailMessage.msgContent = sResult;
			}
			return oMailMessage;
		}

		public void DeleteMessage(POP3EmailMessage msgDele) {
			string sMessage;
			string sResult;
			sMessage = "DELE" + msgDele.msgNumber + "\r\n";
			Write (sMessage);
			sResult = Response ();
			if (sResult.Substring (0, 3) != "+OK")
				throw new POPException (sMessage);
		}

		public void Write(string sMessage) {
			System.Text.ASCIIEncoding oEncodedData = new System.Text.ASCIIEncoding ();
			byte[] WriteBuffer = new byte[1024];
			WriteBuffer = oEncodedData.GetBytes (sMessage);
			NetworkStream NetStream = GetStream ();
			NetStream.Write (WriteBuffer, 0, WriteBuffer.Length);
		}

		private string Response() {
			System.Text.ASCIIEncoding oEncodedData = new System.Text.ASCIIEncoding ();
			byte[] ServerBuffer = new byte[1024];
			NetworkStream NetStream = GetStream ();
			int count = 0;
			while (true) {
				byte[] buff = new byte[2];
				int bytes = NetStream.Read (buff, 0, 1);
				if (bytes == 1) {
					ServerBuffer [count] = buff [0];
					count++;
					if (buff [0] == '\n')
						break;
				} else
					break;
			}
			string ReturnValue = oEncodedData.GetString (ServerBuffer, 0, count);
			return ReturnValue;
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
				Console.Write("Username: ");
				user = Console.ReadLine();
				Console.Write("Password: ");
				pass = Console.ReadLine();
				//cons read user pass
				oPOP.ConnectPOP("pop.yandex.ru", user, pass);
				ArrayList MessageList = oPOP.ListMessages();
				foreach (POP3EmailMessage POPMsg in MessageList) {
					POP3EmailMessage POPMsgContent = oPOP.RetrieveMessage(POPMsg);
					System.Console.WriteLine("Message {0}: {1}",
						POPMsgContent.msgNumber, POPMsgContent.msgContent);
				}
				oPOP.DisconnectPOP();
			}
			catch(POPException e) {
				System.Console.WriteLine (e.ToString ());
			}
			catch(System.Exception e) {
				System.Console.WriteLine (e.ToString());
			}

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