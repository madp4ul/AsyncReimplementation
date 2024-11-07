// See https://aka.ms/new-console-template for more information


using Examples;

Example.RandomPromise();


for (int i = 0; i < 10; i++)
{
    TimeSpan timePassed = await Example.RandomTimePassed()
        .Then(t => Console.WriteLine("then " + t));
    Console.WriteLine(timePassed);
}



