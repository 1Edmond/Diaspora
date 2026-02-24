using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.Extensions;

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T value)
    {
        return Result.Success(value);
    }

    public static async Task<Result<T>> ToResultAsync<T>(this Task<T> task)
    {
        var value = await task;
        return Result.Success(value);
    }

    public static Result ToResult(this bool value, string errorMessage)
    {
        return value ? Result.Success() : Result.Failure(errorMessage);
    }
}