using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static Dictionary<Socket, string> users = new Dictionary<Socket, string>();
        static TcpListener listener= new TcpListener(IPAddress.Parse("127.0.0.1"), 1023);

        static void Main(string[] args)
        {
            Console.WriteLine("Запуск сервера");

            listener.Start();
            Task task = new Task(SocketHandler);
            task.Start();

            while (true)
            {
                string stop = Console.ReadLine();
                switch (stop.ToLower())
                {
                    case "stop":
                        listener.Stop();
                        return;
                    case "users":
                        Console.WriteLine(string.Join( Environment.NewLine, users.Values.ToArray()));
                        break;
                    case "clean":
                        Console.Clear();
                        break;
                }
            }
        }

        private static void SocketHandler()
        {
            while (true)
            {
                Socket s = listener.AcceptSocket();
                Task.Run(() => MessageHandler(s) );
            }
        }

        private static void MessageHandler(Socket socket)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int count = socket.Receive(buffer);

                string name = Encoding.UTF8.GetString(buffer, 1, count - 1);

                if (users.ContainsValue(name))
                {
                    socket.Disconnect(false);
                    Console.WriteLine($"Совпадение имени {name}");
                    return;
                }

                Console.WriteLine($"Подключился пользователь {name}");
                foreach (Socket s in users.Keys)
                    s.Send(Encoding.UTF8.GetBytes('$' + $"Подключился пользователь {name}"));
                socket.Send(new byte[] { 1 });

                users.Add(socket, name);
                while (true) 
                {
                    count = socket.Receive(buffer);
                    if (buffer[0] != 0)
                    {
                        socket.Send(buffer, count, SocketFlags.None);
                        continue;
                    }

                    string message = '$' + name + ": " + Encoding.UTF8.GetString(buffer, 1, count - 1);
                    Console.WriteLine($"Сообщение от {message.Remove(0,1)}");

                    foreach (Socket s in users.Keys)
                        s.Send(Encoding.UTF8.GetBytes(message));
                }
            }
            catch (SocketException)
            {
                users.Remove(socket);
            }
        }
    }
}
