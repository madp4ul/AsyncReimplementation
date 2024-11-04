using System.Runtime.CompilerServices;

namespace AsyncReimplementation;

public class Promise<T>
{
    private T result = default!;
    private Exception exception = null!;
    private bool isResolved;
    private bool isRejected;
    private readonly List<Action<T>> successCallbacks = new();
    private readonly List<Action<Exception>> errorCallbacks = new();
    private readonly List<Action> finallyCallbacks = new();

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
        if (isResolved || isRejected) return;

        isResolved = true;
        result = value;
        foreach (var callback in successCallbacks)
        {
            callback(result);
        }
        InvokeFinally();
    }

    private void Reject(Exception ex)
    {
        if (isResolved || isRejected) return;

        isRejected = true;
        exception = ex;
        foreach (var callback in errorCallbacks)
        {
            callback(exception);
        }
        InvokeFinally();
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

    public Promise<T> Finally(Action onFinally)
    {
        if (isResolved || isRejected)
        {
            onFinally();
        }
        else
        {
            finallyCallbacks.Add(onFinally);
        }
        return this;
    }

    private void InvokeFinally()
    {
        foreach (var callback in finallyCallbacks)
        {
            callback();
        }
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

public class Promise
{
    public static Promise<List<T>> All<T>(IEnumerable<Promise<T>> promises)
    {
        return new Promise<List<T>>((resolve, reject) =>
        {
            var promiseList = promises.ToList();  // Convert to list to access by index
            var results = new T[promiseList.Count]; // Array to store results in order
            int completedCount = 0;

            for (int i = 0; i < promiseList.Count; i++)
            {
                int index = i;  // Capture the current index for the closure

                promiseList[index]
                    .Then(result =>
                    {
                        results[index] = result; // Store result at the correct index
                        completedCount++;

                        if (completedCount == promiseList.Count)
                        {
                            resolve(results.ToList()); // Convert array to list and resolve
                        }
                    })
                    .Catch(reject);  // Reject if any promise fails
            }
        });
    }

    public static Promise<T> Race<T>(IEnumerable<Promise<T>> promises)
    {
        return new Promise<T>((resolve, reject) =>
        {
            foreach (var promise in promises)
            {
                promise
                    .Then(resolve)
                    .Catch(reject);
            }
        });
    }
}