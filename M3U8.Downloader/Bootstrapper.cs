using System.Collections.Concurrent;
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
        Func<string, Task<Dictionary<int, string>>> sourceAsync)
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

        var videoParts = await sourceAsync(inurl);

        int size = videoParts.Count / Environment.ProcessorCount +
                   videoParts.Count % Environment.ProcessorCount;

        ConcurrentDictionary<int, byte[]> videoData = new ConcurrentDictionary<int, byte[]>();
        List<Task> tasks = new List<Task>();
        var bars = new ConcurrentBag<ProgressBar>();

        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            var parts = videoParts.Skip(size * i).Take(size).ToList();
            ProgressBar pb = new ProgressBar(parts.Count);
            pb.Refresh(0, "Connecting to server...");
            bars.Add(pb);

            tasks.Add(DownloadParts(parts, videoData, pb));
        }

        Task.WaitAll(tasks.ToArray());

        Console.WriteLine("Merging video parts...");
        using var memory = new MemoryStream();
        for (int i = 0; i < videoParts.Count; i++)
        {
            var bytes = videoData[i];
            memory.Write(bytes,0,bytes.Length);
        }

        string output = $"{outpath}.ts";
        await File.WriteAllBytesAsync(output, memory.GetBuffer());
        Console.WriteLine($"File {output} save success. End.");
    }

    /// <summary>
    /// Download video parts. Using in async task
    /// </summary>
    /// <param name="parts">Video parts (number and url)</param>
    /// <param name="videoData">Output dictionary with number and data (bytes[])</param>
    /// <param name="pb">Progress bar</param>
    private async Task DownloadParts(IList<KeyValuePair<int, string>> parts, 
        ConcurrentDictionary<int, byte[]> videoData,
        ProgressBar pb)
    {
        using var client = new HttpClient();
        int current = 1;
        foreach (var part in parts)
        {
            var bytes = await client.GetByteArrayAsync(part.Value);
            var ok = videoData.TryAdd(part.Key, bytes);

            if (!ok)
            {
                Console.WriteLine("Can't add part to video data. Please, retry download video.");
                return;
            }

            pb.Refresh(current, $"Downloading data {current} from {parts.Count}");
            current++;
        }
    }
}