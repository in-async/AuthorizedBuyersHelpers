``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.14393.2214 (1607/AnniversaryUpdate/Redstone1)
Intel Core i5-6200U CPU 2.30GHz (Skylake), 1 CPU, 4 logical and 2 physical cores
Frequency=2343749 Hz, Resolution=426.6668 ns, Timer=TSC
.NET Core SDK=2.2.101
  [Host] : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT
  Clr    : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  Core   : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT


```
|          Method |  Job | Runtime |      Mean |     Error |    StdDev |    Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |----- |-------- |----------:|----------:|----------:|----------:|-------:|------:|------:|----------:|
|      TryEncrypt |  Clr |     Clr | 16.826 us | 0.4712 us | 1.3746 us | 16.243 us | 0.8545 |     - |     - |    1392 B |
|      TryDecrypt |  Clr |     Clr | 16.768 us | 0.4028 us | 1.1426 us | 16.509 us | 0.8545 |     - |     - |    1392 B |
|    EncryptPrice |  Clr |     Clr | 17.192 us | 0.3895 us | 1.1300 us | 16.700 us | 0.9766 |     - |     - |    1582 B |
| TryDecryptPrice |  Clr |     Clr | 16.988 us | 0.3409 us | 0.9504 us | 16.493 us | 0.9766 |     - |     - |    1560 B |
|      TryEncrypt | Core |    Core |  5.462 us | 0.2901 us | 0.8554 us |  5.186 us | 0.5264 |     - |     - |     832 B |
|      TryDecrypt | Core |    Core |  5.221 us | 0.2453 us | 0.7233 us |  5.059 us | 0.5264 |     - |     - |     832 B |
|    EncryptPrice | Core |    Core |  6.013 us | 0.2232 us | 0.6441 us |  5.884 us | 0.6485 |     - |     - |    1022 B |
| TryDecryptPrice | Core |    Core |  5.000 us | 0.0995 us | 0.0882 us |  4.972 us | 0.6332 |     - |     - |    1000 B |
