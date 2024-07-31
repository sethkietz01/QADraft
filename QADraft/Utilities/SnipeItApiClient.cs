using Azure;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using System.ComponentModel;
using System.Collections.Specialized;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class SnipeItApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string api_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiIxIiwianRpIjoiOTRiMmRkZjgxMjkzNTg4MTgzNzhmYjlhZjNjMTU0ZDQ5ZDVjNzQxZTk4MDExYjU3Nzk0OTcyNjBmNWExZjM5NWIwMTBiMGE2ZTU4ZDdiYjAiLCJpYXQiOjE3MTU2MTQ4NDguMzU3NzkxLCJuYmYiOjE3MTU2MTQ4NDguMzU3NzkyLCJleHAiOjIxODg5MTQwNDguMjc2NzA5LCJzdWIiOiIyMTQ5MiIsInNjb3BlcyI6W119.XB9P_wlc3FqQZ1zNeM7xcnZsJPnVohYn9pREzyw9opkRaI7zIilIlPDAeUIiVoChwRN83ytyqcteMeuBU22mfWQCUAB3Ww4zooiBWMlMbpDMhjuY6bH28HBKMTXcBOeacnzfzGU265OYgOKs1jAqJwR1m7SdxKYm_gGauOVgcEeFkYWnldOTl3iy4kgB3loiLZ4ly3aBKvWH2J8QapwHFCj9avfJ3yBlTzzShTWqPTCOz9V8jn5QKyq_XoY5Q1EbyqabiPYZVX-1-e6Vsm3GhPvdpQ0Y28bN9jzzvwHnz52FmGUF2gvGgvV-8mM4cYkHAoexwZjoC3IMxIO96zsHIwfQNX8pTmzdij2sUr917P0z0yMlAlWD4zWQQIdKbeeLppgiu639p4IOhxSx3HsP_RN02xNi8ww5dfYwtvpz3BPo4DnSzFmV-_LTqb4qGSR_jX4FfimV_wA3pnRSget19_3UxIWFDNmoERQgoYyzns-ubMg5V_99d8ORUkLGXyjR6YyIkqWD18gTYpzXqN1IaWMJH4LhXUwQg_YZP7bfPr-GWtAuwOBbuDZKWhn-3hSyDoK6mQ2GVcQGy_oq3Ifmmn5Ss6Drhk-Mz3_aXde9Hq25GHKeGyqfrVveCutNodiLyEqYPglypyLRJFQozJdoAhZAb4uk6btRCDAII_YZVws";
    private readonly string base_url = "https://itservice6.eku.edu/api/v1/";

    public SnipeItApiClient()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {api_token}");
    }

    public async Task UpdateXml()
    {
        // Make API calls to get updated data
        int currentCheckedout = await GetStatusCount("Checked Out", 5);
        int currentReChecked = await GetStatusCount("Re-Checked Out", 13);
        int currentReminderEmail = await GetStatusCount("Reminder Email", 2);
        int currentCourtesyCallCompleted = await GetStatusCount("Courtesy Call Completed", 12);
        int current1stLateFee = await GetStatusCount("1st Late Fee", 4);
        int current2ndLateFee = await GetStatusCount("2nd Late Fee", 9);
        int currentMaintenance = await GetStatusCount("Maintenance/Diagnose", 1);
        //int currentReplacementFee = await GetStatusCount("Replacement Fee", 14);
        int currentCirculation = await GetStatusCount("In Circulation", 7);
        int availableWin = await GetCountInCirculation("WIN");
        int availableAir = await GetCountInCirculation("AIR");
        int availableMac = await GetCountInCirculation("MAC");
        int availablGcal = await GetCountInCirculation("GCAL");
        int availablProj = await GetCountInCirculation("PROJ");
        int availablCam = await GetCountInCirculation("CAM");

        // Save that data to an XMl file
        new XDocument(
            new XElement("SnipeIt",
                new XElement("Current-Checked-Out", currentCheckedout.ToString()),
                new XElement("Current-Rechecked-Out", currentReChecked.ToString()),
                new XElement("Current-Reminder-Email", currentReminderEmail.ToString()),
                new XElement("Current-Courtesy-Call-Completed", currentCourtesyCallCompleted.ToString()),
                new XElement("Current-1st-LateFee", current1stLateFee.ToString()),
                new XElement("Current-2nd-LateFee", current2ndLateFee.ToString()),
                new XElement("Current-Maintenance", currentMaintenance.ToString()),
                //new XElement("Current-Replacement-Fee", currentReplacementFee.ToString()),
                new XElement("Current-Circulation", currentCirculation.ToString()),
                new XElement("Available-WIN", availableWin.ToString()),
                new XElement("Available-AIR", availableAir.ToString()),
                new XElement("Available-MAC", availableMac.ToString()),
                new XElement("Available-GCAL", availablGcal.ToString()),
                new XElement("Available-PROJ", availablProj.ToString()),
                new XElement("Available-CAM", availablCam.ToString()),
                new XElement("Last-Update", DateTime.Now.ToString("MM/dd/yy hh:mm tt"))
            )
        )
        .Save("wwwroot/xml/SnipeItData.xml");
    }

    public async Task<int> ActivityReportBetween(string actionType, DateTime StartDate, DateTime EndDate)
    {
        //overwrite for testing
        string itemType = "asset";

        string endpoint = base_url + $"reports/activity?limit=100000&offset=0&action_type={actionType}&order=asc&sort=created_at";

        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        Console.WriteLine("ActivityReportAPI");

        if (response.IsSuccessStatusCode)
        {
            string snipeit_data = await response.Content.ReadAsStringAsync();
            string json = @snipeit_data;

            if (json != null)
            {
                JObject jsonObject = JsonConvert.DeserializeObject<JObject>(json);
                JArray rows = jsonObject["rows"] as JArray;

                List<ActivityReport> activityReports = rows.Select(row =>
                new ActivityReport
                {
                    item = new Item
                    {
                        id = (int)row["item"]["id"],
                        name = (string)row["item"]["name"],
                        type = (string)row["item"]["type"]
                    },
                    action_date = new ActionDate
                    {
                        datetime = (string)row["updated_at"]["datetime"],
                        formatted = (string)row["updated_at"]["formatted"]
                    },
                    note = (string)row["note"]

                }).ToList();

                // Filter entries based on date range, action type, and exclude tests
                List<ActivityReport> filteredReports = activityReports
                    .Where(report =>
                            DateTime.Parse(report.action_date.datetime) >= StartDate &&
                            DateTime.Parse(report.action_date.datetime) <= EndDate &&
                            report.item.type == itemType &&
                            report.item.name != null && !report.item.name.Contains("test", StringComparison.OrdinalIgnoreCase) &&
                            report.note != null && !report.note.Contains("test", StringComparison.OrdinalIgnoreCase))
                    .ToList();


                int numCheckout = 0;
                foreach (var report in filteredReports)
                {
                    numCheckout += 1;
                }

                return numCheckout;

            }
            // Error, json was null
            return -1;
        }
        else
        {
            throw new HttpRequestException($"Error fetching report with action type '{actionType}': {response.StatusCode}");
        }
    }

    public async Task<int> GetCountInCirculation(string tag)
    {
        int status_id = 7; // 7 is the status id for In Circulation
        string endpoint = $"hardware?limit=100000&status_id={status_id}&search={tag}";

        // Send GET request
        HttpResponseMessage response = await _httpClient.GetAsync(base_url + endpoint);
        Console.WriteLine("Circulation Count API" + DateTime.Now.ToString("HH:mm:ss"));

        // Check if successful
        if (response.IsSuccessStatusCode)
        {
            // Read response content
            string responseContent = await response.Content.ReadAsStringAsync();
            string json = @responseContent;

            JObject jsonObject = JsonConvert.DeserializeObject<JObject>(json);
            JArray rows = jsonObject["rows"] as JArray;


            List<Asset> assets = rows.Select(row =>
                new Asset
                {
                    asset_tag = (string)row["asset_tag"]
                }).ToList();

            // Filter assets based on the specified asset tag. The search in the api endpoint searches the whole asset, which includes other fields.
            // Using search there will help reduce the amount of filtering that needs to be done here.
            List<Asset> filteredAssets = assets   // Check if the fetched asset tag starts with {tag} is the same length as {tag}+3. ex: WIN___
                .Where(asset => asset.asset_tag.StartsWith(tag) && asset.asset_tag.Length == tag.Length + 3)
                .ToList();

            int total = filteredAssets.Count;



            return total;
        }
        else
        {
            // Handle unsuccessful response
            throw new HttpRequestException($"Failed to search activity reports: {response.StatusCode}");
        }
    }

    public async Task<int> GetStatusCount(string status, int status_id)
    {
        Console.WriteLine(status);
        string endpoint = $"hardware?limit=100000&status_id={status_id}";

        // Send GET request
        HttpResponseMessage response = await _httpClient.GetAsync(base_url + endpoint);
        Console.WriteLine("Status Count API");

        // Check if successful
        if (response.IsSuccessStatusCode)
        {
            // Read response content
            string responseContent = await response.Content.ReadAsStringAsync();
            string json = @responseContent;

            JObject jsonObject = JsonConvert.DeserializeObject<JObject>(json);
            int total = (int)jsonObject["total"];

            return total;
        }
        else
        {
            // Handle unsuccessful response
            throw new HttpRequestException($"Failed to search activity reports: {response.StatusCode}");
        }

    }

    
}

// Class construction for parsing activity report
public class ActivityReport
{
    public int id { get; set; }
    public string icon { get; set; }
    public object file { get; set; }
    public Item item { get; set; }
    public object location { get; set; }
    public CreatedAt created_at { get; set; }
    public UpdatedAt updated_at { get; set; }
    public NextAuditDate next_audit_date { get; set; }
    public int days_to_next_audit { get; set; }
    public string action_type { get; set; }
    public Admin admin { get; set; }
    public Target target { get; set; }
    public string note { get; set; }
    public object signature_file { get; set; }
    public object log_meta { get; set; }
    public ActionDate action_date { get; set; }
}

public class ActionDate
{
    public string datetime { get; set; }
    public string formatted { get; set; }
}

public class Admin
{
    public int id { get; set; }
    public string name { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
}

public class CreatedAt
{
    public string datetime { get; set; }
    public string formatted { get; set; }
}

public class Item
{
    public int id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
}

public class NextAuditDate
{
    public string date { get; set; }
    public string formatted { get; set; }
}

public class Target
{
    public int id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
}

public class UpdatedAt
{
    public string datetime { get; set; }
    public string formatted { get; set; }
}

// Class construction for parsing the Status
public class Status
{
    public int id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public string color { get; set; }
    public bool show_in_nav { get; set; }
    public bool default_label { get; set; }
    public int assets_count { get; set; }
    public string notes { get; set; }
    public CreatedAt created_at { get; set; }
    public UpdatedAt updated_at { get; set; }
    public Available_Actions available_actions { get; set; }
}

public class Available_Actions
{
    public bool update { get; set; }
    public bool delete { get; set; }
}

// Parse construction for parsing assets
public class Asset
{
    public int id { get; set; }
    public string name { get; set; }
    public string asset_tag { get; set; }
    public string serial { get; set; }
    public Model model { get; set; }
    public object model_number { get; set; }
    public object eol { get; set; }
    public StatusLabel status_label { get; set; }
    public Category category { get; set; }
    public Manufacturer manufacturer { get; set; }
    public object supplier { get; set; }
    public string notes { get; set; }
    public object order_number { get; set; }
    public Company company { get; set; }
    public object location { get; set; }
    public RtdLocation rtd_location { get; set; }
    public string image { get; set; }
    public object qr { get; set; }
    public string alt_barcode { get; set; }
    public AssignedTo assigned_to { get; set; }
    public object warranty_months { get; set; }
    public object warranty_expires { get; set; }
    public CreatedAt created_at { get; set; }
    public UpdatedAt updated_at { get; set; }
    public object last_audit_date { get; set; }
    public object next_audit_date { get; set; }
    public object deleted_at { get; set; }
    public object purchase_date { get; set; }
    public LastCheckout last_checkout { get; set; }
    public ExpectedCheckin expected_checkin { get; set; }
    public object purchase_cost { get; set; }
    public int checkin_counter { get; set; }
    public int checkout_counter { get; set; }
    public int requests_counter { get; set; }
    public bool user_can_checkout { get; set; }
    public List<object> custom_fields { get; set; }
    public AvailableActions available_actions { get; set; }
}

public class AssignedTo
{
    public int id { get; set; }
    public string username { get; set; }
    public string name { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string employee_number { get; set; }
    public string type { get; set; }
}

public class AvailableActions
{
    public bool checkout { get; set; }
    public bool checkin { get; set; }
    public bool clone { get; set; }
    public bool restore { get; set; }
    public bool update { get; set; }
    public bool delete { get; set; }
}

public class Category
{
    public int id { get; set; }
    public string name { get; set; }
}

public class Company
{
    public int id { get; set; }
    public string name { get; set; }
}


public class ExpectedCheckin
{
    public string date { get; set; }
    public string formatted { get; set; }
}

public class LastCheckout
{
    public string datetime { get; set; }
    public string formatted { get; set; }
}

public class Manufacturer
{
    public int id { get; set; }
    public string name { get; set; }
}

public class Model
{
    public int id { get; set; }
    public string name { get; set; }
}

public class RtdLocation
{
    public int id { get; set; }
    public string name { get; set; }
}

public class StatusLabel
{
    public int id { get; set; }
    public string name { get; set; }
    public string status_type { get; set; }
    public string status_meta { get; set; }
}