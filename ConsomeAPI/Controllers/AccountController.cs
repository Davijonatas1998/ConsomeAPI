using ConsomeAPI.Models;
using ConsomeAPI.Models.Config_Arquivos;
using ConsomeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection.Metadata;

namespace ConsomeAPI.Controllers
{
    public class AccountController : Controller
    {
        private readonly ConfigurationImagens _myConfig;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AccountController(IOptions<ConfigurationImagens> myConfiguration, IWebHostEnvironment hostingEnvironment)
        {
            _myConfig = myConfiguration.Value;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([Bind("UserName,CPF,RG,Email,Password,CEP,Rua,Bairro,Cidade,Estado,Numero,Complemento")] RegisterAccount register, List<IFormFile> Imagem)
        {
            var tokenImage = UploadManager.TokenSystem(8);

            foreach (var formFile in Imagem)
            {
                if (!UploadManager.IsValidImage(formFile))
                {
                    ModelState.AddModelError("", "Insira uma imagem válida.");
                    return View();
                }

                var fileName = tokenImage + formFile.FileName;
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, _myConfig.NomePastaUsuario);

                using (var stream = new FileStream(Path.Combine(filePath, fileName), FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }

                register.ImagemUser = fileName;
            }

            string ApiBaseUrl = "https://localhost:7254/api/Account/Register?" + $"ImagemUser={register.ImagemUser}&UserName={register.UserName}&Email={register.Email}&CPF={register.CPF}&RG={register.RG}&Numero={register.Numero}&CEP={register.CEP}&Rua={register.Rua}&Bairro={register.Bairro}&Cidade={register.Cidade}&Estado={register.Estado}&Complemento={register.Complemento}&Password={register.Password}";

            var client = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl);
            var response = await client.SendAsync(httpRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email,Password")] LoginAccount login)
        {
            string ApiBaseUrl = "https://localhost:7254/api/Account/Login?" + $"Email={login.Email}&Password={login.Password}";

            var client = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl);
            var response = await client.SendAsync(httpRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                TokenManager.CookieManager(response, HttpContext);
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("JWTApplication");
            return RedirectToAction("Index", "Home");
        }
    }
}
