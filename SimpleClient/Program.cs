namespace _4_Pizza_Event
{
    internal class Program
    {
        static void Main()
        {
            // 이벤트 루프와 서버를 생성 후 시작
            EventLoop eventLoop = new EventLoop();
            EventServer server = new EventServer(eventLoop);
            server.Start();
            eventLoop.RunForever();
        }
    }
}