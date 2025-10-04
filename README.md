Fork of [CurlThin](https://github.com/TiberiumFusion/CurlThin)

### Changes
- Get rid of CurlThin.Native magic and include and load libcurl binaries directly depending of the platform
- curl-ca-bundle.crt is also included directly - Certificate Authority (CA) bundle. You can use it via [`CURLOPT_CAINFO`](https://curl.se/libcurl/c/CURLOPT_CAINFO.html). |
- Updated native libcurl resources (_from_ 8.7.1 ca 2024-03 _to_ 8.15.0 ca 2025-08) from here:
  https://curl.se/windows/dl-8.15.0_5/curl-8.15.0_5-win32-mingw.zip
  https://curl.se/windows/dl-8.15.0_5/curl-8.15.0_5-win64-mingw.zip
  https://curl.se/windows/dl-8.15.0_5/curl-8.15.0_5-win64a-mingw.zip
- Delegate for XferInfo added
- Append header is now ready for using from loop
- Authentication type is selectable

<br/>
Modified original readme below.
<hr/>

# CurlThin
_CurlThin_ is a NET Standard compatible binding library against [libcurl](https://curl.se/libcurl/).
It includes a modern wrapper for `curl_multi` interface which uses polling with [libuv](https://libuv.org/) library instead of using inefficient `select`.

_CurlThin_ has a very thin abstraction layer, which means that writing the code is as close as possible to writing purely in libcurl. libcurl has extensive documentation and relatively strong support of community and not having additional abstraction layer makes it easier to search solutions for your problems.

Using this library is very much like working with cURL's raw C API.

### License
Library is MIT licensed.

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
