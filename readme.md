# Logging benchmarks
Measuring different methods of logging to `ILogger`.

## Methodology
Created custom logger following [Microsoft recommendations](https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider).
The logger:
- does not store logging messages
- can call `formatter` - configurable by `UseFormatter` configuration property

Tested logging without, with one, two and three parameters.

## Tested logging methods

### Microsoft.Extensions.Logging
Probably the most commonly used method (`MEL`):
```csharp
    _logger.LogDebug(Message3, IntValue, StringValue, _startTime);
```

## Check if logging enabled
Using `MEL`, but first check if the logging is enabled
```csharp
    if (_logger.IsEnabled(LogLevel.Debug))
    {
        _logger.LogDebug(Message3, IntValue, StringValue, _startTime);
    }
```

## Using helper methods
Writing `if (_logger.IsEnabled(LogLevel.Debug))` is annoying, so I created helper methods.  
Using generics to avoid creation of `params object[]` if logging is disabled:
```csharp
    public static void Debug<T0, T1, T2>(this ILogger logger, [StructuredMessageTemplate] string messageTemplate, T0 p0, T1 p1, T2 p2)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.Log(LogLevel.Debug, messageTemplate, p0, p1, p2);
        }
    }
```

## Using [LoggerMessage]
Microsoft recommend using `[LoggerMessage]` for [High-performance logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging).
```csharp
    [LoggerMessage(Level = LogLevel.Debug, Message = Message3)]
    partial void LoggerMessage3(int int1, string string1, DateTime time);
```

## Conclusions
  - `MEL` is the slowest method. It is slow (up to 17 times!) even when the logging is disabled
  - Surprisingly `SourceGenerator` is slower than `IsEnabledCheck` when logging is disabled. The difference is negligible (~1 ns), but still quite surprising
  - `SourceGenerator` is the fastest logging method in most cases when logging is enabled (except two cases)
    - `SourceGenerator` is about the same speed as any other method when message template has no arguments   
    - `SourceGenerator` is 79–100% faster when message template has one argument
    - `SourceGenerator` is 49–68% faster when message template has two arguments
    - `SourceGenerator` is 57–81% faster when message template has three arguments
  - When using formatter (a bit more realistic scenario because formatter is used to produce a logging message)
    - `SourceGenerator` is ~3% **slower** comparing to `MEL` when message template has no arguments
    - `SourceGenerator` is 85–105% faster when message template has one argument
    - `SourceGenerator` is 36–43% faster when message template has two arguments
    - `SourceGenerator` is 42–51% faster when message template has three arguments

Please notice that despite a good relative performance, absolute values are not that impressive.  
We are talking about 5–60 ns (1 Nanosecond = 0.000000001 sec) difference.  
Put it in another way — to save 1 second CPU time, you need to log ~16,500,000–200,000,000 (16–200 million) messages. 

## Benchmark results

### Windows

```
BenchmarkDotNet v0.13.11, Windows 11 (10.0.22621.2861/22H2/2022Update/SunValley2)
Intel Core i7-10850H CPU 2.70GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.100
  [Host]     : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  Job-SOEYUO : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Server=False  
```
| Method           | Categories | UseFormatter | MinLogLevel | Mean       | Error     | StdDev     | Median     | CI99.9% Margin | Ratio    | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|----------------- |----------- |------------- |------------ |-----------:|----------:|-----------:|-----------:|---------------:|---------:|--------:|-----:|-------:|----------:|------------:|
| **MEL0**             | **0**          | **False**        | **Debug**       |  **26.247 ns** | **0.1792 ns** |  **0.1496 ns** |  **26.199 ns** |      **0.1792 ns** |      **-8%** |    **1.7%** |    **1** |      **-** |         **-** |          **NA** |
| IsEnabledCheck0  | 0          | False        | Debug       |  31.558 ns | 0.6285 ns |  0.5248 ns |  31.510 ns |      0.6285 ns |     +11% |    2.3% |    4 |      - |         - |          NA |
| Helper0          | 0          | False        | Debug       |  30.769 ns | 0.3050 ns |  0.2704 ns |  30.803 ns |      0.3050 ns |      +8% |    1.4% |    3 |      - |         - |          NA |
| SourceGenerator0 | 0          | False        | Debug       |  28.489 ns | 0.3244 ns |  0.3735 ns |  28.463 ns |      0.3244 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL1             | 1          | False        | Debug       |  50.376 ns | 0.6529 ns |  0.6107 ns |  50.322 ns |      0.6529 ns |     +79% |    1.4% |    2 | 0.0089 |      56 B |          NA |
| IsEnabledCheck1  | 1          | False        | Debug       |  54.455 ns | 0.5836 ns |  0.5459 ns |  54.322 ns |      0.5836 ns |     +94% |    1.5% |    3 | 0.0089 |      56 B |          NA |
| Helper1          | 1          | False        | Debug       |  56.246 ns | 1.1126 ns |  1.3245 ns |  55.896 ns |      1.1126 ns |    +100% |    2.6% |    4 | 0.0089 |      56 B |          NA |
| SourceGenerator1 | 1          | False        | Debug       |  28.120 ns | 0.3507 ns |  0.3109 ns |  28.095 ns |      0.3507 ns | baseline |         |    1 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL2             | 2          | False        | Debug       |  52.257 ns | 0.5613 ns |  0.4976 ns |  52.163 ns |      0.5613 ns |     +49% |    1.2% |    2 | 0.0102 |      64 B |          NA |
| IsEnabledCheck2  | 2          | False        | Debug       |  58.434 ns | 1.1684 ns |  1.4349 ns |  58.116 ns |      1.1684 ns |     +66% |    3.2% |    3 | 0.0101 |      64 B |          NA |
| Helper2          | 2          | False        | Debug       |  59.150 ns | 0.9211 ns |  0.8616 ns |  59.476 ns |      0.9211 ns |     +68% |    1.9% |    3 | 0.0101 |      64 B |          NA |
| SourceGenerator2 | 2          | False        | Debug       |  35.125 ns | 0.2977 ns |  0.2785 ns |  35.115 ns |      0.2977 ns | baseline |         |    1 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL3             | 3          | False        | Debug       |  61.124 ns | 1.1778 ns |  2.7061 ns |  60.431 ns |      1.1778 ns |     +57% |    6.9% |    2 | 0.0153 |      96 B |          NA |
| IsEnabledCheck3  | 3          | False        | Debug       |  66.448 ns | 0.8390 ns |  0.6551 ns |  66.706 ns |      0.8390 ns |     +81% |    2.7% |    4 | 0.0153 |      96 B |          NA |
| Helper3          | 3          | False        | Debug       |  64.994 ns | 1.3020 ns |  2.5700 ns |  63.901 ns |      1.3020 ns |     +67% |    8.1% |    3 | 0.0153 |      96 B |          NA |
| SourceGenerator3 | 3          | False        | Debug       |  38.637 ns | 0.7529 ns |  2.0611 ns |  38.112 ns |      0.7529 ns | baseline |         |    1 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| **MEL0**             | **0**          | **False**        | **Information** |  **13.580 ns** | **0.1738 ns** |  **0.1541 ns** |  **13.513 ns** |      **0.1738 ns** |    **+518%** |    **5.9%** |    **4** |      **-** |         **-** |          **NA** |
| IsEnabledCheck0  | 0          | False        | Information |   1.800 ns | 0.0162 ns |  0.0144 ns |   1.799 ns |      0.0162 ns |     -18% |    5.6% |    1 |      - |         - |          NA |
| Helper0          | 0          | False        | Information |   2.238 ns | 0.0230 ns |  0.0215 ns |   2.228 ns |      0.0230 ns |      +2% |    5.3% |    3 |      - |         - |          NA |
| SourceGenerator0 | 0          | False        | Information |   2.116 ns | 0.0705 ns |  0.1178 ns |   2.113 ns |      0.0705 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL1             | 1          | False        | Information |  35.570 ns | 0.6401 ns |  0.5674 ns |  35.458 ns |      0.6401 ns |  +1,171% |    2.0% |    4 | 0.0089 |      56 B |          NA |
| IsEnabledCheck1  | 1          | False        | Information |   1.817 ns | 0.0243 ns |  0.0215 ns |   1.810 ns |      0.0243 ns |     -35% |    1.9% |    1 |      - |         - |          NA |
| Helper1          | 1          | False        | Information |   2.417 ns | 0.0486 ns |  0.0431 ns |   2.416 ns |      0.0486 ns |     -14% |    2.1% |    2 |      - |         - |          NA |
| SourceGenerator1 | 1          | False        | Information |   2.799 ns | 0.0446 ns |  0.0417 ns |   2.788 ns |      0.0446 ns | baseline |         |    3 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL2             | 2          | False        | Information |  37.595 ns | 0.7139 ns |  0.6329 ns |  37.295 ns |      0.7139 ns |  +1,574% |    2.1% |    4 | 0.0102 |      64 B |          NA |
| IsEnabledCheck2  | 2          | False        | Information |   1.777 ns | 0.0238 ns |  0.0198 ns |   1.775 ns |      0.0238 ns |     -21% |    2.1% |    1 |      - |         - |          NA |
| Helper2          | 2          | False        | Information |   2.379 ns | 0.0195 ns |  0.0173 ns |   2.380 ns |      0.0195 ns |      +6% |    1.9% |    3 |      - |         - |          NA |
| SourceGenerator2 | 2          | False        | Information |   2.246 ns | 0.0384 ns |  0.0341 ns |   2.243 ns |      0.0384 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL3             | 3          | False        | Information |  60.211 ns | 1.2351 ns |  3.4223 ns |  60.686 ns |      1.2351 ns |  +2,213% |    5.1% |    4 | 0.0153 |      96 B |          NA |
| IsEnabledCheck3  | 3          | False        | Information |   1.838 ns | 0.0573 ns |  0.0536 ns |   1.841 ns |      0.0573 ns |     -29% |    3.5% |    1 |      - |         - |          NA |
| Helper3          | 3          | False        | Information |   2.728 ns | 0.0777 ns |  0.1187 ns |   2.695 ns |      0.0777 ns |      +8% |    6.2% |    3 |      - |         - |          NA |
| SourceGenerator3 | 3          | False        | Information |   2.576 ns | 0.0595 ns |  0.0557 ns |   2.557 ns |      0.0595 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| **MEL0**             | **0**          | **True**         | **Debug**       |  **27.368 ns** | **0.2365 ns** |  **0.2096 ns** |  **27.309 ns** |      **0.2365 ns** |      **-3%** |    **1.6%** |    **1** |      **-** |         **-** |          **NA** |
| IsEnabledCheck0  | 0          | True         | Debug       |  33.601 ns | 0.3107 ns |  0.2594 ns |  33.594 ns |      0.3107 ns |     +19% |    1.9% |    3 |      - |         - |          NA |
| Helper0          | 0          | True         | Debug       |  33.653 ns | 0.4608 ns |  0.4085 ns |  33.533 ns |      0.4608 ns |     +19% |    1.6% |    3 |      - |         - |          NA |
| SourceGenerator0 | 0          | True         | Debug       |  28.249 ns | 0.5040 ns |  0.4209 ns |  28.128 ns |      0.5040 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL1             | 1          | True         | Debug       | 117.008 ns | 1.4670 ns |  1.3005 ns | 116.408 ns |      1.4670 ns |     +85% |    1.2% |    2 | 0.0191 |     120 B |        +88% |
| IsEnabledCheck1  | 1          | True         | Debug       | 125.759 ns | 1.5587 ns |  1.3818 ns | 126.006 ns |      1.5587 ns |     +99% |    1.8% |    3 | 0.0191 |     120 B |        +88% |
| Helper1          | 1          | True         | Debug       | 129.149 ns | 2.4180 ns |  3.5443 ns | 127.814 ns |      2.4180 ns |    +105% |    3.7% |    4 | 0.0191 |     120 B |        +88% |
| SourceGenerator1 | 1          | True         | Debug       |  63.226 ns | 1.0794 ns |  0.9014 ns |  62.890 ns |      1.0794 ns | baseline |         |    1 | 0.0101 |      64 B |             |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL2             | 2          | True         | Debug       | 148.522 ns | 2.9496 ns |  4.6784 ns | 146.684 ns |      2.9496 ns |     +36% |    4.1% |    2 | 0.0267 |     168 B |        +62% |
| IsEnabledCheck2  | 2          | True         | Debug       | 156.628 ns | 0.9991 ns |  0.8343 ns | 156.483 ns |      0.9991 ns |     +43% |    2.3% |    3 | 0.0267 |     168 B |        +62% |
| Helper2          | 2          | True         | Debug       | 155.036 ns | 1.7029 ns |  1.5929 ns | 155.233 ns |      1.7029 ns |     +42% |    2.7% |    3 | 0.0267 |     168 B |        +62% |
| SourceGenerator2 | 2          | True         | Debug       | 110.046 ns | 1.8817 ns |  2.6987 ns | 109.397 ns |      1.8817 ns | baseline |         |    1 | 0.0166 |     104 B |             |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL3             | 3          | True         | Debug       | 238.535 ns | 4.6468 ns | 13.2575 ns | 234.519 ns |      4.6468 ns |     +51% |    7.1% |    3 | 0.0381 |     240 B |        +67% |
| IsEnabledCheck3  | 3          | True         | Debug       | 236.118 ns | 4.4925 ns |  9.7663 ns | 234.960 ns |      4.4925 ns |     +51% |    6.9% |    3 | 0.0381 |     240 B |        +67% |
| Helper3          | 3          | True         | Debug       | 225.931 ns | 2.7115 ns |  2.5364 ns | 225.355 ns |      2.7115 ns |     +42% |    2.6% |    2 | 0.0381 |     240 B |        +67% |
| SourceGenerator3 | 3          | True         | Debug       | 158.202 ns | 3.1967 ns |  3.6814 ns | 157.789 ns |      3.1967 ns | baseline |         |    1 | 0.0229 |     144 B |             |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| **MEL0**             | **0**          | **True**         | **Information** |  **13.548 ns** | **0.0935 ns** |  **0.0780 ns** |  **13.526 ns** |      **0.0935 ns** |    **+559%** |    **4.1%** |    **4** |      **-** |         **-** |          **NA** |
| IsEnabledCheck0  | 0          | True         | Information |   1.925 ns | 0.0372 ns |  0.0330 ns |   1.920 ns |      0.0372 ns |      -6% |    4.7% |    1 |      - |         - |          NA |
| Helper0          | 0          | True         | Information |   2.307 ns | 0.0434 ns |  0.0406 ns |   2.300 ns |      0.0434 ns |     +12% |    4.5% |    3 |      - |         - |          NA |
| SourceGenerator0 | 0          | True         | Information |   2.083 ns | 0.0704 ns |  0.0864 ns |   2.075 ns |      0.0704 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL1             | 1          | True         | Information |  37.269 ns | 0.7529 ns |  0.9790 ns |  37.277 ns |      0.7529 ns |  +1,434% |    5.5% |    4 | 0.0089 |      56 B |          NA |
| IsEnabledCheck1  | 1          | True         | Information |   1.785 ns | 0.0187 ns |  0.0156 ns |   1.787 ns |      0.0187 ns |     -26% |    4.1% |    1 |      - |         - |          NA |
| Helper1          | 1          | True         | Information |   2.489 ns | 0.0780 ns |  0.0867 ns |   2.464 ns |      0.0780 ns |      +2% |    6.1% |    3 |      - |         - |          NA |
| SourceGenerator1 | 1          | True         | Information |   2.414 ns | 0.0764 ns |  0.1120 ns |   2.385 ns |      0.0764 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL2             | 2          | True         | Information |  38.066 ns | 0.6263 ns |  0.5552 ns |  37.889 ns |      0.6263 ns |  +1,594% |    1.8% |    4 | 0.0102 |      64 B |          NA |
| IsEnabledCheck2  | 2          | True         | Information |   1.838 ns | 0.0645 ns |  0.0768 ns |   1.819 ns |      0.0645 ns |     -18% |    4.6% |    1 |      - |         - |          NA |
| Helper2          | 2          | True         | Information |   2.453 ns | 0.0778 ns |  0.0865 ns |   2.424 ns |      0.0778 ns |      +9% |    4.1% |    3 |      - |         - |          NA |
| SourceGenerator2 | 2          | True         | Information |   2.245 ns | 0.0318 ns |  0.0297 ns |   2.237 ns |      0.0318 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |            |            |                |          |         |      |        |           |             |
| MEL3             | 3          | True         | Information |  47.772 ns | 0.9906 ns |  2.6612 ns |  47.140 ns |      0.9906 ns |  +1,748% |    8.0% |    3 | 0.0153 |      96 B |          NA |
| IsEnabledCheck3  | 3          | True         | Information |   1.793 ns | 0.0411 ns |  0.0321 ns |   1.801 ns |      0.0411 ns |     -31% |    5.0% |    1 |      - |         - |          NA |
| Helper3          | 3          | True         | Information |   2.679 ns | 0.0559 ns |  0.0496 ns |   2.667 ns |      0.0559 ns |      +2% |    4.6% |    2 |      - |         - |          NA |
| SourceGenerator3 | 3          | True         | Information |   2.633 ns | 0.0829 ns |  0.1216 ns |   2.593 ns |      0.0829 ns | baseline |         |    2 |      - |         - |          NA |

### Mac

```
BenchmarkDotNet v0.13.11, macOS Ventura 13.6.2 (22G320) [Darwin 22.6.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]     : .NET 8.0.0 (8.0.23.53103), Arm64 RyuJIT AdvSIMD
  Job-RMOVVA : .NET 8.0.0 (8.0.23.53103), Arm64 RyuJIT AdvSIMD

Server=False  
```

| Method           | Categories | UseFormatter | MinLogLevel | Mean       | Error     | StdDev    | CI99.9% Margin | Ratio    | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|----------------- |----------- |------------- |------------ |-----------:|----------:|----------:|---------------:|---------:|--------:|-----:|-------:|----------:|------------:|
| **MEL0**             | **0**          | **False**        | **Debug**       |  **13.705 ns** | **0.0966 ns** | **0.0904 ns** |      **0.0966 ns** |      **-5%** |    **0.8%** |    **1** |      **-** |         **-** |          **NA** |
| IsEnabledCheck0  | 0          | False        | Debug       |  16.986 ns | 0.0688 ns | 0.0610 ns |      0.0688 ns |     +18% |    0.5% |    3 |      - |         - |          NA |
| Helper0          | 0          | False        | Debug       |  17.688 ns | 0.1487 ns | 0.1391 ns |      0.1487 ns |     +23% |    0.9% |    4 |      - |         - |          NA |
| SourceGenerator0 | 0          | False        | Debug       |  14.423 ns | 0.0344 ns | 0.0322 ns |      0.0344 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL1             | 1          | False        | Debug       |  29.598 ns | 0.1435 ns | 0.1343 ns |      0.1435 ns |    +106% |    0.5% |    2 | 0.0067 |      56 B |          NA |
| IsEnabledCheck1  | 1          | False        | Debug       |  32.565 ns | 0.2079 ns | 0.1944 ns |      0.2079 ns |    +127% |    0.7% |    3 | 0.0067 |      56 B |          NA |
| Helper1          | 1          | False        | Debug       |  33.188 ns | 0.0750 ns | 0.0702 ns |      0.0750 ns |    +131% |    0.3% |    4 | 0.0067 |      56 B |          NA |
| SourceGenerator1 | 1          | False        | Debug       |  14.364 ns | 0.0359 ns | 0.0318 ns |      0.0359 ns | baseline |         |    1 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL2             | 2          | False        | Debug       |  31.860 ns | 0.2169 ns | 0.2029 ns |      0.2169 ns |     +68% |    0.9% |    2 | 0.0076 |      64 B |          NA |
| IsEnabledCheck2  | 2          | False        | Debug       |  35.062 ns | 0.4148 ns | 0.3880 ns |      0.4148 ns |     +85% |    1.4% |    3 | 0.0076 |      64 B |          NA |
| Helper2          | 2          | False        | Debug       |  35.678 ns | 0.4047 ns | 0.3786 ns |      0.4047 ns |     +89% |    1.0% |    4 | 0.0076 |      64 B |          NA |
| SourceGenerator2 | 2          | False        | Debug       |  18.914 ns | 0.0907 ns | 0.0849 ns |      0.0907 ns | baseline |         |    1 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL3             | 3          | False        | Debug       |  35.653 ns | 0.1405 ns | 0.1097 ns |      0.1405 ns |     +83% |    0.5% |    2 | 0.0114 |      96 B |          NA |
| IsEnabledCheck3  | 3          | False        | Debug       |  38.679 ns | 0.3110 ns | 0.2909 ns |      0.3110 ns |     +99% |    0.9% |    3 | 0.0114 |      96 B |          NA |
| Helper3          | 3          | False        | Debug       |  39.317 ns | 0.6664 ns | 0.5908 ns |      0.6664 ns |    +102% |    1.3% |    3 | 0.0114 |      96 B |          NA |
| SourceGenerator3 | 3          | False        | Debug       |  19.457 ns | 0.0668 ns | 0.0592 ns |      0.0668 ns | baseline |         |    1 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| **MEL0**             | **0**          | **False**        | **Information** |   **7.677 ns** | **0.0355 ns** | **0.0315 ns** |      **0.0355 ns** |    **+361%** |    **0.4%** |    **4** |      **-** |         **-** |          **NA** |
| IsEnabledCheck0  | 0          | False        | Information |   1.287 ns | 0.0103 ns | 0.0097 ns |      0.0103 ns |     -23% |    0.8% |    1 |      - |         - |          NA |
| Helper0          | 0          | False        | Information |   1.713 ns | 0.0254 ns | 0.0237 ns |      0.0254 ns |      +2% |    1.3% |    3 |      - |         - |          NA |
| SourceGenerator0 | 0          | False        | Information |   1.666 ns | 0.0063 ns | 0.0052 ns |      0.0063 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL1             | 1          | False        | Information |  23.724 ns | 0.0426 ns | 0.0377 ns |      0.0426 ns |  +1,305% |    0.4% |    4 | 0.0067 |      56 B |          NA |
| IsEnabledCheck1  | 1          | False        | Information |   1.257 ns | 0.0035 ns | 0.0029 ns |      0.0035 ns |     -26% |    0.4% |    1 |      - |         - |          NA |
| Helper1          | 1          | False        | Information |   1.737 ns | 0.0067 ns | 0.0056 ns |      0.0067 ns |      +3% |    0.5% |    3 |      - |         - |          NA |
| SourceGenerator1 | 1          | False        | Information |   1.689 ns | 0.0077 ns | 0.0068 ns |      0.0077 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL2             | 2          | False        | Information |  25.960 ns | 0.3832 ns | 0.3585 ns |      0.3832 ns |  +1,434% |    1.8% |    4 | 0.0076 |      64 B |          NA |
| IsEnabledCheck2  | 2          | False        | Information |   1.248 ns | 0.0085 ns | 0.0079 ns |      0.0085 ns |     -26% |    0.9% |    1 |      - |         - |          NA |
| Helper2          | 2          | False        | Information |   1.737 ns | 0.0131 ns | 0.0116 ns |      0.0131 ns |      +3% |    1.0% |    3 |      - |         - |          NA |
| SourceGenerator2 | 2          | False        | Information |   1.692 ns | 0.0117 ns | 0.0110 ns |      0.0117 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL3             | 3          | False        | Information |  30.255 ns | 0.1360 ns | 0.1272 ns |      0.1360 ns |  +1,583% |    0.6% |    4 | 0.0114 |      96 B |          NA |
| IsEnabledCheck3  | 3          | False        | Information |   1.258 ns | 0.0067 ns | 0.0063 ns |      0.0067 ns |     -30% |    0.7% |    1 |      - |         - |          NA |
| Helper3          | 3          | False        | Information |   1.860 ns | 0.0062 ns | 0.0058 ns |      0.0062 ns |      +3% |    0.5% |    3 |      - |         - |          NA |
| SourceGenerator3 | 3          | False        | Information |   1.797 ns | 0.0102 ns | 0.0095 ns |      0.0102 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| **MEL0**             | **0**          | **True**         | **Debug**       |  **15.574 ns** | **0.1559 ns** | **0.1458 ns** |      **0.1559 ns** |      **+4%** |    **1.0%** |    **2** |      **-** |         **-** |          **NA** |
| IsEnabledCheck0  | 0          | True         | Debug       |  18.916 ns | 0.2103 ns | 0.1967 ns |      0.2103 ns |     +27% |    1.2% |    3 |      - |         - |          NA |
| Helper0          | 0          | True         | Debug       |  19.322 ns | 0.3260 ns | 0.5264 ns |      0.3260 ns |     +32% |    3.6% |    3 |      - |         - |          NA |
| SourceGenerator0 | 0          | True         | Debug       |  14.935 ns | 0.0887 ns | 0.0830 ns |      0.0887 ns | baseline |         |    1 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL1             | 1          | True         | Debug       |  67.618 ns | 0.1650 ns | 0.1543 ns |      0.1650 ns |     +88% |    0.3% |    2 | 0.0143 |     120 B |        +88% |
| IsEnabledCheck1  | 1          | True         | Debug       |  70.679 ns | 0.2181 ns | 0.2040 ns |      0.2181 ns |     +97% |    0.4% |    3 | 0.0143 |     120 B |        +88% |
| Helper1          | 1          | True         | Debug       |  71.507 ns | 0.1264 ns | 0.1182 ns |      0.1264 ns |     +99% |    0.2% |    4 | 0.0143 |     120 B |        +88% |
| SourceGenerator1 | 1          | True         | Debug       |  35.873 ns | 0.0792 ns | 0.0741 ns |      0.0792 ns | baseline |         |    1 | 0.0076 |      64 B |             |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL2             | 2          | True         | Debug       |  88.634 ns | 0.2621 ns | 0.2451 ns |      0.2621 ns |     +46% |    0.4% |    2 | 0.0200 |     168 B |        +62% |
| IsEnabledCheck2  | 2          | True         | Debug       |  92.174 ns | 0.2272 ns | 0.2126 ns |      0.2272 ns |     +51% |    0.4% |    3 | 0.0200 |     168 B |        +62% |
| Helper2          | 2          | True         | Debug       |  92.329 ns | 0.1008 ns | 0.0841 ns |      0.1008 ns |     +52% |    0.3% |    3 | 0.0200 |     168 B |        +62% |
| SourceGenerator2 | 2          | True         | Debug       |  60.862 ns | 0.1707 ns | 0.1597 ns |      0.1707 ns | baseline |         |    1 | 0.0124 |     104 B |             |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL3             | 3          | True         | Debug       | 130.137 ns | 0.3154 ns | 0.2796 ns |      0.3154 ns |     +59% |    0.7% |    2 | 0.0286 |     240 B |        +67% |
| IsEnabledCheck3  | 3          | True         | Debug       | 133.231 ns | 0.5572 ns | 0.4653 ns |      0.5572 ns |     +62% |    0.7% |    3 | 0.0286 |     240 B |        +67% |
| Helper3          | 3          | True         | Debug       | 134.002 ns | 0.6431 ns | 0.6015 ns |      0.6431 ns |     +63% |    0.9% |    3 | 0.0286 |     240 B |        +67% |
| SourceGenerator3 | 3          | True         | Debug       |  82.016 ns | 0.4883 ns | 0.4568 ns |      0.4883 ns | baseline |         |    1 | 0.0172 |     144 B |             |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| **MEL0**             | **0**          | **True**         | **Information** |   **7.674 ns** | **0.0273 ns** | **0.0228 ns** |      **0.0273 ns** |    **+403%** |    **1.4%** |    **4** |      **-** |         **-** |          **NA** |
| IsEnabledCheck0  | 0          | True         | Information |   1.269 ns | 0.0107 ns | 0.0100 ns |      0.0107 ns |     -17% |    1.3% |    1 |      - |         - |          NA |
| Helper0          | 0          | True         | Information |   1.728 ns | 0.0216 ns | 0.0191 ns |      0.0216 ns |     +13% |    1.4% |    3 |      - |         - |          NA |
| SourceGenerator0 | 0          | True         | Information |   1.526 ns | 0.0199 ns | 0.0186 ns |      0.0199 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL1             | 1          | True         | Information |  23.579 ns | 0.0306 ns | 0.0287 ns |      0.0306 ns |  +1,283% |    0.7% |    4 | 0.0067 |      56 B |          NA |
| IsEnabledCheck1  | 1          | True         | Information |   1.256 ns | 0.0042 ns | 0.0040 ns |      0.0042 ns |     -26% |    0.8% |    1 |      - |         - |          NA |
| Helper1          | 1          | True         | Information |   1.758 ns | 0.0096 ns | 0.0090 ns |      0.0096 ns |      +3% |    0.7% |    3 |      - |         - |          NA |
| SourceGenerator1 | 1          | True         | Information |   1.705 ns | 0.0124 ns | 0.0116 ns |      0.0124 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL2             | 2          | True         | Information |  25.587 ns | 0.0632 ns | 0.0591 ns |      0.0632 ns |  +1,412% |    0.8% |    4 | 0.0076 |      64 B |          NA |
| IsEnabledCheck2  | 2          | True         | Information |   1.257 ns | 0.0037 ns | 0.0031 ns |      0.0037 ns |     -26% |    0.7% |    1 |      - |         - |          NA |
| Helper2          | 2          | True         | Information |   1.765 ns | 0.0144 ns | 0.0120 ns |      0.0144 ns |      +4% |    0.9% |    3 |      - |         - |          NA |
| SourceGenerator2 | 2          | True         | Information |   1.693 ns | 0.0128 ns | 0.0120 ns |      0.0128 ns | baseline |         |    2 |      - |         - |          NA |
|                  |            |              |             |            |           |           |                |          |         |      |        |           |             |
| MEL3             | 3          | True         | Information |  29.930 ns | 0.1366 ns | 0.1211 ns |      0.1366 ns |  +1,577% |    0.5% |    4 | 0.0114 |      96 B |          NA |
| IsEnabledCheck3  | 3          | True         | Information |   1.266 ns | 0.0060 ns | 0.0056 ns |      0.0060 ns |     -29% |    0.6% |    1 |      - |         - |          NA |
| Helper3          | 3          | True         | Information |   1.871 ns | 0.0069 ns | 0.0061 ns |      0.0069 ns |      +5% |    0.5% |    3 |      - |         - |          NA |
| SourceGenerator3 | 3          | True         | Information |   1.785 ns | 0.0060 ns | 0.0050 ns |      0.0060 ns | baseline |         |    2 |      - |         - |          NA |
