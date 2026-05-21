using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Do_an_co_so.Controllers
{
    public class LanguageController : Controller
    {
        [HttpGet]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            // 1. Lưu lựa chọn ngôn ngữ của người dùng vào Cookie (Hạn sử dụng 1 năm)
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            // 2. Tải lại chính cái trang mà người dùng vừa đứng bấm nút
            return LocalRedirect(returnUrl ?? "/");
        }
    }
}