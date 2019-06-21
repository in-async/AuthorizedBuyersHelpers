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
|      TryEncrypt |  Clr |     Clr | 18.097 us | 0.7548 us | 2.2257 us | 17.761 us | 0.8545 |     - |     - |    1392 B |
|      TryDecrypt |  Clr |     Clr | 17.286 us | 0.4490 us | 1.3099 us | 16.920 us | 0.8545 |     - |     - |    1392 B |
|    EncryptPrice |  Clr |     Clr | 18.893 us | 0.9964 us | 2.8588 us | 17.195 us | 0.9766 |     - |     - |    1582 B |
| TryDecryptPrice |  Clr |     Clr | 16.723 us | 0.1662 us | 0.1473 us | 16.737 us | 0.9766 |     - |     - |    1560 B |
|      TryEncrypt | Core |    Core |  4.452 us | 0.0394 us | 0.0349 us |  4.454 us | 0.5264 |     - |     - |     832 B |
|      TryDecrypt | Core |    Core |  4.521 us | 0.0895 us | 0.1660 us |  4.459 us | 0.5264 |     - |     - |     832 B |
|    EncryptPrice | Core |    Core |  5.305 us | 0.0603 us | 0.0534 us |  5.293 us | 0.6485 |     - |     - |    1021 B |
| TryDecryptPrice | Core |    Core |  5.112 us | 0.0970 us | 0.0996 us |  5.083 us | 0.6332 |     - |     - |    1000 B |
