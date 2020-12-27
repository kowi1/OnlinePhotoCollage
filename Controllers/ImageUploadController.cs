using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;


namespace OnlinePhotoCollage.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageUploadController : ControllerBase
    {
        
        private readonly ILogger<ImageUploadController> _logger;
        
        private readonly Producer _producer;
        private readonly IMemoryCache _memoryCache;

        public ImageUploadController(ILogger<ImageUploadController> logger,Producer producer, IMemoryCache memoryCache)
        {
            _logger = logger;
            _producer = producer;
            _memoryCache = memoryCache;
        }

       
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("d");
        }
        [HttpPost("uploads")]
        public async Task<IActionResult> uploads(IFormCollection data)
        {   
            var files = Request.Form.Files; 
            List<String> images= new List<String>();       
             foreach(var file in files){
            
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"uploads", fileName);

             images.Add(fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
             _producer.PushMessageToQ(images,Int16.Parse(data["border"]),Int16.Parse(data["colorRed"]),Int16.Parse(data["colorGreen"]),Int16.Parse(data["colorBlue"]),data["orientation"]);
            return Ok(data["about"]);
        }
        [HttpGet("[action]")]
        public List<List<string>> Refresh()
        {
            Dictionary<List<string>,Tuple<int,int,int,int,string, int>> messages = null;
            _memoryCache.TryGetValue<Dictionary<List<string>,Tuple<int,int,int,int,string, int>>>("messages", out messages);
            if (messages == null) messages = new Dictionary<List<string>,Tuple<int,int,int,int,string, int>>();

            return messages.OrderBy(m => m.Value).Select(m => m.Key).ToList();
        }
    }
}
