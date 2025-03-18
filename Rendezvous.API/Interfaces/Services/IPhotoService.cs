using CloudinaryDotNet.Actions;

namespace Rendezvous.API.Interfaces.Services;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);

    Task<DeletionResult> DeletePhotoAsync(string publicId);
}
