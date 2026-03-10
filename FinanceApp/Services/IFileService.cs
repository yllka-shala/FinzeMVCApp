namespace FinanceApp.Services
{
    public interface IFileService
    {
        Task<string> SaveFile(IFormFile file);
        void DeleteFile(string fileName);
    }
}
