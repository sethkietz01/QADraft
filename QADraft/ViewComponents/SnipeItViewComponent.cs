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

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
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
