namespace Client
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            //Console.Write("Введите имя: ");
            //string name = Console.ReadLine()!;
            //Console.Write("Введите порт: ");
            //string port = Console.ReadLine()!;
            //Client client = new Client(name, port);
            _ = Task.Run(Client.Instance.ClientReceveAsync);
            await Client.Instance.ClientSendAsync();
        }
    }
}