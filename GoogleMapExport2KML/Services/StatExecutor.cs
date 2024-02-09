using System.Diagnostics;
using GoogleMapExport2KML.Models;

namespace GoogleMapExport2KML.Services;

public class StatExecutor
{
    private readonly List<Stat> _stats = [];
    private readonly Stopwatch _stopWatch;

    public StatExecutor()
    {
        _stopWatch = new Stopwatch();
    }

    public async Task<TOut> ExecuteAsync<TOut>(string @event, Func<Task<TOut>> action)
    {
        try
        {
            _stopWatch.Start();
            return await action.Invoke();
        }
        finally
        {
            _stopWatch.Stop();
            _stats.Add(new Stat(@event, _stopWatch.ElapsedMilliseconds));
            _stopWatch.Reset();
        }
    }

    public async Task ExecuteAsync(string @event, Func<Task> action)
    {
        try
        {
            _stopWatch.Start();
            await action.Invoke();
        }
        finally
        {
            _stopWatch.Stop();
            _stats.Add(new Stat(@event, _stopWatch.ElapsedMilliseconds));
            _stopWatch.Reset();
        }
    }

    public IEnumerable<Stat> GetResults()
    {
        return _stats;
    }

    public void AddStat(Stat stat)
    {
        _stats.Add(stat);
    }

    public void Reset()
    {
        _stopWatch.Reset();
        _stats.Clear();
    }
}
