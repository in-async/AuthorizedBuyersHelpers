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

            BenchmarkRunner.Run<Benchmark>(config);
        }
    }

    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class Benchmark {
        private readonly ABCrypto _crypto = new ABCrypto(CryptoKeys.Default);

        private sealed class CryptoKeys : IABCryptoKeys {
            public static CryptoKeys Default = new CryptoKeys();
            public byte[] EncryptionKey { get; } = Base64Url.Decode("sIxwz7yw62yrfoLGt12lIHKuYrK_S5kLuApI2BQe7Ac=");
            public byte[] IntegrityKey { get; } = Base64Url.Decode("v3fsVcMBMMHYzRhi7SpM0sdqwzvAxM6KPTu9OtVod5I=");
        }

        private readonly byte[] _plainBytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        private readonly byte[] _iv = Base16.Decode("386E3AC0000C0A080123456789ABCDEF");
        private readonly byte[] _encryptResult;

        private readonly byte[] _cipherBytes = Base64Url.Decode("OG46wAAMCggBI0VniavN7-mMyUZXP79GRe8GCxemZ6YXxmvL");
        private readonly byte[] _decryptResult;

        private readonly decimal _price = 1234.5678m;
        private readonly string _cipherPrice = "OG46wAAMCggBI0VniavN7-mNy0UarLs5X0vorg==";

        public Benchmark() {
            _encryptResult = new byte[_plainBytes.Length + ABCrypto.OverheadSize];
            _decryptResult = new byte[_cipherBytes.Length - ABCrypto.OverheadSize];
        }

        [Benchmark]
        public object TryEncrypt() => _crypto.TryEncrypt(_plainBytes, _iv, _encryptResult, out _) ? (object)null : throw new ApplicationException();

        [Benchmark]
        public object TryDecrypt() => _crypto.TryDecrypt(_cipherBytes, _decryptResult, out _) ? (object)null : throw new ApplicationException();

        [Benchmark]
        public string EncryptPrice() => _crypto.EncryptPrice(_price);

        [Benchmark]
        public object TryDecryptPrice() => _crypto.TryDecryptPrice(_cipherPrice, out var price) && price == _price ? (object)null : throw new ApplicationException();
    }
}
