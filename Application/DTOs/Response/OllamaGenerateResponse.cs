using Application.DTOs.Request;

namespace Application.DTOs.Response;

public sealed class OllamaGenerateResponse
{
    public string Response { get; set; } = string.Empty;

    public bool Done { get; set; }

    public string DoneReason { get; set; } = string.Empty;

    public long TotalDuration { get; set; }

    public long LoadDuration { get; set; }

    public int PromptEvalCount { get; set; }

    public int EvalCount { get; set; }

    public long EvalDuration { get; set; }
}