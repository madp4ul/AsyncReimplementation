using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncReimplementation;

public static class SetTimeout
{
    public static Promise<bool> SetTimeoutAsync(int milliseconds)
    {
        return new Promise<bool>((resolve, reject) =>
        {
            Timer? timer = null;
            timer = new Timer(_ =>
            {
                resolve(true);
                timer?.Dispose(); // Clean up the timer

            }, null, milliseconds, Timeout.Infinite);
        });
    }
}
