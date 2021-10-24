using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

namespace MyBrowser
{
    class Fetcher
    {
        public string Body;
        public System.Net.HttpStatusCode Code;

        public Uri LastUri;
        static HttpClient client = null;

        public Fetcher()
        {
            HttpClientHandler Handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Automatic,
                AutomaticDecompression = System.Net.DecompressionMethods.All,
            };
            client = new HttpClient(Handler);
            client.Timeout = TimeSpan.FromSeconds(50);
        }

        async Task fetchFile(string Path)
        {
            try
            {
                Console.WriteLine("Fetching file " + Path);
                Body = await File.ReadAllTextAsync(Path);
                Code = System.Net.HttpStatusCode.OK;
            }
            catch (FileNotFoundException)
            {
                Code = System.Net.HttpStatusCode.NotFound;
            }
            catch (DirectoryNotFoundException)
            {
                Code = System.Net.HttpStatusCode.NotFound;
            }
        }

        async Task fetchHttp(Uri Uri)
        {
            HttpResponseMessage Response = await client.GetAsync(Uri);
            Code = Response.StatusCode;
            Body = await Response.Content.ReadAsStringAsync();
        }

        public async Task fetch(string Url)
        {
            Body = "";
            LastUri = new Uri(Url);

            if (LastUri.Scheme.ToLower() == "file")
            {
                await fetchFile(Uri.UnescapeDataString(LastUri.AbsolutePath));
            }
            else
            {
                await fetchHttp(LastUri);
            }
        }
    }
}