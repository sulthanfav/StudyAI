using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace StudyAI.Api.IntegrationTests.Support;

internal sealed class CapturingLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentQueue<string> _entries = new();
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    public IReadOnlyCollection<string> Entries => _entries.ToArray();

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(this);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

    public void Dispose()
    {
    }
    private void Capture<TState>(TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var parts = new List<string> { formatter(state, exception) };
        if (exception != null)
        {
            parts.Add($"exception:{exception}");
        }
        AppendProperties("state", state, parts);
        _scopeProvider.ForEachScope(
            (scope, target) => AppendProperties("scope", scope, target),
            parts);
        _entries.Enqueue(string.Join(" | ", parts));
    }

    private static void AppendProperties(string source, object? state, List<string> parts)
    {
        if (state is IEnumerable<KeyValuePair<string, object?>> properties)
        {
            foreach (var property in properties)
            {
                parts.Add($"{source}:{property.Key}={property.Value}");
            }
        }
    }

    private sealed class CapturingLogger(CapturingLoggerProvider provider) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
            provider._scopeProvider.Push(state);

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            provider.Capture(state, exception, formatter);
    }
}
