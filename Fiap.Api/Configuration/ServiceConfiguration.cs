﻿using Fiap.Api.Interfaces;
using Fiap.Domain.Repositories;
using Fiap.Infra.Context;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Fiap.Api.Configuration
{
    public static class ServiceConfiguration
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddHealthChecks();
            builder.Services.AddSingleton<Instrumentor>();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<FiapDataContext>(options => options.UseNpgsql(connectionString));
            builder.Services.AddScoped<IContatoRepository, ContatoRepository>();

            Action<ResourceBuilder> appResourceBuilder =
                resource => resource
                    .AddTelemetrySdk()
                    .AddService(builder.Configuration.GetValue<string>("Otlp:ServiceName"));

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(appResourceBuilder)
                .WithTracing(tracingBuilder => tracingBuilder
                    .AddSource("FiapApiTracing")
                    .SetSampler(new AlwaysOnSampler())
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.Filter = context =>
                        {
                            Console.WriteLine("WithTracing AddAspNetCoreInstrumentation " + context.Request.Path.ToString());
                            var ignore = new[] { "/swagger" };
                            return !ignore.Any(s => context.Request.Path.ToString().Contains(s));
                        };
                    })
                    .AddHttpClientInstrumentation(opts =>
                    {
                        opts.FilterHttpRequestMessage = req =>
                        {
                            Console.WriteLine("WithTracing AddHttpClientInstrumentation " + req.RequestUri!.ToString());
                            var ignore = new[] { "/loki/api" };
                            return !ignore.Any(s => req.RequestUri!.ToString().Contains(s));
                        };
                    })
                    .AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint")))
                )
                .WithMetrics(metricsProviderBuilder =>
                    metricsProviderBuilder
                        .AddRuntimeInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint"))));
        }

    }
}
