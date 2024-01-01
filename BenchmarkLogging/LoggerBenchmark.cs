#define ENABLE_MEL
#define ENABLE_CHECK

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;

namespace BenchmarkLogging;

[MemoryDiagnoser]
[RankColumn, CategoriesColumn, ConfidenceIntervalErrorColumn]
// [OperationsPerSecond]
// [ShortRunJob]
[GcServer]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams, BenchmarkLogicalGroupRule.ByCategory)]
[Config(typeof(Config))]
public partial class LoggerBenchmark
{
    private const string Message0 = "Test message";
    private const string Message1 = "Test message: int {Int1}";
    private const string Message2 = "Test message: int {Int1} and {String1}";
    private const string Message3 = "Test message: int {Int1}, {String1} and {Time}";

    private ILogger _logger = null!;
    private readonly DateTime _startTime = DateTime.UtcNow;
    private const int IntValue = 42;
    private const string StringValue = "Hello, World!";

    [Params(false, true)] public bool UseFormatter;
    [Params(LogLevel.Debug, LogLevel.Information)] public LogLevel MinLogLevel;

    private class Config : ManualConfig
    {
        public Config()
        {
            WithOption(ConfigOptions.DontOverwriteResults, true);
            WithOption(ConfigOptions.DisableLogFile, true);
            WithUnionRule(ConfigUnionRule.Union);
            WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Percentage));
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MinLogLevel);
            builder.ClearProviders();
            builder.AddTestLogger(configuration =>
            {
                configuration.UseFormatter = UseFormatter;
                configuration.MinLogLevel = MinLogLevel;
            });
        });
        _logger = loggerFactory.CreateLogger(nameof(LoggerBenchmark));
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = Message0)]
    partial void LoggerMessage0();

    [LoggerMessage(Level = LogLevel.Debug, Message = Message1)]
    partial void LoggerMessage1(int int1);

    [LoggerMessage(Level = LogLevel.Debug, Message = Message2)]
    partial void LoggerMessage2(int int1, string string1);

    [LoggerMessage(Level = LogLevel.Debug, Message = Message3)]
    partial void LoggerMessage3(int int1, string string1, DateTime time);

#if ENABLE_MEL
    [Benchmark, BenchmarkCategory("0")]
    public void MEL0()
    {
        _logger.LogDebug(Message0);
    }

    [Benchmark, BenchmarkCategory("1")]
    public void MEL1()
    {
        _logger.LogDebug(Message1, IntValue);
    }

    [Benchmark, BenchmarkCategory("2")]
    public void MEL2()
    {
        _logger.LogDebug(Message2, IntValue, StringValue);
    }

    [Benchmark, BenchmarkCategory("3")]
    public void MEL3()
    {
        _logger.LogDebug(Message3, IntValue, StringValue, _startTime);
    }
#endif

#if ENABLE_CHECK
    [Benchmark, BenchmarkCategory("0")]
    public void IsEnabledCheck0()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(Message0);
        }
    }

    [Benchmark, BenchmarkCategory("1")]
    public void IsEnabledCheck1()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(Message1, IntValue);
        }
    }

    [Benchmark, BenchmarkCategory("2")]
    public void IsEnabledCheck2()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(Message2, IntValue, StringValue);
        }
    }

    [Benchmark, BenchmarkCategory("3")]
    public void IsEnabledCheck3()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(Message3, IntValue, StringValue, _startTime);
        }
    }
#endif
    
    [Benchmark, BenchmarkCategory("0")]
    public void Helper0()
    {
        _logger.Debug(Message0);
    }

    [Benchmark, BenchmarkCategory("1")]
    public void Helper1()
    {
        _logger.Debug(Message1, IntValue);
    }

    [Benchmark, BenchmarkCategory("2")]
    public void Helper2()
    {
        _logger.Debug(Message2, IntValue, StringValue);
    }
    
    [Benchmark, BenchmarkCategory("3")]
    public void Helper3()
    {
        _logger.Debug(Message3, IntValue, StringValue, _startTime);
    }

    [Benchmark(Baseline = true), BenchmarkCategory("0")]
    public void SourceGenerator0()
    {
        LoggerMessage0();
    }

    [Benchmark(Baseline = true), BenchmarkCategory("1")]
    public void SourceGenerator1()
    {
        LoggerMessage1(IntValue);
    }

    [Benchmark(Baseline = true), BenchmarkCategory("2")]
    public void SourceGenerator2()
    {
        LoggerMessage2(IntValue, StringValue);
    }
    
    [Benchmark(Baseline = true), BenchmarkCategory("3")]
    public void SourceGenerator3()
    {
        LoggerMessage3(IntValue, StringValue, _startTime);
    }
}