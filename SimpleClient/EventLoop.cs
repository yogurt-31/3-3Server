using System.Net.Sockets;

namespace _4_Pizza_Event
{
    internal class EventLoop
    {
        // 읽기 쓰기 딕셔너리
        private readonly Dictionary<Socket, Action<Socket>> readers = new();
        private readonly Dictionary<Socket, Action<Socket>> writers = new();
        // "소켓을 읽을 수 있을 때 알려줘" 목록에 등록
        public void RegisterRead(Socket socket, Action<Socket> callback)
        {
            // 읽기 딕셔너리에 데이터 저장
            readers[socket] = callback;
        }
        // "소켓을 쓸 수 있을 때 알려줘" 목록에 등록
        public void RegisterWrite(Socket socket, Action<Socket> callback)
        {
            // 쓰기 딕셔너리에 데이터 저장
            writers[socket] = callback;
        }
        // 	소켓이 종료되면 등록 해제
        public void Unregister(Socket socket)
        {
            // 딕셔너리 데이터 삭제
            readers.Remove(socket);
            writers.Remove(socket);
        }
        // 이벤트 루프 돌며 Select()로 가능한 작업 감지 후 콜백 실행
        public void RunForever()
        {
            while (true)
            {
                var readList = new List<Socket>(readers.Keys);  // 읽기 가능 소켓 목록
                var writeList = new List<Socket>(writers.Keys); // 쓰기 가능 소켓 목록
                Socket.Select(readList, writeList, null, 1000); // 읽/쓸 수 있는 상태가 된 소켓 선별해 list에 반환

                // 읽을 리스트의 반복문
                foreach (var sock in readList)
                {
                    // readers에 들어있는 값과 sock의 값이 동일한 값을 찾고, 그 Key값에 저장돼있는 callback(Action)을 반환.
                    if (readers.TryGetValue(sock, out var callback))
                    {
                        callback(sock); // 예: OnRead 호출
                    }
                }

                // 쓸 리스트의 반복문
                foreach (var sock in writeList)
                {
                    // writers에 들어있는 값과 sock의 값이 동일한 값을 찾고, 그 Key값에 저장돼있는 callback(Action)을 반환.
                    if (writers.TryGetValue(sock, out var callback))
                    {
                        callback(sock); // 예: OnWrite 호출
                    }
                }
            }
        }
    }
}