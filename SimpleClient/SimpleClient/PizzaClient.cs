using System.Net.Sockets;
using System.Text;

namespace _0_SimpleClient
{
    internal class PizzaClient
    {
        private const int BUFFER_SIZE = 1024;   // 데이터 송수신 최대 길이
        private const string SERVER_ADDRESS = "127.0.0.1"; // 서버 주소
        private const int SERVER_PORT = 12345; // 서버 포트. 우리 집에 있는 가족 이름과 같은 느낌

        static void Main()
        {
            try
            {   // 클라이언트 소켓 생성
                using TcpClient client = new TcpClient(SERVER_ADDRESS, SERVER_PORT);
                NetworkStream stream = client.GetStream();

                // 무한반복문
                while (true)
                {
                    Console.Write("How many pizzas do you want? ");
                    string? order = Console.ReadLine();                     // 피자 주문 입력

                    // 문자열이 비어있거나 널일 경우 탈출
                    if (string.IsNullOrEmpty(order))
                        break;

                    byte[] dataToSend = Encoding.UTF8.GetBytes(order);      // 바이트 변환
                    stream.Write(dataToSend, 0, dataToSend.Length);         // 데이터 전송

                    // 1024 크기로 버퍼생성
                    byte[] buffer = new byte[BUFFER_SIZE];
                    int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);    // 데이터 수신
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimEnd();      // 바이트 변환

                    Console.WriteLine($"Server replied '{response}'");      // 주문 결과 출력
                }
                // 문자열이 비어있거나 널일 경우 출력
                Console.WriteLine("Client closing");
            }
            catch (SocketException ex)
            {
                // 소켓 에러 디버그
                Console.WriteLine($"Connection error: {ex.Message}");
            }
        }
    }
}