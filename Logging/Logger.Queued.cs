using System;
using System.Collections.Generic;
using System.Threading;

namespace Agile.Diagnostics.Logging
{
    public static partial class Logger
    {
        private static ManualResetEvent hasNewItems;
        private static ManualResetEvent terminate;
        private static ManualResetEvent waiting;
        private static Queue<Action> queue;
        private static readonly object initialLock = new object();

        static partial void WriteQueued(string message, LogLevel level, LogCategory category, Type errorType, params object[] args)
        {
            lock (queue)
            {
                var threadId = GetCurrentManagedThreadId();
                queue.Enqueue(() =>
                {
                    WriteLog(message, level, category, errorType, threadId, args);
                });
            }
            hasNewItems.Set();
        }

        static partial void StartBackgroundLogging()
        {
            if(IsBackgroundProcessingStarted)
                return;
            IsBackgroundProcessingStarted = true;

            // init here so is null if not using the queue
            lock (initialLock)
            {
                if(queue != null)
                    return;
                queue = new Queue<Action>();
            }
            hasNewItems = new ManualResetEvent(false);
            terminate = new ManualResetEvent(false);
            waiting = new ManualResetEvent(false);

            try
            {
                var loggingThread = new Thread(ProcessQueue);
                loggingThread.IsBackground = true;
                loggingThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                IsBackgroundProcessingStarted = false;
            }
        }        

        private static void ProcessQueue()
        {
            while (true)
            {
                try
                {
                    waiting.Set();
                    int i = ManualResetEvent.WaitAny(new WaitHandle[] { hasNewItems, terminate });
                    // terminate was signaled 
                    if (i == 1) return;
                    hasNewItems.Reset();
                    waiting.Reset();

                    Queue<Action> queueCopy;
                    lock (queue)
                    {
                        queueCopy = new Queue<Action>(queue);
                        queue.Clear();
                    }
                    Console.WriteLine("[{0}] {1} ****", GetCurrentManagedThreadId(), queueCopy.Count);
                    foreach (var log in queueCopy)
                    {
                        log();
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
        }


    }
}