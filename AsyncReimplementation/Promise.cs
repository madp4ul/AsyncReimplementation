using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsyncReimplementation;
public class Promise<T>
{
    private T result;
    private Exception exception;
    private bool isResolved;
    private bool isRejected;
    private List<Action<T>> successCallbacks = new List<Action<T>>();
    private List<Action<Exception>> errorCallbacks = new List<Action<Exception>>();

    public Promise(Action<Action<T>, Action<Exception>> executor)
    {
        try
        {
            executor(Resolve, Reject);
        }
        catch (Exception ex)
        {
            Reject(ex);
        }
    }

    private void Resolve(T value)
    {
        if (isResolved || isRejected)
            return;

        isResolved = true;
        result = value;
        foreach (var callback in successCallbacks)
        {
            callback(result);
        }
    }

    private void Reject(Exception ex)
    {
        if (isResolved || isRejected)
            return;

        isRejected = true;
        exception = ex;
        foreach (var callback in errorCallbacks)
        {
            callback(exception);
        }
    }

    public Promise<T> Then(Action<T> onResolved)
    {
        if (isResolved)
        {
            onResolved(result);
        }
        else if (!isRejected)
        {
            successCallbacks.Add(onResolved);
        }
        return this;
    }

    public Promise<U> Then<U>(Func<T, Promise<U>> onResolved)
    {
        var nextPromise = new Promise<U>((resolve, reject) =>
        {
            Then(result =>
            {
                try
                {
                    var next = onResolved(result);
                    next.Then(resolve).Catch(reject);
                }
                catch (Exception ex)
                {
                    reject(ex);
                }
            }).Catch(reject);
        });

        return nextPromise;
    }

    public Promise<T> Catch(Action<Exception> onRejected)
    {
        if (isRejected)
        {
            onRejected(exception);
        }
        else if (!isResolved)
        {
            errorCallbacks.Add(onRejected);
        }
        return this;
    }

    public static implicit operator Task<T>(Promise<T> promise)
    {
        var tcs = new TaskCompletionSource<T>();

        // Set up the promise to complete the TaskCompletionSource when resolved or rejected
        promise.Then(result => tcs.SetResult(result))
               .Catch(ex => tcs.SetException(ex));

        return tcs.Task;
    }

    public TaskAwaiter<T> GetAwaiter()
    {
        var tcs = new TaskCompletionSource<T>();

        // Complete the TaskCompletionSource based on the promise's outcome
        Then(result => tcs.SetResult(result))
           .Catch(ex => tcs.SetException(ex));

        return tcs.Task.GetAwaiter();
    }
}