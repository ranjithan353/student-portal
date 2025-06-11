using System;
using System.Collections.Concurrent;
using System.Threading;

namespace StudentAttendanceManagementSystem1
{
    public static class MessageQueue
    {
        private static ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private static AutoResetEvent signal = new AutoResetEvent(false);

        static MessageQueue()
        {
            Thread receiverThread = new Thread(ReceiveMessages)
            {
                IsBackground = true
            };
            receiverThread.Start();
        }

        public static void Send(string message)
        {
            Console.WriteLine($"[Sender] Sending message: \"{message}\"");
            queue.Enqueue(message);
            signal.Set();
        }

        private static void ReceiveMessages()
        {
            while (true)
            {
                signal.WaitOne();
                while (queue.TryDequeue(out string msg))
                {
                    Console.WriteLine($"[Receiver] Got message: \"{msg}\"");
                }
            }
        }
    }
}