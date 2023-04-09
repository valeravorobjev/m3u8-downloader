using Konsole;

namespace M3U8.Downloader;

public class Bootstrapper
{
    public async Task RunAsync(string? inurl, string? outpath,
        Func<string, Task<IList<string>>> sourceAsync)
    {
        if (string.IsNullOrEmpty(inurl))
        {
            Console.WriteLine("Input url is null or empty. Exit.");
            return;
        }

        if (string.IsNullOrEmpty(outpath))
        {
            Console.WriteLine("Out path is null or empty. Exit.");
            return;
        }

        var items = await sourceAsync(inurl);
        
        var pb = new ProgressBar(items.Count);
        pb.Refresh(0, "Connecting to server...");

        using HttpClient client = new HttpClient();
        using var memory = new MemoryStream();

        int current = 1;
        foreach (string iitem in items)
        {
            byte[] bytes = await client.GetByteArrayAsync(iitem);

            foreach (var b in bytes)
            {
                memory.WriteByte(b);
            }
            
            pb.Refresh(current, $"Downloading file number {current} from {items.Count}");
            current++;
        }

        await File.WriteAllBytesAsync($"{outpath}.ts", memory.GetBuffer());
    }
}

/// <summary>
/// Class for source functions. Put here function that build video url(s).
/// </summary>
public static class Sources
{
    /// <summary>
    /// If you need download video m3u8 from master file, use this function.
    /// This function download and read master m3u8. Get one part and get it's items.
    /// </summary>
    /// <param name="url">Master m3u8 file</param>
    /// <returns></returns>
    public static async Task<IList<string>> MasterAsync(string url)
    {
        using HttpClient client = new HttpClient();
        string content = await client.GetStringAsync(url);

        var master = content.Split("\n").Last(c => c.Contains("https"));

        content = await client.GetStringAsync(master);

        var items = content.Split("\n").Where(c => c.Contains("https")).ToList();

        return items;
    }
}