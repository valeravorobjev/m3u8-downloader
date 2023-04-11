namespace M3U8.Downloader;

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
    public static async Task<Dictionary<int, string>> MasterAsync(string url)
    {
        using HttpClient client = new HttpClient();
        string content = await client.GetStringAsync(url);

        var master = content.Split("\n").Last(c => c.Contains("https"));

        content = await client.GetStringAsync(master);

        var items = content.Split("\n").Where(c => c.Contains("https")).ToList();

        var videoParts = new Dictionary<int, string>();

        for (int i = 0; i < items.Count; i++)
        {
            videoParts.Add(i, items[i]);
        }

        return videoParts;
    }
}