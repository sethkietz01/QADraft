using Microsoft.AspNetCore.Mvc;

namespace QADraft.ViewComponents
{
    public class SnipeItViewComponent : ViewComponent
    {
        private readonly SnipeItApiClient _snipeItApiClient;

        public SnipeItViewComponent()
        {

            _snipeItApiClient = new SnipeItApiClient();
        }

        public async Task<IViewComponentResult> InvokeAsync(DateTime startDate, DateTime endDate, int[] data)
        {
            try
            {
                // When the invoke is called it means the date range was changed 
                ViewBag.totalCheckedOut = await _snipeItApiClient.ActivityReportBetween("checkout", startDate, endDate);

                // Pass the start and end date into the viewbag. This is not necessary for the api call to function properly but is user friendly
                ViewBag.startDate = startDate;
                ViewBag.endDate = endDate;

                // Check if the data array has length 14 (the number of fields fetched from snipe it ai)
                if (data.Length == 14)
                {
                    // All data is in the array and doesn't need to be re-calculated
                    ViewBag.currentCheckedout = data[0];
                    ViewBag.currentReChecked = data[2];
                    ViewBag.currentReminderEmail = data[2];
                    ViewBag.currentCourtesyCallCompleted = data[3];
                    ViewBag.current1stLateFee = data[4];
                    ViewBag.current2ndLateFee = data[5];
                    ViewBag.currentMaintenance = data[6];
                    ViewBag.currentCirculation = data[7];
                    ViewBag.availableWin = data[8];
                    ViewBag.availableAir = data[9];
                    ViewBag.availableMac = data[10];
                    ViewBag.availablGcal = data[11];
                    ViewBag.availablProj = data[12];
                    ViewBag.availablCam = data[13];
                }
                else
                {
                    // Otherwise, the index page is being reloaded and the data should be fetched again
                    ViewBag.currentCheckedout = await _snipeItApiClient.GetStatusCount("Checked Out");
                    ViewBag.currentReChecked = await _snipeItApiClient.GetStatusCount("Re-Checked Out");
                    ViewBag.currentReminderEmail = await _snipeItApiClient.GetStatusCount("Reminder Email");
                    ViewBag.currentCourtesyCallCompleted = await _snipeItApiClient.GetStatusCount("Courtesy Call Completed");
                    ViewBag.current1stLateFee = await _snipeItApiClient.GetStatusCount("1st Late Fee");
                    ViewBag.current2ndLateFee = await _snipeItApiClient.GetStatusCount("2nd Late Fee");
                    ViewBag.currentMaintenance = await _snipeItApiClient.GetStatusCount("Maintenance/Diagnose");
                    ViewBag.currentCirculation = await _snipeItApiClient.GetStatusCount("In Circulation");
                    ViewBag.availableWin = await _snipeItApiClient.GetCountInCirculation("WIN");
                    ViewBag.availableAir = await _snipeItApiClient.GetCountInCirculation("AIR");
                    ViewBag.availableMac = await _snipeItApiClient.GetCountInCirculation("MAC");
                    ViewBag.availablGcal = await _snipeItApiClient.GetCountInCirculation("GCAL");
                    ViewBag.availablProj = await _snipeItApiClient.GetCountInCirculation("PROJ");
                    ViewBag.availablCam = await _snipeItApiClient.GetCountInCirculation("CAM");
                }

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