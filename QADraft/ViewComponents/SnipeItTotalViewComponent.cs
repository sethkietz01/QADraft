using Microsoft.AspNetCore.Mvc;

namespace QADraft.ViewComponents
{
    public class SnipeItTotalViewComponent : ViewComponent
    {

        private readonly SnipeItApiClient _snipeItApiClient;

        public SnipeItTotalViewComponent()
        {

            _snipeItApiClient = new SnipeItApiClient();
        }

        public async Task<IViewComponentResult> InvokeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                ViewBag.totalCheckout = await _snipeItApiClient.ActivityReportBetween("checkout", startDate, endDate);
                
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
