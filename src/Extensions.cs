using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TriggerGrid.Shared.Collections;
using TriggerGrid.Shared.Data;
using TriggerGrid.Shared.Events;
using TriggerGrid.Shared.Interfaces;
using TriggerGrid.Shared.Serializing;

namespace TriggerGrid.Shared.AF
{
    public static class Extensions
    {
        public static IActionResult ToActionResult(this ErrorResponse? error) => new ErrorActionResult(error);
        public static async Task<IActionResult> ToActionResult(this Task<ErrorResponse?> error) => new ErrorActionResult(await error);



        public static IActionResult ToActionResult(this IResponse response, Func<AsyncBinaryWriter, Task> writer)
            => response.Error != null ? response.Error.ToActionResult() : new SuccessActionResult(writer);
        public static async Task<IActionResult> ToActionResult(this Task<IResponse> response, Func<AsyncBinaryWriter, Task> writer)
            => ToActionResult(await response, writer);



        public static IActionResult ToActionResult<TResult>(this IResponse<TResult> response)
            where TResult : IAsyncSerializable => ToActionResult(response, writer => response.Result.Write(writer));
        public static async Task<IActionResult> ToActionResult<TResult>(this Task<IResponse<TResult>> response)
            where TResult : IAsyncSerializable => (await response).ToActionResult();


        public static IActionResult ToActionResult<TResult>(this IResponse<IReadOnlyCollection<TResult>?> response)
            where TResult : IAsyncSerializable
            => ToActionResult(response, writer => writer.Write(response.Result));

        public static async Task<IActionResult> ToActionResult<TResult>(this Task<IResponse<IReadOnlyCollection<TResult>>> response)
            where TResult : IAsyncSerializable => (await response).ToActionResult();

        public static async Task<IActionResult> ToActionResult<TResult>(this Task<TResult> response)
            where TResult : IAsyncSerializable => new SuccessActionResult((await response).Write);

        public static IActionResult ToActionResult<TItem>(this IResponse<IPage<TItem>> page)
            where TItem : IAsyncSerializable {
            return page.Error != null ? new ErrorActionResult(page.Error)
                : new SuccessActionResult(writer => writer.Write(page.Result));
        }

        public static async Task<IActionResult> ToActionResult<TItem>(this IAsyncEnumerable<IResponse<IPage<TItem>>> pages)
            where TItem : IAsyncSerializable
        {
            await foreach (IResponse<IPage<TItem>> page in pages)
                return page.ToActionResult();

            return new Page<TItem>(Array.Empty<TItem>()).Success<IPage<TItem>>().ToActionResult();
        }

        public static Response<int> Query(this HttpRequest request, string param)
        {
            string? value = request.Query[param];
            if (string.IsNullOrEmpty(value)) HttpStatusCode.BadRequest.Error<int>($"{param} was not specified in query string");
            int i;
            return int.TryParse(value, out i) ? i.Success() : HttpStatusCode.BadRequest.Error<int>($"Query string paramaeter {param} does not contain valid number {value}");
        }

        public static async Task<Response<string[]>> ReadIds(this HttpRequest request)
        {
            using Stream body = request.Body;
            Response<string?[]?> response = await new AsyncBinaryReader(body, request.HttpContext.RequestAborted).ReadStrings();
            if (response.Error != null) return response;
            if (response.Result == null || response.Result.Length == 0) return HttpStatusCode.BadRequest.Error<string[]>("Body does not contains ids");
            for (int i = 0; i < response.Result.Length; i++)
                if (string.IsNullOrEmpty(response.Result[i])) return HttpStatusCode.BadRequest.Error<string[]>($"Ids iwith index {i} is empty");
            return response;
        }

        public static async Task<IActionResult> ToActionResult(this ErrorResponse?[] errors)
        {
            return new SuccessActionResult(
                writer => writer.Write(errors,
                    async error => await writer.Write(error != null) ?? (error != null ? await error.Write(writer) : null)
             ));
        }

        public static async Task<IActionResult> ToActionResult(this Task<ErrorResponse?[]> task) => await (await task).ToActionResult();
    }
}
