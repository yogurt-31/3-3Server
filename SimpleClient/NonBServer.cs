using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _3_Pizza_NonB
{
    internal class NonBServer
    {
        private const int BUFFER_SIZE = 1024;
        private const int PORT = 12345;
        private Socket serverSocket;
        private readonly List<Socket> clients = new();

        public NonBServer()
        {
            try
            {
                Console.WriteLine($"Starting up at: 127.0.0.1:{PORT}");

                // 서버 소켓 생성. TCP 통신을 위한 소켓을 만듦
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // 소켓을 특정 IP 주소와 포트에 연결
                serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, PORT));
                serverSocket.Listen(1000);      // 클라이언트 소켓 대기열 최대 1000 
                serverSocket.Blocking = false;  // 서버 소켓 : 블로킹 off!
            }
            catch (SocketException)
            {
                serverSocket?.Close();
                Console.WriteLine("\nServer failed to start.");
            }
        }

        private void Accept()
        {
            try
            {
                // 클라이언트 소켓 수락
                Socket clientSocket = serverSocket.Accept();
                clientSocket.Blocking = false;      // 클라이언트소켓 : 블로킹 off!
                // 클라이언트 리스트에 삽입
                clients.Add(clientSocket);
                Console.WriteLine($"Connected to {clientSocket.RemoteEndPoint}");
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
            {
                // no client waiting to be accepted — just continue
            }
        }

        private void Serve(Socket client)
        {
            byte[] buffer = new byte[BUFFER_SIZE];

            try
            {
                // 클라이언트의 버퍼를 받음
                // bytesRead는 바이트 수
                int bytesRead = client.Receive(buffer);
                if (bytesRead == 0)
                {
                    clients.Remove(client);
                    client.Close();
                    return;
                }

                // 받은 버퍼를 문자열로 변환
                string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string response;

                // 문자열 안에 숫자가 있으면 order에 넣음
                if (int.TryParse(received, out int order))
                {
                    response = $"Thank you for ordering {order} pizzas!\n";
                }
                else
                {
                    response = "Wrong number of pizzas, please try again\n";
                }

                Console.WriteLine($"Sending message to {client.RemoteEndPoint}");
                // 예쁘게 꾸민 문자열을 바이트로 변환한 수 전송
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                client.Send(responseData);
            }
            // 데이터가 없으면 기다리지 않고 바로 예외를 던짐
            // 이게 논블로킹 서버의 특징임
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
            {
                // no client waiting to be accepted — just continue
            }
        }

        public void Start()
        {
            Console.WriteLine("Server listening for incoming connections");

            try
            {
                while (true)        // 폴링 반복문
                {                   // 입출력 연산(accept(), send(), read() 이 성공할 때까지 연산 반복
                    Accept();       // 만약 소켓에 데이터가 없으면 대기하지 않고 넘어감

                    // 클라이언트 리스트 반복문
                    foreach (var client in new List<Socket>(clients))
                    {
                        Serve(client);  // 만약 소켓에 데이터가 없으면 대기하지 않고 넘어감
                    }

                    Thread.Sleep(1); // avoid 100% CPU usage
                }
            }
            finally
            {
                serverSocket.Close();
                Console.WriteLine("\nServer stopped.");
            }
        }

        static void Main()
        {
            NonBServer server = new NonBServer();
            server.Start();
        }
    }
}