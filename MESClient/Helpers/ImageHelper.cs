using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MESClient.Helpers;

public static class ImageHelper
{
    /// <summary>
    /// 从Assets文件夹加载图片
    /// </summary>
    /// <param name="imageName"></param>
    /// <returns></returns>
    public static Bitmap LoadFromAssets(string imageName)
    {
        return new Bitmap(AssetLoader.Open(new Uri($"avares://{nameof(MESClient)}/Assets/Images/{imageName}.png")));
    }
    
    public static async Task<Bitmap?> LoadFromWebAsync(Uri url)
    {
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch(HttpRequestException ex)
        {
            Console.WriteLine($"Error loading image from {url}: {ex.Message}");
            return null;
        }
    }
}