using Elfie.Serialization;
using Microsoft.AspNetCore.Mvc;
using QADraft.Models;
using System.Xml.Linq;

namespace QADraft.ViewComponents
{
    public class SnipeItViewComponent : ViewComponent
    {
        private readonly SnipeItApiClient _snipeItApiClient;

        public SnipeItViewComponent()
        {

            _snipeItApiClient = new SnipeItApiClient();
        }

        public async Task<IViewComponentResult> InvokeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {

                // When the invoke is called it means the date range was changed 
                Console.WriteLine("Before date call");
                ViewBag.totalCheckedOut = await _snipeItApiClient.ActivityReportBetween("checkout", startDate, endDate);
                Console.WriteLine("After date call");

                // Pass the start and end date into the viewbag. This is not necessary for the api call to function properly but is user friendly
                ViewBag.startDate = startDate;
                ViewBag.endDate = endDate;

                
                // Load the XML file
                XDocument xmlDocument = XDocument.Load("wwwroot/xml/SnipeItData.xml");

                //Extract data from XML and populate ViewBag
                ViewBag.currentCheckedout = xmlDocument.Descendants("Current-Checked-Out").FirstOrDefault()?.Value;
                ViewBag.currentReChecked = xmlDocument.Descendants("Current-Rechecked-Out").FirstOrDefault()?.Value;
                ViewBag.currentReminderEmail = xmlDocument.Descendants("Current-Reminder-Email").FirstOrDefault()?.Value;
                ViewBag.currentCourtesyCallCompleted = xmlDocument.Descendants("Current-Courtesy-Call-Completed").FirstOrDefault()?.Value;
                ViewBag.current1stLateFee = xmlDocument.Descendants("Current-1st-LateFee").FirstOrDefault()?.Value;
                ViewBag.current2ndLateFee = xmlDocument.Descendants("Current-2nd-LateFee").FirstOrDefault()?.Value;
                ViewBag.currentMaintenance = xmlDocument.Descendants("Current-Maintenance").FirstOrDefault()?.Value;
                //ViewBag.currentReplacementFee = xmlDocument.Descendants("Current-Replacement-Fee").FirstOrDefault()?.Value;
                ViewBag.currentCirculation = xmlDocument.Descendants("Current-Circulation").FirstOrDefault()?.Value;
                ViewBag.availableWin = xmlDocument.Descendants("Available-WIN").FirstOrDefault()?.Value;
                ViewBag.availableAir = xmlDocument.Descendants("Available-AIR").FirstOrDefault()?.Value;
                ViewBag.availableMac = xmlDocument.Descendants("Available-MAC").FirstOrDefault()?.Value;
                ViewBag.availablGcal = xmlDocument.Descendants("Available-GCAL").FirstOrDefault()?.Value;
                ViewBag.availablProj = xmlDocument.Descendants("Available-PROJ").FirstOrDefault()?.Value;
                ViewBag.availablCam = xmlDocument.Descendants("Available-CAM").FirstOrDefault()?.Value;
                ViewBag.lastUpdate = xmlDocument.Descendants("Last-Update").FirstOrDefault()?.Value;


                return View();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View();
            }

        }


    }
}