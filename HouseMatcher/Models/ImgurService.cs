using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class ImgurService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;

    public ImgurService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _accessToken = configuration["Imgur:AccessToken"]; // 讀取 Imgur Access Token
    }

    public async Task<bool> DeleteImageAsync(string imgUrl)
    {
        if (string.IsNullOrEmpty(imgUrl)) return false; // 沒圖片直接返回

        // 解析 Imgur 連結，取得 Image Hash
        Match match = Regex.Match(imgUrl, @"imgur\.com\/([a-zA-Z0-9]+)\.");
        if (!match.Success) return false;

        string imageHash = match.Groups[1].Value;
        string apiUrl = $"https://api.imgur.com/3/image/{imageHash}";

        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

        HttpResponseMessage response = await _httpClient.DeleteAsync(apiUrl);
        return response.IsSuccessStatusCode;
    }
}
