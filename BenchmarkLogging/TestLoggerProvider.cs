using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace BenchmarkLogging;

using Microsoft.Extensions.Logging;

public sealed class TestLoggerConfiguration
{
    public LogLevel MinLogLevel { get; set; }
    public bool UseFormatter { get; set; }
}

public sealed class TestLogger(string name, Func<TestLoggerConfiguration> getCurrentConfig) : ILogger
{
    public string Name { get; } = name;
    private string _formattedString = "";

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= getCurrentConfig().MinLogLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (getCurrentConfig().UseFormatter)
        {
            _formattedString = formatter(state, exception);
        }
    }
}

public sealed class TestLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private TestLoggerConfiguration _currentConfig;

    private readonly ConcurrentDictionary<string, TestLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    public TestLoggerProvider(IOptionsMonitor<TestLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new TestLogger(name, GetCurrentConfig));

    private TestLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}

public static class TestLoggerExtensions
{
    public static ILoggingBuilder AddTestLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<TestLoggerConfiguration, TestLoggerProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddTestLogger(this ILoggingBuilder builder, Action<TestLoggerConfiguration> configure)
    {
        builder.AddTestLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}
