namespace CrossCutting
{
    public interface IRecurringJob
    {
        string IntervalCronExpression { get; }
        string Identifier { get; }
        object Run();
    }
}
