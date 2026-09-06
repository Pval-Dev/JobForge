namespace JobForge.Job;

public class JobResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; }

    private JobResult()
    {
        Message = null!;
    }

    public JobResult(bool success, string message)
    {
        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("The result should have a message");

        Success = success;
        Message = message;
    }
}