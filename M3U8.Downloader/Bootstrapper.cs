using Konsole;

namespace M3U8.Downloader;

/// <summary>
/// Incapsulate all program logic: get video parts, download video segments, merge and save
/// to output file
/// </summary>
public class Bootstrapper
{
    /// <summary>
    /// Run download video
    /// </summary>
    /// <param name="inurl">In url with video data</param>
    /// <param name="outpath">Output file (path with name)</param>
    /// <param name="sourceAsync">Function for loading source.</param>
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