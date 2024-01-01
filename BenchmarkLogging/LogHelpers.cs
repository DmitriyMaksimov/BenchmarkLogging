using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace BenchmarkLogging;

[SuppressMessage("Usage", "CA2254:Template should be a static expression")]
public static class LogHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug(this ILogger logger, [StructuredMessageTemplate] string messageTemplate)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.Log(LogLevel.Debug, messageTemplate);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug<T0>(this ILogger logger, [StructuredMessageTemplate] string messageTemplate, T0 p0)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.Log(LogLevel.Debug, messageTemplate, p0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug<T0, T1>(this ILogger logger, [StructuredMessageTemplate] string messageTemplate, T0 p0, T1 p1)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.Log(LogLevel.Debug, messageTemplate, p0, p1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug<T0, T1, T2>(this ILogger logger, [StructuredMessageTemplate] string messageTemplate, T0 p0, T1 p1, T2 p2)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.Log(LogLevel.Debug, messageTemplate, p0, p1, p2);
        }
    }
}