using BuildingBlocks.Exceptions;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.ProblemDetails
{
    public static class Extension
    {
        public static WebApplication UseCustomProblemDetails(this WebApplication app)
        {
            app.UseStatusCodePages(statusCodeApp =>
                {
                    statusCodeApp.Run(async ctx =>
                    {
                        ctx.Response.ContentType = "application/problem+json";
                        var problemDetailService = ctx.RequestServices.GetService<IProblemDetailsService>();

                        await problemDetailService.WriteAsync(new ProblemDetailsContext()
                        {
                            HttpContext = ctx,
                            ProblemDetails =
                            {
                                Status = ctx.Response.StatusCode,
                                Detail = $"[{ReasonPhrases.GetReasonPhrase(ctx.Response.StatusCode)}] An error occurred while processing your request.",
                                Title = ReasonPhrases.GetReasonPhrase(ctx.Response.StatusCode),
                                Extensions =
                                {
                                    ["TraceId"] = ctx.TraceIdentifier,
                                    ["CorrelationId"] = ctx.GetCorrelationId().ToString()
                                },
                                Instance = ctx.Request.Path,
                                Type = $"https://httpstatuses.com/{ctx.Response.StatusCode}"
                            }
                        });
                    });
                }
            );

            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async ctx =>
                {
                    ctx.Response.ContentType = "application/problem+json";
                    var problemDetailService = ctx.RequestServices.GetService<IProblemDetailsService>();

                    var exceptionHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();
                    var exceptionType = exceptionHandlerFeature.Error;
                    (int StatusCode, string title) = exceptionType switch
                    {
                        ValidationException appException => (
                            StatusCodes.Status400BadRequest,
                            ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest)
                        ),
                        AppException appException => (
                            StatusCodes.Status500InternalServerError,
                            ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError)
                        ),
                        _ => (
                            StatusCodes.Status500InternalServerError,
                            ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError)
                        )
                    };

                    var problemDetail = new ProblemDetailsContext()
                    {
                        HttpContext = ctx,
                        ProblemDetails =
                        {
                            Status = StatusCode,
                            Detail = exceptionHandlerFeature.Error.Message,
                            Title = $"[{title}] An error occurred while processing your request.",
                            Extensions =
                            {
                                ["TraceId"] = ctx.TraceIdentifier,
                                ["CorrelationId"] = ctx.GetCorrelationId().ToString()
                            },
                            Instance = ctx.Request.Path,
                            Type = $"https://httpstatuses.com/{StatusCode}",
                        }
                    };

                    if (app.Environment.IsDevelopment())
                    {
                        problemDetail.ProblemDetails.Extensions.Add("Exception", exceptionHandlerFeature.Error.ToString());
                    }

                    await problemDetailService.WriteAsync(problemDetail);
                });
            });

            return app;
        }
    }
}
