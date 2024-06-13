using QADraft.Models;

namespace QADraft.ViewModels
{
    public class CombinedEventsViewModel
    {
        public EventsViewModel EventsViewModel { get; set; }
        public Events NewEvent { get; set; } = new Events(); // Initialize with a new event instance
    }
}
