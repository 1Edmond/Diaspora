using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.Models;

public class DataResponse<T> : BaseResponse
{
    public T? Data { get; set; }

    public static DataResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new DataResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public new static DataResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new DataResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}