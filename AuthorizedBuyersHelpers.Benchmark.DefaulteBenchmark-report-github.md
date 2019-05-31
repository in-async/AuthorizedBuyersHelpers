``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.14393.2214 (1607/AnniversaryUpdate/Redstone1)
Intel Core i5-6200U CPU 2.30GHz (Skylake), 1 CPU, 4 logical and 2 physical cores
Frequency=2343749 Hz, Resolution=426.6668 ns, Timer=TSC
.NET Core SDK=2.2.101
  [Host] : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT
  Clr    : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  Core   : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT

Categories=TryDecrypt  

```
|     Method |  Job | Runtime |      Mean |     Error |    StdDev |    Median | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------- |----- |-------- |----------:|----------:|----------:|----------:|------:|-------:|------:|------:|----------:|
| TryDecrypt |  Clr |     Clr | 18.217 us | 0.6708 us | 1.9673 us | 17.821 us |  1.00 | 1.1597 |     - |     - |   1.83 KB |
|            |      |         |           |           |           |           |       |        |       |       |           |
| TryDecrypt | Core |    Core |  5.792 us | 0.2404 us | 0.6858 us |  5.576 us |  1.00 | 0.8011 |     - |     - |   1.23 KB |
