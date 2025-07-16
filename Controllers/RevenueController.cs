using Microsoft.AspNetCore.Mvc;
using WebFilmOnline.Services.Real_Time;
using WebFilmOnline.Models;

namespace WebFilmOnline.Controllers
{
    [ApiController] // Đánh dấu đây là một API Controller
    [Route("api/[controller]")] // Định tuyến cơ bản cho Controller này, ví dụ: /api/Revenue
    public class RevenueController : ControllerBase
    {
        private readonly StreamProcessor _streamProcessor;

        public RevenueController(StreamProcessor streamProcessor)
        {
            _streamProcessor = streamProcessor;
        }

        [HttpGet("summary")] // Endpoint: /api/Revenue/summary
        public IActionResult GetRevenueSummary()
        {
            var summary = _streamProcessor.GetCurrentRevenueSummary();
            return Ok(summary); // Trả về dữ liệu dưới dạng JSON
        }
    }
}
