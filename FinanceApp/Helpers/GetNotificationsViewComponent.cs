using FinanceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Helpers
{
    [ViewComponent(Name = "GetNotifications")]
    public class GetNotificationsViewComponent : ViewComponent
    {
        private readonly INotificationService _service;

        public GetNotificationsViewComponent(INotificationService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var notifications = await _service.GetNotifications();
            return View(notifications);
        }
    }
}
