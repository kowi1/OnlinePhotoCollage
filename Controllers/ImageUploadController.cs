using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Threading;
using System.Threading.Tasks;


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
        public async Task<string> uploads(IFormCollection formdata)
        {   
            var files = Request.Form.Files; 
            List<String> imageNameList= new List<String>();       
            foreach(var file in files)
            {
            
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"uploads", fileName);

            imageNameList.Add(fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
             {
                await file.CopyToAsync(stream);
             }
            }
             var unique_id =_producer.PushMessageToQ(imageNameList,Int16.Parse(formdata["border"]),Int16.Parse(formdata["colorRed"]),Int16.Parse(formdata["colorGreen"]),Int16.Parse(formdata["colorBlue"]),formdata["orientation"]);
            return JsonConvert.SerializeObject(unique_id);
        }
        [HttpGet("[action]")]
        public List<List<string>> Refresh()
        {
            Dictionary<List<string>,Tuple<int,int,int,int,string, int>> messages = null;
            _memoryCache.TryGetValue<Dictionary<List<string>,Tuple<int,int,int,int,string, int>>>("messages", out messages);
            if (messages == null) messages = new Dictionary<List<string>,Tuple<int,int,int,int,string, int>>();

            return messages.OrderBy(m => m.Value).Select(m => m.Key).ToList();
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> img(string unique_id)
        {
            Console.WriteLine(unique_id);
            Dictionary<string,string> outputImage = null;
            _memoryCache.TryGetValue<Dictionary<string,string>>("outputImage", out outputImage);
            if (outputImage == null) outputImage = new Dictionary<string,string>();
            var path=outputImage[unique_id];
              var image = System.IO.File.OpenRead(path);
            return File(image, "image/png");
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> DownloadImage(string unique_id)
        {     
            Dictionary<string,string> outputImage = null;
            _memoryCache.TryGetValue<Dictionary<string,string>>("outputImage", out outputImage);
            if (outputImage == null) outputImage = new Dictionary<string,string>();
            var path=outputImage[unique_id];
            // var path = Path.GetFullPath("./wwwroot/images/school-assets/" + filename);
             MemoryStream memory = new MemoryStream();
             using (FileStream stream = new FileStream(path, FileMode.Open))
             {
              await stream.CopyToAsync(memory);
             }
             memory.Position = 0;
             return File(memory, "image/png", Path.GetFileName(path));
        }
    }
}
