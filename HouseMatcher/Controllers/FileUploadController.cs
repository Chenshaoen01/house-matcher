using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class FileUploadController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public FileUploadController(IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "無效的檔案" });

        try
        {
            string token = _configuration["Imgur:AccessToken"]; // 讀取 Imgur Access Token
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { error = "未設定 Imgur Access Token" });

            using var stream = file.OpenReadStream();
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "image", file.FileName);

            // 設定 Imgur API Header (使用 Bearer Token)
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PostAsync("https://api.imgur.com/3/image", content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return BadRequest(new { error = "圖片上傳失敗", message = result });

            var jsonResponse = JObject.Parse(result);
            var data = jsonResponse["data"];
            var url = data["link"].ToString();

            return Ok(new { message = "上傳成功", url });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "圖片上傳失敗", message = ex.Message });
        }
    }
}