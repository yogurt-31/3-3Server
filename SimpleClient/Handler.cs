using System.Net.Sockets;
using System.Net;
using System.Text;

namespace PizzaThread
{
    internal class Handler
    {
        private const int BUFFER_SIZE = 1024;
        private readonly TcpClient client;

        public Handler(TcpClient client)    // 각 스레드마다 클라이언트 소켓을 부여받는다.
        {
            this.client = client;
        }

        public void Run()   // 스레드마다 서버의 역할 수행
        {
            // 클라이언트 가져오기
            IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"Connected to {clientEndPoint}");

            // 주소 포트 등등 가져오기
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[BUFFER_SIZE];

            try
            {
                while (true)
                {
                    // 클라이언트가 보낸 데이터 읽기
                    int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);
                    if (bytesRead == 0) break;

                    // 읽은 데이터를 문자열로 변환
                    string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string response;

                    // 만약 문자열에 숫자가 포함되어 있을 경우
                    // 있으면 order에 값을 넣음
                    if (int.TryParse(received, out int order))
                    {
                        response = $"Thank you for ordering {order} pizzas!\n";
                    }
                    else
                    {
                        response = "Wrong number of pizzas, please try again\n";
                    }

                    Console.WriteLine($"Sending message to {clientEndPoint}");
                    // 위에서 열심히 만든 문자열을 다시 전송하기 위해 바이트로 변환
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    // 클라이언트에게 데이터 보내기
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
            finally
            {
                Console.WriteLine($"Connection with {clientEndPoint} has been closed");
                client.Close();
            }
        }
    }

    internal class ThreadServer
    {
        private const int PORT = 12345;
        private readonly TcpListener server;

        public ThreadServer()   // 서버 소켓 세팅
        {
            try
            {
                // 주소 및 포트번호로 서버 시작
                IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);
                Console.WriteLine($"Starting up at: {localAddress}");
                server = new TcpListener(localAddress);
                server.Start();
            }
            catch (SocketException)
            {
                // 서버가 널이 아니면 중단
                server?.Stop();
                Console.WriteLine("\nServer stopped.");
            }
        }

        public void Start()
        {
            Console.WriteLine("Server listening for incoming connections");

            try
            {
                while (true)
                {
                    // 클라이언트 수락
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine($"Client connection request from {client.Client.RemoteEndPoint}");

                    // 핸들러 클래스 생성
                    Handler handler = new Handler(client);
                    // 스레드를 만들어서 handler의 Run 메서드를 실행함.
                    Thread thread = new Thread(new ThreadStart(handler.Run));
                    thread.Start();
                } // 클라이언트 요청이 들어올 때마다 새로운 스레드를 생성한다.
            }
            finally
            {
                // 서버 종료
                server.Stop();
                Console.WriteLine("\nServer stopped.");
            }
        }
        static void Main()
        {
            // 스레드 서버 시작
            ThreadServer server = new ThreadServer();
            server.Start();
        }
    }
}