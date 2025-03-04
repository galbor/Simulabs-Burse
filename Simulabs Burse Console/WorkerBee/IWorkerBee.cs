using System;
using System.Collections.Generic;

namespace Simulabs_Burse_Console.WorkerBee;

public interface IWorkerBee<T>
{
    public Dictionary<ICollection<T>, Action<T>> Actions { get; }

    /*
     * returns true if started working
     */
    public bool StartWork();
    /*
     * returns true if stopped working
     */
    public bool StopWork();

    public bool IsDoingWork();
}