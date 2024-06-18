using System;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace QADraft.Utilities
{
    public class GoogleStatus
    {
        
        public static string[] Get()
        {
            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                // attempt to download JSON data as a string
                try
                {
                    string url = "https://www.google.com/appsstatus/dashboard/incidents.json";
                    json_data = w.DownloadString(url);
                }
                catch { }

                List<Rootobject>? roots = (List<Rootobject>)Newtonsoft.Json.JsonConvert.DeserializeObject<List<Rootobject>>(json_data);

                // Track printed service names
                HashSet<string> printedServiceNames = new HashSet<string>();

                // Debug-print only the first occurrence of each service name
                List<string[]> services = new List<string[]>();

                foreach (var root in roots)
                {
                    if (!printedServiceNames.Contains(root.service_name))
                    {
                        Console.WriteLine($"Service Name: {root.service_name}, Severity: {root.severity}, Status: {root.most_recent_update.status}");
                        printedServiceNames.Add(root.service_name);

                        String[] service = { root.service_name, root.most_recent_update.status };
                        services.Add(service);
                    }
                }

                // Calculate outage
                var total = 0;
                var online = 0;
                // Calculate outage percentage
                foreach (var service in services)
                {
                    total += 1;
                    if (service[1] == "AVAILABLE")
                            online += 1;
                }
                var percentOutage = (online / total) * 100 + "%";

                // Generate output message
                // Define vital services
                string[] vital = { "Google Docs", "Google Drive", "Google Calendar", "Google Sheets", "App Scripts", "Google Forms" };
                // Compare outages against vital services
                string vitalOutage = "Vital Services Down:";
                foreach (var service in services)
                {
                    if (service[1] != "AVAILABLE")
                        if (vital.Contains(service[0]))
                            vitalOutage += service[0];
                }

                // Output to console for debug
                Console.WriteLine(percentOutage);
                Console.WriteLine(vitalOutage);

                // Define output string
                string[] output = { percentOutage, vitalOutage };

                return output;
            }
        }

    }


    public class Rootobject
    {
        public string id { get; set; }
        public string number { get; set; }
        public DateTime begin { get; set; }
        public DateTime created { get; set; }
        public DateTime end { get; set; }
        public DateTime modified { get; set; }
        public string external_desc { get; set; }
        public Update[] updates { get; set; }
        public Most_Recent_Update most_recent_update { get; set; }
        public string status_impact { get; set; }
        public string severity { get; set; }
        public string service_key { get; set; }
        public string service_name { get; set; }
        public Affected_Products[] affected_products { get; set; }
        public string uri { get; set; }
    }

    public class Most_Recent_Update
    {
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
        public DateTime when { get; set; }
        public string text { get; set; }
        public string status { get; set; }
    }

    public class Update
    {
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
        public DateTime when { get; set; }
        public string text { get; set; }
        public string status { get; set; }
    }

    public class Affected_Products
    {
        public string title { get; set; }
        public string id { get; set; }
    }


}