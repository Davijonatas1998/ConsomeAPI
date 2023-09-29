using Xunit;

namespace ConsomeAPI.Services
{
    public static class UploadManager
    {
        [Fact]
        public static bool IsValidImage(IFormFile file)
        {
            var imagewExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return imagewExtensions.Contains(ext);
        }

        [Fact]
        public static string TokenSystem(int tamanho)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, tamanho).Select(s => s[random.Next(s.Length)]).ToArray());
            return result;
        }
    }
}
