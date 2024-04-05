namespace BankingSystem.API.Utilities
{
    public interface IFileUploadService
    {
        Task<string> getfileurl(string fileName);

    }
}
