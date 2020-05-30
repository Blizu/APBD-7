using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace APBD6.Middlewares
{
    public class RequestLogger
    {
        private readonly RequestDelegate _next;

        private readonly string _logsPath = "Logs.txt";

        public RequestLogger(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            await _next(httpContext);

            using (var reader = new StreamReader(httpContext.Request.Body))
            {
                string httpMethod = httpContext.Request.Method;
                string httpUrl = httpContext.Request.Path;
                string httpBody = await reader.ReadToEndAsync();
                string httpQuery = httpContext.Request.QueryString.Value;

                string log = $"{httpMethod},{httpUrl},{httpBody},{httpQuery}{Environment.NewLine}";
                await File.AppendAllTextAsync(this._logsPath, log);
            }
        }
    }
}
