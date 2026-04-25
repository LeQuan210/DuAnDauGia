using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace WebDauGiaUI.Controllers

{
    public class PerformanceFilter : IActionFilter
    {
        private Stopwatch _stopwatch;

        // Bắt đầu bấm giờ ngay khi người dùng vừa click chuyển trang
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch = Stopwatch.StartNew();
        }

        // Dừng bấm giờ khi hệ thống đã tải xong dữ liệu
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch.Stop();

            // Nếu tác vụ trả về một giao diện (View), thì gửi thời gian ra màn hình
            if (context.Result is ViewResult && context.Controller is Controller controller)
            {
                controller.ViewBag.GlobalPageLoadTime = _stopwatch.ElapsedMilliseconds;
            }
        }
    }
}
