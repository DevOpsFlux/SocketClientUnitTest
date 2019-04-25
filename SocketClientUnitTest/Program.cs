/*
'	프로그램명	: SocketClientUnitTest
'	작성자		: DevOpsFlux
'	작성일		: 2019-01-17
'	설명		: SocketClient Library Test
'       https://trip2ee.tistory.com/23
'       https://sunyzero.tistory.com/198
'       http://egloos.zum.com/yajino/v/782519
'       https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=netframework-4.8
'       https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=netframework-4.8
*/
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace SocketClientUnitTest
{
    class Program
    {
        //[SND]004020010171950456                      E
        //[RCV]00402001117195045617195108400success성공E

        private static string strIP = "127.0.0.1";
        private static string strPort = "17001";

        private static int nTimeoutSecond = 5;
        private static string strSendMsg = "004020010171950456                      ";

        
        static void Main(string[] args)
        {
            // # TCPClient
            SendTCPClient();

            Console.WriteLine("");

            // # SocketClient
            //SendSocketClient();

            Console.WriteLine("");

            // # NetAsioClient
            //SendNetAsioClient();
            
        }

        #region # SendSocketClient
        private static void SendSocketClient()
        {
            string strReciveMsg = "";
            int nRecieve = 0;
            byte[] sb = new byte[1024];
            byte[] rb = new byte[1024];

            Socket client = null;
            IPEndPoint ServerIPEndPoint = null;

            try
            {
                Console.WriteLine("# SendSocketClient START");

                LingerOption lingerOption = new LingerOption(true, 0);

                ServerIPEndPoint = new IPEndPoint(IPAddress.Parse(strIP), Convert.ToInt32(strPort));
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //client.LingerState = lingerOption;
                //client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 0);
                client.SendTimeout = nTimeoutSecond;
                client.ReceiveTimeout = nTimeoutSecond;
                client.Connect(ServerIPEndPoint);

                Console.WriteLine(string.Format("[SND]{0}E", strSendMsg));

                //sb = System.Text.Encoding.Unicode.GetBytes(strSendMsg);
                //sb = System.Text.Encoding.ASCII.GetBytes(strSendMsg);
                sb = System.Text.Encoding.GetEncoding("euc-kr").GetBytes(strSendMsg);
                
                client.Send(sb, 0, sb.Length, SocketFlags.None);

                nRecieve = client.Receive(rb, 0, rb.Length, SocketFlags.None);
                
                if (nRecieve > 0)
                {
                    strReciveMsg = System.Text.Encoding.GetEncoding("euc-kr").GetString(rb, 0, nRecieve);
                    //strReciveMsg = System.Text.Encoding.ASCII.GetString(rb, 0, nRecieve);
                    //strReciveMsg = System.Text.Encoding.UTF8.GetString(rb, 0, nRecieve);
                    //strReciveMsg = System.Text.Encoding.Unicode.GetString(rb, 0, nRecieve);

                    Console.WriteLine(string.Format("[RCV]{0}E", strReciveMsg));
                }

                // 옵션 : TIME_WAIT 없애기
                // 참고 : http://egloos.zum.com/yajino/v/782519
                sb = System.Text.Encoding.GetEncoding("euc-kr").GetBytes("BYE");
                client.Send(sb, 0, sb.Length, SocketFlags.None);

            }
            catch (SocketException ex)
            {
                Console.WriteLine(string.Format("SocketException : {0}", ex.ToString()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception : {0}", ex.ToString()));
            }
            finally
            {
                client.Shutdown(SocketShutdown.Both);
                client.Disconnect(true);
                client.Close();

                //Console.WriteLine(client.Connected);


                Console.WriteLine("END");
            }
        }
        #endregion

        #region # SendTCPClient
        private static void SendTCPClient()
        {
            string strReciveMsg = "";
            int nRecieve = 0;
            byte[] sb = new byte[1024];
            byte[] rb = new byte[1024];

            TcpClient client = null;
            NetworkStream stream = null;

            try
            {
                Console.WriteLine("# SendTCPClient START");

                LingerOption lingerOption = new LingerOption(false, 0);
                client = new TcpClient(strIP, Convert.ToInt32(strPort));
                client.LingerState = lingerOption;
                client.ReceiveTimeout = nTimeoutSecond;
                client.SendTimeout = nTimeoutSecond;

                Console.WriteLine(string.Format("[SND]{0}E", strSendMsg));

                //sb = System.Text.Encoding.Unicode.GetBytes(strSendMsg);
                //sb = System.Text.Encoding.ASCII.GetBytes(strSendMsg);
                sb = System.Text.Encoding.GetEncoding("euc-kr").GetBytes(strSendMsg);
                
                stream = client.GetStream();
                stream.Write(sb, 0, sb.Length);

                nRecieve = stream.Read(rb, 0, rb.Length);

                if (nRecieve > 0)
                {
                    //strReciveMsg = System.Text.Encoding.ASCII.GetString(rb, 0, nRecieve);
                    strReciveMsg = System.Text.Encoding.GetEncoding("euc-kr").GetString(rb, 0, nRecieve);
                    //strReciveMsg = System.Text.Encoding.UTF8.GetString(rb, 0, nRecieve);
                    //strReciveMsg = System.Text.Encoding.Unicode.GetString(rb, 0, nRecieve);

                    Console.WriteLine(string.Format("[RCV]{0}E", strReciveMsg));
                }

                sb = System.Text.Encoding.GetEncoding("euc-kr").GetBytes("BYE");
                stream.Write(sb, 0, sb.Length);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(string.Format("SocketException : {0}", ex.ToString()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception : {0}", ex.ToString()));
            }
            finally
            {
                stream.Close();
                client.Close();
                
                Console.WriteLine(client.Connected);
                

                Console.WriteLine("END");
            }
        }
        #endregion

        #region # SendNetAsioClient
        private static void SendNetAsioClient()
        {
            string strReciveMsg = "";
            int nRecieve = 0;
            byte[] sb = new byte[1024];

            try
            {
                Console.WriteLine("# SendNetAsioClient START");

                nRecieve = SendPacket(strIP, strPort, nTimeoutSecond, strSendMsg, sb);

                Console.WriteLine(string.Format("[SND]{0}E", strSendMsg));

                if (nRecieve > 0)
                {
                    //strReciveMsg = Encoding.Unicode.GetString(sb, 0, nRecieve);
                    strReciveMsg = Encoding.GetEncoding("euc-kr").GetString(sb, 0, nRecieve);
                    //strReciveMsg = Encoding.UTF8.GetString(sb, 0, nRecieve);
                    Console.WriteLine(string.Format("[RCV]{0}E", strReciveMsg));
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(string.Format("SocketException : {0}", ex.ToString()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception : {0}", ex.ToString()));
            }
            finally
            {
                Console.WriteLine("END");
            }
        }

        [DllImport("NetAsioClient.dll", CharSet = CharSet.Ansi)]
        private static extern int SendPacket(
            string strIP, string nPort, int nElapse,
            string szMessage,
            [MarshalAs(UnmanagedType.LPArray)] byte[] sb
        );

        #endregion
        
    }
}
