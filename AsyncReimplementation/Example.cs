using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsyncReimplementation;
public class Example
{
    public void RandomPromise()
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
        Fetch.FetchAsync("http://www.example.com")
            .Then(response =>
            {
                Console.WriteLine("First request response:\n" + response);
                return Fetch.FetchAsync("http://www.example.org"); // Second request after the first completes
            })
            .Then(response =>
            {
                Console.WriteLine("Second request response:\n" + response);
                return Fetch.FetchAsync("http://www.example.net"); // Third request after the second completes
            })
            .Then(response =>
            {
                Console.WriteLine("Third request response:\n" + response);
            })
            .Catch(error => Console.WriteLine("An error occurred: " + error.Message));
    }

    public static Promise<string> HttpRequestPromise()
    {
        return Fetch.FetchAsync("http://www.example.com")
            .Then(response =>
            {
                Console.WriteLine("First request response:\n" + response);
                return Fetch.FetchAsync("http://www.example.org"); // Second request after the first completes
            })
            .Then(response =>
            {
                Console.WriteLine("Second request response:\n" + response);
                return Fetch.FetchAsync("http://www.example.net"); // Third request after the second completes
            })
            .Then(response =>
            {
                Console.WriteLine("Third request response:\n" + response);
            })
            .Catch(error => Console.WriteLine("An error occurred: " + error.Message));
    }

    public static async void HttpRequestPromiseAsyncVoid()
    {
        try
        {
            var response1 = await Fetch.FetchAsync("http://www.example.com");
            Console.WriteLine("First request response:\n" + response1);

            var response2 = await Fetch.FetchAsync("http://www.example.org");
            Console.WriteLine("Second request response:\n" + response2);

            var response3 = await Fetch.FetchAsync("http://www.example.net");
            Console.WriteLine("Third request response:\n" + response3);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    public static async Task HttpRequestPromiseAsync()
    {
        try
        {
            var response1 = await Fetch.FetchAsync("http://www.example.com");
            Console.WriteLine("First request response:\n" + response1);

            var response2 = await Fetch.FetchAsync("http://www.example.org");
            Console.WriteLine("Second request response:\n" + response2);

            var response3 = await Fetch.FetchAsync("http://www.example.net");
            Console.WriteLine("Third request response:\n" + response3);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}
