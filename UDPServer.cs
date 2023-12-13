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
            Console.WriteLine($"[PUSH] {to.ToString()} >> {Encoding.ASCII.GetString(payload)}");
            server.SendTo(payload, payload.Length, SocketFlags.None, to);
        }

        public void NotifyAll(byte[] payload)
        {
            foreach (IPEndPoint observer in observerList)
            {
                Notify(observer, payload);
            }
        }
        int test = 10000;
        private byte[] CreateTransactionLedger(string ticker, int closePrice, long tradeVol)
        {
            byte[] payload = new byte[4096];

            // 종목코드 string -> byte[]
            byte[] tickerBytes = Encoding.ASCII.GetBytes(ticker);

            // 종가 int -> byte[] 
            byte[] closePriceBytes = BitConverter.GetBytes(test++);
            
            // 거래량 int -> byte[] 
            byte[] tradeVolBytes = BitConverter.GetBytes(tradeVol);

            // 시간값 byte[] 구하는 부분
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;                 // 현재 시간값을 가져옴
            long unixTimeMilliseconds = currentTime.ToUnixTimeMilliseconds();   // Unix 시간 Milisec 으로 가져옴
            ulong unixTimeUlong = (ulong)unixTimeMilliseconds;                  // Milisec Unix 시간 ulong 으로 변환
            byte[] unixTimeBytes = BitConverter.GetBytes(unixTimeUlong);        // ulong 값을 바이트 배열로 변환

            /*
                Array.Copy(
                    sourceArray,        // 복사할 요소들이 있는 원본 배열
                    sourceIndex,        // 복사를 시작할 소스 배열 내의 시작 인덱스
                    destinationArray,   // 복사될 대상 배열
                    destinationIndex,   // 대상 배열 내의 복사가 시작될 위치
                    length              // 복사될 요소의 개수
                );
            */
            Array.Copy(tickerBytes, 0, payload, 0, tickerBytes.Length);         // str[7]   0~7
            Array.Copy(closePriceBytes, 0, payload, 7, closePriceBytes.Length); // int      7~11
            Array.Copy(tradeVolBytes, 0, payload, 11, tradeVolBytes.Length);    // long     11~19
            Array.Copy(unixTimeBytes, 0, payload, 19, unixTimeBytes.Length);    // long     19~27
        
            return payload;
        }
        public async void updateStockInformation()
        {
            Random random = new Random();
            int closePrice = 10000;
            int tradeVol = 0;

            while (true)
            {
                await Task.Delay(random.Next(10, 100));                                 // 100ms에서 1000ms(1초) 사이의 랜덤한 시간 간격 설정
                
                closePrice -= random.Next(-5, 5);                                       // 가격 랜덤으로 등락
                tradeVol += random.Next(10, 1000);                                      // 거래량 랜덤으로 증가
                
                NotifyAll(CreateTransactionLedger("A163730", closePrice, tradeVol));    // 모든 observer 에게 전달

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