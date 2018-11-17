using System.Threading;
using System.Threading.Tasks;

void RunInParallel(
    IEnumerable<Action> buildActions,
    int maxDegreeOfParallelism = -1,
    CancellationToken cancellationToken = default(CancellationToken)) 
{
    var options = new ParallelOptions {
        MaxDegreeOfParallelism = maxDegreeOfParallelism,
        CancellationToken = cancellationToken
    };

    Parallel.Invoke(options, buildActions.ToArray());
}

Action GetSingleAction(params  IEnumerable<Action>[] buildActions)
{
    return () => {
        foreach(var actionGroup in buildActions)
            foreach(var action in actionGroup)
                action.Invoke();
    };
}

IEnumerable<Action> GetSingleActionAsEnumerable(params IEnumerable<Action>[] buildActions)
{
    return new [] { GetSingleAction(buildActions) };
}

IEnumerable<T> EnumerableConcat<T>(params IEnumerable<T>[] items)
{
   return items.Aggregate((aggregate,next) => aggregate.Concat(next));
}