using System.Runtime.CompilerServices;

namespace AsyncReimplementation;

public class Promise<T>
{
    private T result = default!;
    private Exception exception = null!;
    public bool IsResolved { get; private set; }
    public bool IsRejected { get; private set; }
    public bool IsCompleted => IsResolved || IsRejected;
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
        if (IsResolved || IsRejected) return;

        IsResolved = true;
        result = value;
        foreach (var callback in successCallbacks)
        {
            callback(result);
        }
        InvokeFinally();
    }

    private void Reject(Exception ex)
    {
        if (IsResolved || IsRejected) return;

        IsRejected = true;

        try
        {
            throw ex;
        }
        catch
        {
            // set stacktrace
            exception = ex;
        }

        foreach (var callback in errorCallbacks)
        {
            callback(exception);
        }
        InvokeFinally();
    }

    public Promise<T> Then(Action<T> onResolved)
    {
        if (IsResolved)
        {
            onResolved(result);
        }
        else if (!IsRejected)
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
        if (IsRejected)
        {
            onRejected(exception);
        }
        else if (!IsResolved)
        {
            errorCallbacks.Add(onRejected);
        }
        return this;
    }

    public Promise<T> Finally(Action onFinally)
    {
        if (IsResolved || IsRejected)
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

    public PromiseAwaiter<T> GetAwaiter()
    {
        return new PromiseAwaiter<T>(this);
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