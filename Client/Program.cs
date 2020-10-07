using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] buffer = new byte[1024];
            int count;
            Socket socket;

            Console.WriteLine("Введите имя пользователя");
            while (true)
            {
                try
                {
                    string name = Console.ReadLine();

                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect("127.0.0.1", 1023);

                    socket.Send(Encoding.UTF8.GetBytes('\0' + name));
                    count = socket.Receive(buffer);

                    if (count == 1 && buffer[0] == 1)
                        break;
                }
                catch (SocketException)
                {
                    Console.WriteLine("Ошибка ввода. Введите еще раз");
                }
            }

            Task.Run(() => MassegeHandler(socket));

            Console.WriteLine("Для выхода из чата нажмите Ctrl+C");
            Console.CancelKeyPress += (s, e) => 
            {
                socket.Disconnect(false);
                Environment.Exit(0);
            };

            string message;
            while(true)
            {
                message = Console.ReadLine();
                if(message != null)
                    socket.Send(Encoding.UTF8.GetBytes('\0' + message));
            }
        }

        private static void MassegeHandler(Socket socket)
        {
            byte[] buffer = new byte[1024];
            int count;

            try
            {
                while (true)
                {
                    count = socket.Receive(buffer);
                    if (buffer[0] == 0)
                    {
                        socket.Send(buffer);
                        continue;
                    }
                    Console.WriteLine(Encoding.UTF8.GetString(buffer, 1, count - 1));
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Соединение с сервером потеряно");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }
    }
}
