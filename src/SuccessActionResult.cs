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
using TriggerGrid.Shared.Interfaces;
using TriggerGrid.Shared.Serializing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TriggerGrid.Shared.AF
{
    public class SuccessActionResult : IActionResult
    {
        Func<AsyncBinaryWriter, Task> writer;
        public SuccessActionResult(Func<AsyncBinaryWriter, Task> writer)
        {
            this.writer = writer;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            HttpResponse http = context.HttpContext.Response;
            http.StatusCode = (int)HttpStatusCode.OK;
            http.ContentType = "application/octet-stream";
            return this.writer(new AsyncBinaryWriter(http.Body, context.HttpContext.RequestAborted));            
        }
    }
}
