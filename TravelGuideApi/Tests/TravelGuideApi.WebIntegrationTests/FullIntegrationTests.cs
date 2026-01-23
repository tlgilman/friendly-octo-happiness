using JA.Logging.Sinks.Serilog.InMemory;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Serilog.Extensions;
using Web.IntegrationTests.Engine;
using Web.IntegrationTests.Engine.Models;
using LogEvent = Web.IntegrationTests.Engine.Models.LogEvent;

namespace TravelGuideApi.WebIntegrationTests;

[TestFixture]
public class FullIntegrationTests
{
    private static readonly TestEngine<Startup> _testEngine;

    private static readonly InMemoryLogEventSink _logEventSink;

    static FullIntegrationTests()
    {
        _logEventSink = new();

        _testEngine = new TestEngine<Startup>(
            new WebApplicationFactoryClientOptions
            {
                HandleCookies = false,
                AllowAutoRedirect = false
            },
            webHostBuilder => webHostBuilder
                .ConfigureTestServices(services =>
                {
                })
                .UseSerilog((_, conf) =>
                {
                    conf.WriteTo.Sink(_logEventSink);
                }));
    }

    [TestCaseSource(nameof(AllTestCases))]
    public async Task Tests(string testCaseContent, TestEngine<Startup> testEngine)
    {
        await testEngine.RunTestCase(
            testCaseContent,
            _logEventSink.LogEvents.Clear,
            _logEventSink.LogEvents.Select(logEvent => new LogEvent
            {
                Message = logEvent.RenderMessage(),
                Severity = logEvent.Level switch
                {
                    LogEventLevel.Information => LogSeverity.Information,
                    LogEventLevel.Warning => LogSeverity.Warning,
                    LogEventLevel.Error => LogSeverity.Error,
                    _ => LogSeverity.Warning
                },
            }).ToArray);
    }

    private static IEnumerable<TestCaseData> AllTestCases
        => TestCases.All.Select(UpdateTestCaseWithTestEngine);

    private static TestCaseData UpdateTestCaseWithTestEngine(TestCaseData data)
    {
        TestCaseData result = new(data.Arguments.First(), _testEngine);
        result.SetName(data.TestName);
        return result;
    }
}
