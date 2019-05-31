using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Inasync;

namespace AuthorizedBuyersHelpers.Benchmark {

    internal class Program {

        private static void Main(string[] args) {
            var config = ManualConfig.Create(DefaultConfig.Instance)
                //.With(RPlotExporter.Default)
                .With(MarkdownExporter.GitHub)
                .With(MemoryDiagnoser.Default)
                //.With(StatisticColumn.Min)
                //.With(StatisticColumn.Max)
                //.With(RankColumn.Arabic)
                .With(Job.Core)
                .With(Job.Clr)
                //.With(Job.ShortRun)
                //.With(Job.ShortRun.With(BenchmarkDotNet.Environments.Platform.X64).WithWarmupCount(1).WithIterationCount(1))
                .WithArtifactsPath(null)
                ;

            BenchmarkRunner.Run<DefaulteBenchmark>(config);
        }
    }

    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class DefaulteBenchmark {
        private readonly ABPriceCrypto _cryptoV1 = new ABPriceCrypto(CryptoKeys.Default);
        //private readonly ABPriceCrypto _cryptoV2 = new ABPriceCrypto(CryptoKeys.Default);

        private sealed class CryptoKeys : IABCryptoKeys {
            public static CryptoKeys Default = new CryptoKeys();
            public byte[] EncryptionKey { get; } = Base64Url.Decode("sIxwz7yw62yrfoLGt12lIHKuYrK_S5kLuApI2BQe7Ac=");
            public byte[] IntegrityKey { get; } = Base64Url.Decode("v3fsVcMBMMHYzRhi7SpM0sdqwzvAxM6KPTu9OtVod5I=");
        }

        private const string _cipherPrice = "5nmwvgAM0UABI0VniavN72_sy3T6V9ohlpvOpA==";

        public DefaulteBenchmark() {
        }

        [BenchmarkCategory("TryDecrypt"), Benchmark(Baseline = true)]
        public object TryDecrypt() => _cryptoV1.TryDecrypt(_cipherPrice, out _) ? (object)null : throw new ApplicationException();

        //[BenchmarkCategory("TryDecrypt"), Benchmark]
        //public object TryDecrypt_V2() => _cryptoV2.TryDecrypt(_cipherPrice, out _) ? (object)null : throw new ApplicationException();
    }
}
