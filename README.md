Simple fork of CurlThin.

### Changes
* NuGet package ID changed (_from_ `CurlThin` _to_ `CurlThin-tfusion`)
* Reverted highly incompatible target framework (_from_ .NET Standard 2.1 _to_ .Net Standard 2.0)
* Updated native libcurl resources (_from_ 7.69.1 ca 2020-03 _to_ 8.7.1 ca 2024-03)
* Updated some minor NuGet dependencies

<br/>
Modified original readme below.
<hr/>

# CurlThin
_CurlThin_ is a NET Standard compatible binding library against [libcurl](http://curl.haxx.se/libcurl).
It includes a modern wrapper for `curl_multi` interface which uses polling with [libuv](https://libuv.org/) library instead of using inefficient `select`.

_CurlThin_ has a very thin abstraction layer, which means that writing the code is as close as possible to writing purely in libcurl. libcurl has extensive documentation and relatively strong support of community and not having additional abstraction layer makes it easier to search solutions for your problems.

Using this library is very much like working with cURL's raw C API.

### License
Library is MIT licensed.

## Installation
Release assemblies and nupkgs are available from the [Releases](https://github.com/TiberiumFusion/CurlThin/releases) page.

This is a small-fry fork, so the nupkgs are _not_ on the global nuget.org repository. Add them to your project via a [local NuGet package repository](https://stackoverflow.com/a/48549013).

| Package   | Description  |
|-----------|--------------|
| `CurlThin-tfusion` | The C# wrapper for libcurl.  |
| `CurlThin-tfusion.Native` | Contains the embedded libcurl native binaries for x86 and x64 Windows. |

`CurlThin-tfusion.Native` provides the native curl library for machines that do *not* have [libcurl](https://curl.se/windows/) in their PATH.
<br/>If libcurl is already installed and in your PATH, you can skip `CurlThin-tfusion.Native`.

After you add `CurlThin-tfusion.Native` to your project, your program *must* call the following method just once, before you can use curl:

```csharp
CurlResources.Init();
```

It will extract following files to your application output directory

| Windows x86 | Windows x64 | Description |
|-------------|-------------|-------------|
| libcurl.dll | libcurl.dll | The multiprotocol file transfer library, with all dependencies included. |
| curl-ca-bundle.crt | curl-ca-bundle.crt | Certificate Authority (CA) bundle. You can use it via [`CURLOPT_CAINFO`](https://curl.se/libcurl/c/CURLOPT_CAINFO.html). |

## Examples

### Easy interface

#### GET request
```csharp
// curl_global_init() with default flags.
var global = CurlNative.Init();

// curl_easy_init() to create easy handle.
var easy = CurlNative.Easy.Init();
try
{
    CurlNative.Easy.SetOpt(easy, CURLoption.URL, "http://httpbin.org/ip");

    var stream = new MemoryStream();
    CurlNative.Easy.SetOpt(easy, CURLoption.WRITEFUNCTION, (data, size, nmemb, user) =>
    {
        var length = (int) size * (int) nmemb;
        var buffer = new byte[length];
        Marshal.Copy(data, buffer, 0, length);
        stream.Write(buffer, 0, length);
        return (UIntPtr) length;
    });

    var result = CurlNative.Easy.Perform(easy);

    Console.WriteLine($"Result code: {result}.");
    Console.WriteLine();
    Console.WriteLine("Response body:");
    Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
}
finally
{
    easy.Dispose();

    if (global == CURLcode.OK)
    {
        CurlNative.Cleanup();
    }
}
```


#### POST request
```csharp
// curl_global_init() with default flags.
var global = CurlNative.Init();

// curl_easy_init() to create easy handle.
var easy = CurlNative.Easy.Init();
try
{
    var postData = "fieldname1=fieldvalue1&fieldname2=fieldvalue2";

    CurlNative.Easy.SetOpt(easy, CURLoption.URL, "http://httpbin.org/post");

    // This one has to be called before setting COPYPOSTFIELDS.
    CurlNative.Easy.SetOpt(easy, CURLoption.POSTFIELDSIZE, Encoding.ASCII.GetByteCount(postData));
    CurlNative.Easy.SetOpt(easy, CURLoption.COPYPOSTFIELDS, postData);
    
    var stream = new MemoryStream();
    CurlNative.Easy.SetOpt(easy, CURLoption.WRITEFUNCTION, (data, size, nmemb, user) =>
    {
        var length = (int) size * (int) nmemb;
        var buffer = new byte[length];
        Marshal.Copy(data, buffer, 0, length);
        stream.Write(buffer, 0, length);
        return (UIntPtr) length;
    });

    var result = CurlNative.Easy.Perform(easy);

    Console.WriteLine($"Result code: {result}.");
    Console.WriteLine();
    Console.WriteLine("Response body:");
    Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
}
finally
{
    easy.Dispose();

    if (global == CURLcode.OK)
    {
        CurlNative.Cleanup();
    }
}
```

### Multi interface

#### Web scrape StackOverflow questions
See [Multi/HyperSample.cs](CurlThin.Samples/Multi/HyperSample.cs).
