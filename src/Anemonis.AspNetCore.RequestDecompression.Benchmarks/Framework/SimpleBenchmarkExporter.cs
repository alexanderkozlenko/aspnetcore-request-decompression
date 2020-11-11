// © Alexander Kozlenko. Licensed under the MIT License.

using System;
using System.Globalization;

using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

namespace Anemonis.AspNetCore.RequestDecompression.Benchmarks.Framework
{
    internal sealed class SimpleBenchmarkExporter : ExporterBase
    {
        public override void ExportToLog(Summary summary, ILogger logger)
        {
            if (summary.Table.FullContent.Length == 0)
            {
                return;
            }

            summary.Table.PrintLine(summary.Table.FullHeader, logger, string.Empty, " |");

            var separator = new string[summary.Table.FullHeader.Length];

            for (var i = 0; i < summary.Table.FullHeader.Length; i++)
            {
                separator[i] = new('-', summary.Table.Columns[i].Width);
            }

            summary.Table.PrintLine(separator, logger, string.Empty, "-|");

            for (var i = 0; i < summary.Table.FullContent.Length; i++)
            {
                summary.Table.PrintLine(summary.Table.FullContent[i], logger, string.Empty, " |");
            }
        }

        protected override string FileCaption
        {
            get => null;
        }

        protected override string FileNameSuffix
        {
            get => DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss", CultureInfo.InvariantCulture);
        }
    }
}
