using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Client
    {
        readonly IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
        readonly Message message = new Message();
        readonly UdpClient client;
        static Lazy<Client> lazyClient = new Lazy<Client>(() =>
        {
            Console.Write("Введите имя: ");
            string name = Console.ReadLine()!;
            Console.Write("Введите порт: ");
            string port = Console.ReadLine()!;
            return new Client(name, port);
        });
        public static Client Instance => lazyClient.Value;
        private Client(string name, string port)
        {
            message.Name = name;
            client = new UdpClient(int.Parse(port));
        }

        public async Task ClientReceveAsync()
        {
            try
            {
                while (true)
                {
                    var receiveAnswer = await client.ReceiveAsync();
                    string str = Encoding.UTF8.GetString(receiveAnswer.Buffer);
                    var answer = Message.FromJson(str);
                    Console.WriteLine(answer);
                    if (message.Text == "Close")
                    {
                        Console.WriteLine("Клиент закрывается. Нажмите любую клавишу.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task ClientSendAsync()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Введите имя получателя: ");
                    message.ToName = Console.ReadLine()!;
                    Console.WriteLine("Введите сообщение: ");
                    message.Text = Console.ReadLine()!;
                    message.Date = DateTime.Now;
                    var data = Encoding.UTF8.GetBytes(message.ToJson());
                    await client.SendAsync(data, remotePoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
