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
|      TryEncrypt |  Clr |     Clr | 11.491 us | 0.4985 us | 1.3896 us | 11.009 us | 0.2594 |     - |     - |     416 B |
|      TryDecrypt |  Clr |     Clr | 10.904 us | 0.4776 us | 0.4467 us | 10.751 us | 0.2594 |     - |     - |     416 B |
|    EncryptPrice |  Clr |     Clr | 12.361 us | 0.4431 us | 1.2997 us | 11.946 us | 0.3815 |     - |     - |     605 B |
| TryDecryptPrice |  Clr |     Clr | 10.967 us | 0.0910 us | 0.0760 us | 10.961 us | 0.3662 |     - |     - |     584 B |
|      TryEncrypt | Core |    Core |  1.746 us | 0.0342 us | 0.0286 us |  1.736 us | 0.1202 |     - |     - |     192 B |
|      TryDecrypt | Core |    Core |  1.901 us | 0.0635 us | 0.1823 us |  1.836 us | 0.1202 |     - |     - |     192 B |
|    EncryptPrice | Core |    Core |  2.570 us | 0.0769 us | 0.2218 us |  2.462 us | 0.2403 |     - |     - |     382 B |
| TryDecryptPrice | Core |    Core |  2.523 us | 0.0905 us | 0.2611 us |  2.410 us | 0.2251 |     - |     - |     360 B |
