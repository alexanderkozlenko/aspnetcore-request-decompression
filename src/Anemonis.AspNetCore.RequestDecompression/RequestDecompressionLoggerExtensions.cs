﻿// © Alexander Kozlenko. Licensed under the MIT License.

using System;

using Anemonis.AspNetCore.RequestDecompression.Resources;

using Microsoft.Extensions.Logging;

namespace Anemonis.AspNetCore.RequestDecompression
{
    internal static class RequestDecompressionLoggerExtensions
    {
        private static readonly Action<ILogger, Type, Exception> s_logRequestDecodingApplied =
            LoggerMessage.Define<Type>(
                LogLevel.Debug,
                new(1100, "REQDEC_DECODING_APPLIED"),
                Strings.GetString("logging.decoding_applied"));
        private static readonly Action<ILogger, Exception> s_logRequestDecodingSkipped =
            LoggerMessage.Define(
                LogLevel.Debug,
                new(1101, "REQDEC_DECODING_SKIPPED"),
                Strings.GetString("logging.decoding_skipped"));
        private static readonly Action<ILogger, Exception> s_logRequestDecodingDisabled =
            LoggerMessage.Define(
                LogLevel.Warning,
                new(1300, "REQDEC_DECODING_DISABLED"),
                Strings.GetString("logging.decoding_disabled"));

        public static void LogRequestDecodingApplied(this ILogger logger, IDecompressionProvider provider)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                s_logRequestDecodingApplied.Invoke(logger, provider.GetType(), null);
            }
        }

        public static void LogRequestDecodingSkipped(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                s_logRequestDecodingSkipped.Invoke(logger, null);
            }
        }

        public static void LogRequestDecodingDisabled(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                s_logRequestDecodingDisabled.Invoke(logger, null);
            }
        }
    }
}
