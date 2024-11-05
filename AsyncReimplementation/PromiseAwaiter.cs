using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncReimplementation;
public class PromiseAwaiter<T> : INotifyCompletion
{
    private readonly Promise<T> _promise;
    private T _result = default!;
    private Exception _exception = null!;

    public PromiseAwaiter(Promise<T> promise)
    {
        _promise = promise;
    }

    public bool IsCompleted => _promise.IsCompleted;

    public T GetResult()
    {
        if (_exception != null)
            throw new AggregateException(_exception);

        return _result;
    }

    public void OnCompleted(Action continuation)
    {
        _promise
            .Then(result =>
            {
                _result = result;
                continuation();
            })
            .Catch(ex => _exception = ex);
    }
}
