using System;
using System.IO;
using System.Text;
using System.Threading;

namespace TestApp
{
    class Program
    {
        public static object Token = new();
        public static IBusConnection BusConnection = new BusConnection();

        static void Main(string[] args)
        {
            var threads = new Thread[5];

            for (var i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    var bus = new BusMessageWriter(BusConnection);
                    for (var j = 0; j < 40; j++)
                    {
                        bus.SendMessageAsync(Encoding.ASCII.GetBytes(j + ","));
                    }
                });
            }

            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Name = $"Thread {i + 1}";
                threads[i].Start();
            }
        }
    }

    internal class BusMessageWriter
    {
        private readonly IBusConnection _connection;
        private readonly MemoryStream _buffer = new();

        public BusMessageWriter(IBusConnection connection)
        {
            _connection = connection;
        }

        public void SendMessageAsync(byte[] nextMessage)
        {
            lock (Program.Token)
            {
                _buffer.Write(nextMessage);
                if (_buffer.Length > 20)
                {
                    _connection.PublishAsync(_buffer.ToArray());
                    _buffer.SetLength(0);
                }
            }
        }
    }

    internal interface IBusConnection
    {
        public void PublishAsync(byte[] message);
    }

    internal class BusConnection : IBusConnection
    {
        public void PublishAsync(byte[] message)
        {
            Console.Write($"{Thread.CurrentThread.Name} » {Encoding.ASCII.GetString(message)}\n");
        }
    }
}