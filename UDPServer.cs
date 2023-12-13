using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class UDPServer
    {
        private IPEndPoint ipEndPoint;
        private Socket server;
        private int port;
        private LinkedList<EndPoint> observerList = new LinkedList<EndPoint>();
        private uint observerCnt = 0;

        private const byte SERVER_REG_PAYLOAD     = 0x01;
        private const byte RT_PRICE_SUB_PAYLOAD   = 0x10;
        private const byte RT_PRICE_UNSUB_PAYLOAD = 0x20;
        private const byte SERVER_DEREG_PAYLOAD   = 0xFF;
        
        public void Initialize(int port=5555)
        {
            // Bind()
            ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(ipEndPoint);
            
            Console.WriteLine("Server Initialized");
        }
        public void run()
        {
            Console.WriteLine("Server Start");
            Task.Run(() => RegistrationObserver());
            Task.Run(() => updateStockInformation());
        }
        public void RegistrationObserver()
        {
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)(sender);

                byte[] data = new byte[4096];

                server.ReceiveFrom(data, ref remote);

                if (data[0] == SERVER_REG_PAYLOAD)
                {
                    observerList.AddLast(remote);
                    observerCnt++;
                    Console.WriteLine("옵저버 등록");
                }
                if (data[0] == SERVER_DEREG_PAYLOAD)
                {
                    /* 옵저버 삭제 기능 구현 필요 */
                    observerCnt--;
                }
            }
        }
        public void Notify(IPEndPoint to, byte[] payload)
        {
            Console.WriteLine($"전달할 가격-${Encoding.UTF8.GetString(payload)}");
            server.SendTo(payload, payload.Length, SocketFlags.None, to);
        }

        public void NotifyAll(byte[] payload)
        {
            foreach (IPEndPoint observer in observerList)
            {
                Notify(observer, payload);
            }
        }
        public async void updateStockInformation()
        {
            Random random = new Random();
            int closePrice = 10000;

            while (true)
            {
                await Task.Delay(random.Next(10, 100));                         // 100ms에서 1000ms(1초) 사이의 랜덤한 시간 간격 설정
                
                closePrice -= random.Next(-5, 5) % 10;                          // 가격 랜덤으로 등락
                
                NotifyAll(Encoding.Default.GetBytes(closePrice.ToString()));    // 모든 observer 에게 전달

                if (closePrice < 5000) closePrice += 1000;
                if (closePrice > 15000) closePrice -= 1000;
            }
        }   
        public void Close()
        {
            NotifyAll(Encoding.Default.GetBytes("Server shutdown..."));
            server.Close();
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            UDPServer server = new UDPServer();
            
            server.Initialize();
            server.run();

            await Task.Delay(-1);                                       // 작업이 완료될 때까지 대기
        }
    }
}