using System;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Interfaces.Repositories;

namespace Rendezvous.API.Data.Repositories;

public class PhotoRepository(DataContext context) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoByIdAsync(int id)
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotosAsync()
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .Where(p => !p.IsApproved)
            .Select(p => new PhotoForApprovalDto
            {
                Id = p.Id,
                Url = p.Url,
                Username = p.AppUser.UserName,
                IsApproved = p.IsApproved
            })
            .ToListAsync();
    }

    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}
