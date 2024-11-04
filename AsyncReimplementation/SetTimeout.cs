using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncReimplementation;

public static class SetTimeout
{
    public static Promise<bool> SetTimeoutPromise(int milliseconds)
    {
        return new Promise<bool>((resolve, reject) =>
        {
            var timer = new System.Timers.Timer(milliseconds);
            timer.Elapsed += (sender, args) =>
            {
                timer.Stop();
                timer.Dispose();
                resolve(true); // Resolve promise after timeout
            };
            timer.AutoReset = false; // Ensures the timer only ticks once
            timer.Start();
        });
    }
}
