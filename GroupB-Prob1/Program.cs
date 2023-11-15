using System;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

class Program
{
    public const string MethodAdd = "add";
    public const string MethodMode = "mode";
    public const string MethodMean = "mean";
    public const string MethodRange = "range";
    public const string MethodStdDev = "stddev";



    static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHost(
                webHost => webHost.UseKestrel(
                        kestrelOptions =>
                        {
                            kestrelOptions.ListenAnyIP(8080);
                            kestrelOptions.AllowSynchronousIO = true;
                        })
                    .Configure(app => app.Run(
                            async context =>
                            {
                                if (context.Request.Method != "POST")
                                {
                                    context.Response.StatusCode = 405;
                                    await context.Response.WriteAsync("Only POST are accepted");
                                }
                                else
                                {
                                    var body = context.Request.Body;
                                    if (body != null)
                                    {
                                        var ops = JsonSerializer.Deserialize<Req>(body);

                                        var res = new Res();

                                        if (ops.Method.ToLower() == MethodAdd)
                                        {
                                            res.Result = ops.Input.Sum();
                                        }
                                        else if (ops.Method.ToLower() == MethodMean)
                                        {
                                            res.Result = (int)ops.Input.Average();
                                        }
                                        else if (ops.Method.ToLower() == MethodMode)
                                        {
                                            res.Result = ops.Input.GroupBy(x => x )
                                                         .OrderByDescending(g => g.Count())
                                                         .First()
                                                         .Key;
                                        }

                                        await context.Response.WriteAsJsonAsync(res);
                                    }
                                }
                            }
                        )
                    )
                );
}

public class Req
{
    [JsonPropertyName("input")]
    public List<int> Input { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; }
}

public class Res
{
    [JsonPropertyName("result")]
    public int Result { get; set; }
}


