using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Simulabs_Burse_Console.WorkerBee;

public class CopyingWorkerBee<T> : IWorkerBee<T>
{
    private object _lock = new object();
    private int _sleepTime;
    private bool _run = false;
    private bool _isDoingWork = false;

    public Dictionary<ICollection<T>, Action<T>> Actions { get; }

    public int SleepTime
    {
        get => _sleepTime;
        set
        {
            lock (_lock)
            {
                _sleepTime = value;
            }
        }
    }

    public CopyingWorkerBee(Dictionary<ICollection<T>, Action<T>> actions, int sleepTime = 1)
    {
        Actions = actions;
    }

    public bool StartWork()
    {
        lock (_lock)
        {
            if (_run) return false;
            _run = true;
            Thread thread = new Thread(() =>
            {
                while (_run)
                {
                    foreach (var pair in Actions)
                    {
                        DoWork(pair.Key, pair.Value);
                    }
                    Thread.Sleep(SleepTime);
                }
            });
            thread.Start();
        }

        return true;
    }

    public bool StopWork()
    {
        lock (_lock)
        {
            if (!_run) return false;
            _run = false;
        }

        return true;
    }

    public bool IsDoingWork()
    {
        return _isDoingWork || Actions.Keys.Any(collection => collection.Count > 0);
    }

    private void DoWork(ICollection<T> collection, Action<T> action)
    {
        List<T> copy;
        lock (collection)
        {
            _isDoingWork = collection.Count > 0;

            copy = collection.ToList();
            collection.Clear();
        }
        copy.ForEach(action);
        _isDoingWork = collection.Count > 0;
    }
}