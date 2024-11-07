using AsyncReimplementation;

namespace Examples;
public static class Example
{
    public static void RandomPromise()
    {
        var promise = new Promise<int>((resolve, reject) =>
        {
            // Simulate an asynchronous operation
            var random = new Random();
            int result = random.Next(0, 2);

            if (result == 1)
                resolve(42);  // Success
            else
                reject(new Exception("Random failure"));  // Failure
        });

        promise
            .Then(result => Console.WriteLine("Promise resolved with: " + result))
            .Catch(error => Console.WriteLine("Promise rejected with error: " + error.Message));
    }

    public static void HttpRequestPromiseVoid()
    {


        Fetch.FetchPromise("http://www.example.com")
            .Then(response =>
            {
                Console.WriteLine("First request response:\n" + response);
                return Fetch.FetchPromise("http://www.example.org"); // Second request after the first completes
            })
            .Then(response =>
            {
                Console.WriteLine("Second request response:\n" + response);
                return Fetch.FetchPromise("http://www.example.net"); // Third request after the second completes
            })
            .Then(response =>
            {
                Console.WriteLine("Third request response:\n" + response);
            })
            .Catch(error => Console.WriteLine("An error occurred: " + error.Message));
    }

    public static Promise<string> HttpRequestPromise()
    {
        return Fetch.FetchPromise("http://www.example.com")
            .Then(response =>
            {
                Console.WriteLine("First request response:\n" + response);
                return Fetch.FetchPromise("http://www.example.org"); // Second request after the first completes
            })
            .Then(response =>
            {
                Console.WriteLine("Second request response:\n" + response);
                return Fetch.FetchPromise("http://www.example.net"); // Third request after the second completes
            })
            .Then(response =>
            {
                Console.WriteLine("Third request response:\n" + response);
            })
            .Catch(error => Console.WriteLine("An error occurred: " + error.Message));
    }

    public static async Task HttpRequestPromiseAwaiter()
    {
        try
        {
            var awaiter1 = Fetch.FetchPromise("http://www.example.com")
                .GetAwaiter();

            awaiter1
                .OnCompleted(() =>
                {
                    try
                    {
                        var response = awaiter1.GetResult();
                        Console.WriteLine("First request response:\n" + response);

                        var awaiter2 = Fetch.FetchPromise("http://www.example.org")
                            .GetAwaiter();

                        awaiter2
                           .OnCompleted(() =>
                           {
                               try
                               {
                                   var response = awaiter2.GetResult();
                                   Console.WriteLine("Second request response:\n" + response);

                                   var awaiter3 = Fetch.FetchPromise("http://www.example.net")
                                       .GetAwaiter();

                                   awaiter3
                                       .OnCompleted(() =>
                                       {
                                           try
                                           {
                                               var response = awaiter3.GetResult();
                                               Console.WriteLine("Third request response:\n" + response);
                                           }
                                           catch (Exception ex)
                                           {
                                               Console.WriteLine("An error occurred: " + ex.Message);
                                           }
                                       });
                               }
                               catch (Exception ex)
                               {
                                   Console.WriteLine("An error occurred: " + ex.Message);
                               }
                           });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                    }
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }


    public static Promise<string> HttpRequestPromiseIterativeAwaiter()
    {
        var currentAwaiter = Fetch.FetchPromise("http://www.example.com").GetAwaiter();

        Action<string> _resolve = null!;
        var resultPromise = new Promise<string>((resolve, reject) => _resolve = resolve);

        foreach (var action in GetContinuation())
        {
            currentAwaiter.OnCompleted(action);
        }

        return resultPromise;

        IEnumerable<Action> GetContinuation()
        {
            yield return () =>
            {
                try
                {
                    var response = currentAwaiter.GetResult();
                    Console.WriteLine("First request response:\n" + response);
                    currentAwaiter = Fetch.FetchPromise("http://www.example.org").GetAwaiter(); // Second request after the first completes
                }
                catch (Exception ex)
                {
                    Catch(ex);
                }

            };

            yield return () =>
            {
                try
                {
                    var response = currentAwaiter.GetResult();
                    Console.WriteLine("Second request response:\n" + response);
                    currentAwaiter = Fetch.FetchPromise("http://www.example.net").GetAwaiter(); // Third request after the second completes
                }
                catch (Exception ex)
                {
                    Catch(ex);
                }

            };

            yield return () =>
            {
                try
                {
                    var response = currentAwaiter.GetResult();
                    Console.WriteLine("Third request response:\n" + response);

                    _resolve?.Invoke(response);
                }
                catch (Exception ex)
                {
                    Catch(ex);
                }
            };
        }

        void Catch(Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    public static async Task HttpRequestPromiseAsync()
    {
        try
        {
            var response1 = await Fetch.FetchPromise("http://www.example.com");
            Console.WriteLine("First request response:\n" + response1);

            var response2 = await Fetch.FetchPromise("http://www.example.org");
            Console.WriteLine("Second request response:\n" + response2);

            var response3 = await Fetch.FetchPromise("http://www.example.net");
            Console.WriteLine("Third request response:\n" + response3);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    public static async void HttpRequestPromiseAsyncVoid()
    {
        try
        {
            var response1 = await Fetch.FetchPromise("http://www.example.com");
            Console.WriteLine("First request response:\n" + response1);

            var response2 = await Fetch.FetchPromise("http://www.example.org");
            Console.WriteLine("Second request response:\n" + response2);

            var response3 = await Fetch.FetchPromise("http://www.example.net");
            Console.WriteLine("Third request response:\n" + response3);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    public static Promise<TimeSpan> RandomTimePassed()
    {
        var random = new Random();

        var waitMs = random.Next(2000);

        return SetTimeout.SetTimeoutPromise(waitMs)
             .Then(_ => Promise.FromResult(TimeSpan.FromMilliseconds(waitMs)));
    }
}
