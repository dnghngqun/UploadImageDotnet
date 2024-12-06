using UploadImgurApp.Models;

namespace UploadImgurApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestSharp;
using System.IO;
using System.Threading.Tasks;

public class ImageController : Controller
{
    private readonly string _clientId;

    public ImageController(IOptions<ImgurSettings> options)
    {
        _clientId = options.Value.ClientId;
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ViewBag.Message = "Vui lòng chọn một file!";
            return View();
        }

        // Đọc file thành byte[]
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();
        var base64Image = Convert.ToBase64String(fileBytes);

        // Upload đến Imgur
        var client = new RestClient("https://api.imgur.com/3/upload");
        var request = new RestRequest("/", Method.Post);
        request.AddHeader("Authorization", $"Client-ID {_clientId}");
        request.AddParameter("image", base64Image);
        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            var imgurResponse = System.Text.Json.JsonDocument.Parse(response.Content);
            var imgUrl = imgurResponse.RootElement.GetProperty("data").GetProperty("link").GetString();
            ViewBag.ImageUrl = imgUrl;
        }
        else
        {
            ViewBag.Message = "Upload thất bại!";
        }

        return View();
    }
}