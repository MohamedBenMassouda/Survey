namespace Survey.Core.Exceptions;

public class SurveyException : Exception
{
    public int StatusCode { get; set; }
    
    public SurveyException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }
    
    public SurveyException(string message, Exception innerException, int statusCode = 500) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : SurveyException
{
    public NotFoundException(string message) : base(message, 404)
    {
    }
}

public class BadRequestException : SurveyException
{
    public BadRequestException(string message) : base(message, 400)
    {
    }
}

public class UnauthorizedException : SurveyException
{
    public UnauthorizedException(string message) : base(message, 401)
    {
    }
}

public class ValidationException : SurveyException
{
    public Dictionary<string, string[]> Errors { get; set; }
    
    public ValidationException(string message, Dictionary<string, string[]> errors = null) 
        : base(message, 400)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}

