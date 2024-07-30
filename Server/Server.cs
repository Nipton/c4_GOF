using Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Server
    {
        public static async Task StartServer()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Any, 0);
            Dictionary<string, IPEndPoint> users = new Dictionary<string, IPEndPoint>();
            try
            {
                using UdpClient server = new UdpClient(5000);
                Console.WriteLine("Сервер запущен.");
                new Thread(() => {
                    while (true)
                    {
                        string input = Console.ReadLine()!;
                        if (input == "list")
                        {
                            foreach (var user in users.Keys)
                            {
                                Console.WriteLine(user);
                            }
                        }
                    }
                }).Start();
                while (!token.IsCancellationRequested)
                {
                    var buffer = await server.ReceiveAsync();
                    string data = Encoding.UTF8.GetString(buffer.Buffer);
                    remotePoint = buffer.RemoteEndPoint;
                    var mes = Message.FromJson(data);
                    Console.WriteLine(mes);
                    Message answerMes = new Message() { Date = DateTime.Now, Name = "Server", Text = "Сервер принял запрос" };
                    if (mes == null)
                    {
                        continue;
                    }
                    if (mes.ToName == "server")
                    {
                        if (mes.Text == "Close")
                        {
                            Console.WriteLine("Сервер заканчивает работу.");
                            answerMes.Text = "Сервер заканчивает работу.";
                            cts.Cancel();
                        }
                        else if (mes.Text == "register")
                        {
                            bool flag = users.TryAdd(mes.Name, buffer.RemoteEndPoint);
                            if (flag == false) 
                            {
                                answerMes.Text = "Ошибка регистрации.";
                            }
                            else
                            {
                                answerMes.Text = "Вы успешно зарегистрировались.";
                            }
                        }
                        else if(mes.Text == "delete")
                        {
                            if(users.ContainsKey(mes.Name))
                            {
                                users.Remove(mes.Name);
                                answerMes.Text = "Профиль успешно удален.";
                            }
                            else
                            {
                                answerMes.Text = "Ошибка удаления";
                            }
                        }
                        answerMes.ToName = mes.Name;
                    }
                    
                    else if(users.ContainsKey(mes.ToName))
                    {
                        answerMes.Name = mes.Name;
                        answerMes.Text = mes.Text;
                        remotePoint = users[mes.ToName];
                    }
                    else if (mes.ToName == "all")
                    {
                        answerMes.Name = mes.Name;
                        answerMes.Text = mes.Text;
                        byte[] answerAll = Encoding.UTF8.GetBytes(answerMes.ToJson());
                        foreach (var user in users)
                        {
                            if (user.Key != mes.Name)
                            {
                                await server.SendAsync(answerAll, user.Value);
                            }
                        }
                        continue;
                    }
                    else
                    {
                        answerMes.Text = "Пользователь не найден.";
                        continue;
                    }
                    byte[] answer = Encoding.UTF8.GetBytes(answerMes.ToJson());
                    await server.SendAsync(answer, remotePoint);
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Превышено время ожидания. Сервер заканчивает работу.");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}