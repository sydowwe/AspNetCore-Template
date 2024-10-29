using AspNetCore_Template.helper;

namespace AspNetCore_Template.model.DTO;

public class ServiceResult
{
    public bool Succeeded { get; protected init; }
    public ServiceResultErrorType? ErrorType { get; protected init; }
    public string? ErrorMessage { get; protected init; }

    public static ServiceResult Successful()
    {
        return new ServiceResult
        {
            Succeeded = true
        };
    }

    public static ServiceResult Error(ServiceResultErrorType? errorType, string? errorMessage)
    {
        return new ServiceResult
        {
            Succeeded = false,
            ErrorType = errorType,
            ErrorMessage = errorMessage
        };
    }
}

public class ServiceResult<T> : ServiceResult where T : class
{
    public T? Data { get; private init; }

    public static ServiceResult<T> Successful(T data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data), "Data cannot be null on success.");
        return new ServiceResult<T>
        {
            Succeeded = true,
            Data = data
        };
    }

    public new static ServiceResult<T> Error(ServiceResultErrorType? errorType, string? errorMessage)
    {
        return new ServiceResult<T>
        {
            Succeeded = false,
            ErrorType = errorType,
            ErrorMessage = errorMessage
        };
    }

    public static ServiceResult<T> Error(ServiceResult serviceResult)
    {
        return new ServiceResult<T>
        {
            Succeeded = false,
            ErrorType = serviceResult.ErrorType,
            ErrorMessage = serviceResult.ErrorMessage
        };
    }
}