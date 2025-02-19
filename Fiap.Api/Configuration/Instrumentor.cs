﻿using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace Fiap.Api.Configuration
{
    public sealed class Instrumentor : IDisposable
    {
        public const string ServiceName = "FiapApiService";

        public ActivitySource Tracer { get; }
        public Meter Recorder { get; }
        public Counter<long> IncomingRequestCounter { get; }

        public Instrumentor()
        {
            var version = typeof(Instrumentor).Assembly.GetName().Version?.ToString();
            Tracer = new ActivitySource(ServiceName, version);
            Recorder = new Meter(ServiceName, version);
            IncomingRequestCounter = Recorder.CreateCounter<long>("app.incoming.requests",
                description: "The number of incoming requests to the backend API");
        }

        public void Dispose()
        {
            Tracer.Dispose();
            Recorder.Dispose();
        }
    }
}
