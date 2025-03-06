using AutoMapper;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Extensions;

namespace Rendezvous.API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(member => member.Age,
                opt => opt.MapFrom(user => user.DateOfBirth.CalculateAge()))
            .ForMember(member => member.PhotoUrl,
                opt => opt.MapFrom(user => user.Photos.FirstOrDefault(photo => photo.IsMain)!.Url));
        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
        CreateMap<string, DateOnly>().ConvertUsing(source => DateOnly.Parse(source));
        CreateMap<Message, MessageDto>()
            .ForMember(dto => dto.SenderPhotoUrl,
                opt => opt.MapFrom(source => source.Sender.Photos.FirstOrDefault(photo => photo.IsMain)!.Url))
            .ForMember(dto => dto.RecipientPhotoUrl,
                opt => opt.MapFrom(source => source.Recipient.Photos.FirstOrDefault(photo => photo.IsMain)!.Url));
    }
}
