namespace Money.Api.BackgroundServices;

public class EmailSenderSettings
{
    public TimeSpan ProcessingInterval { get; init; } = TimeSpan.FromSeconds(10);
    public int MaxRetries { get; init; } = 3;
    public double RetryBaseDelaySeconds { get; init; } = 5;
    public int MaxBatchSize { get; init; } = 100;
    public int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount * 2;
}
