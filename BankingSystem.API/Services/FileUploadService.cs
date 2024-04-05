using BankingSystem.API.Controllers;
using BankingSystem.API.Services.IServices;
using BankingSystem.API.Utilities;

namespace BankingSystem.API.Services
{
    public class FileUploadService:IFileUploadService
    {
      
                 private readonly FileStorageHelper _imageUploadService;
        private readonly IConfiguration _configuration;


        /// <summary>
        /// Initializes a new instance of the <see cref="KycDocumentController"/> class.
        /// </summary>
        /// <param name="kycService">The service that handles KYC Documents.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="kycService"/> is null.</exception>
        public FileUploadService(IKycService kycService, IConfiguration configuration)
        {

            _configuration = configuration;
            _imageUploadService = new FileStorageHelper(configuration);
        }

        public async Task<string> getfileurl(string fileName)
        {
            return await _imageUploadService.GeneratePresignedUrlAsync(fileName);
        }
    }
}
