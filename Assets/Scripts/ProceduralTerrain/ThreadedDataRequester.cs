using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour
{
    static ThreadedDataRequester instance;
    Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

    struct ThreadContext
    {
        public Func<object> generateData;
        public Action<object> callback;
    }

    void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
        ThreadPool.SetMaxThreads(16,16);
    }

    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        ThreadStart threadStart = delegate
        {
            instance.DataThread(new ThreadContext(){generateData = generateData, callback = callback});
        };

        new Thread(threadStart).Start();
    }

    public static void RequestDataThreadPool(Func<object> generateData, Action<object> callback)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(instance.DataThread), new ThreadContext(){generateData = generateData, callback = callback});
    }

    void DataThread(object threadContext)
    {
        ThreadContext context = (ThreadContext) threadContext;
        object data = context.generateData(); 
        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(context.callback, data));
        }
    }

    private void Update()
    {
        if (dataQueue.Count > 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                ThreadInfo threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    struct ThreadInfo
    {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
