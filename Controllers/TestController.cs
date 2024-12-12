using ASP_Chat.Service;
using Microsoft.AspNetCore.Mvc;

namespace ASP_Chat.Controllers
{
    [Route("api/v1/[controller]")]
    public class TestController : Controller
    {
        private readonly IKafkaService _kafkaProducerService;

        public TestController(IKafkaService kafkaProducerService)
        {
            _kafkaProducerService = kafkaProducerService;
        }

        [HttpGet("save")]
        public async Task<IActionResult> Test()
        {
            await _kafkaProducerService.SendMessageAsync(new Service.Requests.FileRequest() 
            { 
                Operation = "Save",
                FileName = "test/test",
                FileData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Hello, world!"))
            });
            return Ok();
        }

        [HttpGet("get")]
        public async Task<IActionResult> Test2()
        {
            await _kafkaProducerService.SendMessageAsync(new Service.Requests.FileRequest()
            {
                Operation = "Get",
                FileName = "test/test"
            });

            string result = await _kafkaProducerService.WaitForResponseAsync("test/test", "media-responses");

            return Ok(result);
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Test3()
        {
            await _kafkaProducerService.SendMessageAsync(new Service.Requests.FileRequest()
            {
                Operation = "Delete",
                FileName = "test/test"
            });
            return Ok();
        }
    }
}
