using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.Models;

public class BaseResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static BaseResponse SuccessResponse(string? message = null)
    {
        return new BaseResponse
        {
            Success = true,
            Message = message
        };
    }

    public static BaseResponse ErrorResponse(string message, List<string>? errors = null)
    {
        return new BaseResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}