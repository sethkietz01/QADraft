using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace QADraft.Utilities
{
    public class ZoomStatus
    {

        public static async void Get()
        {
            string apiUrl = "https://api.zoom.us/v2/status";

            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode(); // Throw if not success

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);

                    // Process responseBody to handle outage status information

                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error accessing Zoom API: {ex.Message}");
                }
            }
        }

    }
}
