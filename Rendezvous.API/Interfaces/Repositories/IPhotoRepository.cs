using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;

namespace Rendezvous.API.Interfaces.Repositories;

public interface IPhotoRepository
{
    Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotosAsync();

    Task<Photo?> GetPhotoByIdAsync(int id);

    void RemovePhoto(Photo photo);
}
