namespace FinanceApp.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveFile(IFormFile file)
        {
            var rootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(rootPath, "Images");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string[] allowedExtensions = [".png", ".jpg"];
            var extension = Path.GetExtension(file.FileName);

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Only {string.Join(",", allowedExtensions)}"); 
            }

            var newFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(path, newFileName);

            using var fileStream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(fileStream);
            return newFileName;
        }

        public void DeleteFile(string fileName)
        {
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", fileName);

            if (!Path.Exists(fullPath))
            {
                throw new FileNotFoundException($"File {fileName} does not exist!");
            }

            File.Delete(fullPath);
        } 
    }
}
