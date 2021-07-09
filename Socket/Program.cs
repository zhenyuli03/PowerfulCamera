using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Socket1
{

    public class Program
    {
        static void Main(string[] args)
        {
            //int[] array = new int[3] { 3, 4, 5 };
            //var d = array[0..2];
        }
        public static void Listener()
        {

            // 构建Socket实例、设置端口号和监听队列大小
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string host = "127.0.0.1";  //指定ip
            int port = 1234;            //指定端口
            listener.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            listener.Listen(5);
            Console.WriteLine("Waiting for connect...");
            // 进入死循环，等待新的客户端连入。一旦有客户端连入，就分配一个Task去做专门处理。然后自己继续等待。
            Task.Run(() =>
            {
                while (true)
                {
                    var clientExecutor = listener.Accept();
                    Task.Factory.StartNew(() =>
                    {
                        // 获取客户端信息，C#对(ip+端口号)进行了封装。
                        var remote = clientExecutor.RemoteEndPoint;
                        Console.WriteLine("Accept new connection from {0}", remote);
                        // 发送一个欢迎消息
                        clientExecutor.Send(Encoding.UTF32.GetBytes("Welcome"));
                        // 进入死循环，读取客户端发送的信息
                        var bytes = new byte[1024];
                        while (true)
                        {
                            var count = clientExecutor.Receive(bytes);
                            //var msg = Encoding.UTF32.GetString(bytes, 0, count);
                            var msg = Encoding.UTF8.GetString(bytes, 0, count);
                            if (msg == "exit")
                            {
                                System.Console.WriteLine("{0} request close", remote);
                                break;
                            }
                            Console.WriteLine("{0}: {1}", remote, msg);
                            Array.Clear(bytes, 0, count);
                        }
                        clientExecutor.Close();
                        System.Console.WriteLine("{0} closed", remote);
                    });
                }
            });
            Console.WriteLine($"桌面:{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}");
            Console.ReadKey();
        }

        public static void Speaker()
        {
            var host = "127.0.0.1";
            var port = 1000;
            // 构建一个Socket实例，并连接指定的服务端。这里需要使用IPEndPoint类(ip和端口号的封装)
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            var bytes = new byte[1024];
            var count = client.Receive(bytes);
            Console.WriteLine("New message from server: {0}", Encoding.UTF32.GetString(bytes, 0, count));

            var input = "";
            while (input != "exit")
            {
                input = Console.ReadLine();
                //client.Send(Encoding.UTF32.GetBytes(input));
                client.Send(Encoding.UTF8.GetBytes(input));
            }
            client.Close();
        }
    }
}
