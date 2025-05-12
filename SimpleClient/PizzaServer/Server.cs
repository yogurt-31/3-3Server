using System.Net.Sockets;
using System.Net;
using System.Text;

internal class Server
{
    private const int BUFFER_SIZE = 1024;
    // IP주소, 포트번호
    private static readonly IPEndPoint ADDRESS = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
    private TcpListener serverSocket;

    public Server()     // 서버 소켓 생성
    {
        try // 시도해 볼게요
        {
            Console.WriteLine($"Starting up at: {ADDRESS}");
            serverSocket = new TcpListener(ADDRESS);
            // 서버 시작
            serverSocket.Start();
        }
        catch (SocketException) // 아차차 예외
        {
            Console.WriteLine("\nServer failed to start.");
            // 소켓이 null이 아닐 경우에 서버 중단
            serverSocket?.Stop();
        }
    }

    public TcpClient Accept()   // 클라이언트 서버 접속 대기 및 클라이언트 소켓 반환
    {
        // 클라이언트 수락
        TcpClient client = serverSocket.AcceptTcpClient();
        // 클라이언트 IP주소 및 포트번호 훔치기
        IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        Console.WriteLine($"Connected to {clientEndPoint}");
        // 클라이언트를 반환
        return client;
    }

    public void Serve(TcpClient client)     // 클라가 보내는 데이터 수신 및 서버의 응답 송신
    {
        // 데이터를 송수신하기 위한 스트림
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[BUFFER_SIZE];

        try // 시도해볼게요
        {
            while (true)
            {
                // 클라이언트가 보낸 데이터 읽기
                int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);
                if (bytesRead == 0) break;

                // 그걸 문자열로 전환.
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string response;

                // 문자열에 있는 정수를 캐오는 것
                if (int.TryParse(receivedData, out int order))
                {
                    response = $"Thank you for ordering {order} pizzas!\n";
                }
                else
                {
                    response = "Wrong number of pizzas, please try again\n";
                }

                Console.WriteLine($"Sending message to {client.Client.RemoteEndPoint}");
                // 위에서 완성된 정수 캐오는것에 문자열을 또 만들고
                // 그걸 또 보내기 위해서 바이트로 변환
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                // 바이트 전송
                stream.Write(responseData, 0, responseData.Length);
            }
        }
        finally // 예외가 발생하든 말든 어쨌든 실행함.
        {
            Console.WriteLine($"Connection with {client.Client.RemoteEndPoint} has been closed");
            client.Close();
        }
    }

    public void Start() // 클라 연결 대기를 위한 loop문
    {
        Console.WriteLine("Server listening for incoming connections");

        try
        {
            while (true)
            {
                // 클라이언트 수락
                // 문제는 Serve라는 메서드에서 무한반복문을 돌기 때문에 단일 클라이언트만 접속할 수 있음
                // 딴 클라이언트 받을라면 앞 클라이언트가 나갈 때까지 기다려야함
                TcpClient client = Accept();
                Serve(client);
            }
        }
        finally
        {
            serverSocket.Stop();
            Console.WriteLine("\nServer stopped.");
        }
    }

    static void Main(string[] args) // Main문 서버 시작
    {
        // 서버 생성 및 시작
        Server server = new Server();
        server.Start();
    }
}