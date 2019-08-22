# Authorized Buyers Helpers
[![Build status](https://ci.appveyor.com/api/projects/status/w6qs46oly5ptu2a4/branch/master?svg=true)](https://ci.appveyor.com/project/inasync/authorizedbuyershelpers/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Inasync.AuthorizedBuyersHelpers.svg)](https://www.nuget.org/packages/Inasync.AuthorizedBuyersHelpers/)

***Authorized Buyers Helpers*** は Authorized Buyers RTB の為のヘルパーライブラリです。


## Target Frameworks
- .NET Standard 2.0+
- .NET Standard 1.3+
- .NET Framework 4.5+
- .NET Core 2.1+


## Usage
### Decrypt Price
```cs
var crypto = new ABCrypto(new ABCryptoKeys(
      encryptionKey: Base64Url.Decode("sIxwz7yw62yrfoLGt12lIHKuYrK_S5kLuApI2BQe7Ac=")
    , integrityKey : Base64Url.Decode("v3fsVcMBMMHYzRhi7SpM0sdqwzvAxM6KPTu9OtVod5I=")
));

var cipherPrice = "OG46wAAMCggBI0VniavN7-mNy0VTKPbB3o5CMQ==";
var success = crypto.TryDecryptPrice(cipherPrice, out var price);

Console.WriteLine(success);  // true
Console.WriteLine(price);  // 1.2
```

### Encrypt Price
```cs
var crypto = new ABCrypto(new ABCryptoKeys(
      encryptionKey: Base64Url.Decode("sIxwz7yw62yrfoLGt12lIHKuYrK_S5kLuApI2BQe7Ac=")
    , integrityKey : Base64Url.Decode("v3fsVcMBMMHYzRhi7SpM0sdqwzvAxM6KPTu9OtVod5I=")
));

var price = 1.2m;
var iv = Base16.Decode("386E3AC0000C0A080123456789ABCDEF");
var cipherPrice = crypto.EncryptPrice(price, iv);

Console.WriteLine(cipherPrice);  // OG46wAAMCggBI0VniavN7-mNy0VTKPbB3o5CMQ==
```


## Licence
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
