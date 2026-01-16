using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TriggerGrid.Shared.Data;
using TriggerGrid.Shared.Serializing;

namespace TriggerGrid.Shared.AF
{
    public class ErrorActionResult : IActionResult
    {
        readonly ErrorResponse? error;
        public ErrorActionResult(ErrorResponse? error)
        {
            this.error = error;
        }
        public Task ExecuteResultAsync(ActionContext context)
        {
            HttpResponse http = context.HttpContext.Response;

            http.ContentType = "application/json";
            if (error != null)
            {
                http.StatusCode = (int)error.StatusCode;
                return JsonSerializer.SerializeAsync(http.Body, error);
            }
            

            http.StatusCode = (int)HttpStatusCode.OK;
            return http.WriteAsync("{}");
        }
    }
}
