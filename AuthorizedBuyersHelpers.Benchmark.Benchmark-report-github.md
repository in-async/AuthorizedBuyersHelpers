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
|      TryEncrypt |  Clr |     Clr | 10.691 us | 0.2441 us | 0.7042 us | 10.401 us | 0.2594 |     - |     - |     416 B |
|      TryDecrypt |  Clr |     Clr | 11.282 us | 0.2918 us | 0.8559 us | 10.984 us | 0.2594 |     - |     - |     416 B |
|    EncryptPrice |  Clr |     Clr | 11.327 us | 0.2253 us | 0.5132 us | 11.261 us | 0.3815 |     - |     - |     605 B |
| TryDecryptPrice |  Clr |     Clr | 11.360 us | 0.2272 us | 0.6181 us | 11.232 us | 0.3662 |     - |     - |     584 B |
|      TryEncrypt | Core |    Core |  1.734 us | 0.0234 us | 0.0183 us |  1.734 us | 0.1183 |     - |     - |     192 B |
|      TryDecrypt | Core |    Core |  1.805 us | 0.0359 us | 0.0920 us |  1.781 us | 0.1202 |     - |     - |     192 B |
|    EncryptPrice | Core |    Core |  2.459 us | 0.0502 us | 0.1173 us |  2.444 us | 0.2403 |     - |     - |     382 B |
| TryDecryptPrice | Core |    Core |  2.396 us | 0.0479 us | 0.1219 us |  2.373 us | 0.2251 |     - |     - |     360 B |
