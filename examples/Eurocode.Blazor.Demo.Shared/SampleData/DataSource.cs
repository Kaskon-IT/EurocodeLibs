using Eurocode.BetonConstructies;
using Eurocode.Grondslagen;

namespace Eurocode.Blazor.Demo.Shared.SampleData;

public class DataSource
{
    public GrondslagenContext GrondslagenContext { get; set; } = new();
    //public BelastingenContext BelastingenContext { get; set; } = new(grondslagenContext);
    public BetonContext BetonContext { get; set; } = new();



    public static async Task WaitAsync(int milliseconds, Action action)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(milliseconds));
        while (await timer.WaitForNextTickAsync())
        {
            timer.Dispose();
            action.Invoke();
        };
    }

}




