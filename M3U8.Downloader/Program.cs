using M3U8.Downloader;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder();
builder.AddCommandLine(args);

var config = builder.Build();

string? url = config["url"];
string? outpath = config["out"];

Bootstrapper bootstrapper = new Bootstrapper();
await bootstrapper.RunAsync(url, outpath, Sources.MasterAsync);