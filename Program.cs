using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace UDP_SERVER
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }
        public Program()
        {
            // Bind()
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 5555);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(ipep);

            Console.WriteLine("Server Start");

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = (EndPoint)(sender);

            byte[] _data = new byte[1024];

            // ReceiveFrom()
            server.ReceiveFrom(_data, ref remote);
            Console.WriteLine("{0} : \r\nServar Recieve Data : {1}", remote.ToString(), 
                Encoding.Default.GetString(_data));

            // string --> byte[]
            _data = Encoding.Default.GetBytes("Client SendTo Data");

            // SendTo()
            server.SendTo(_data, _data.Length, SocketFlags.None, remote);

            // Close()
            server.Close();

            Console.WriteLine("Press Any key...");
            Console.ReadLine();
        }
    }
}