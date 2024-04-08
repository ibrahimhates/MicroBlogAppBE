
using AutoMapper;
using MicroBlog.Core.Dtos.AuthDto.Request;
using MicroBlog.Core.Dtos.UserDto.Response;
using MicroBlog.Core.Entities;

namespace MicroBlog.Service.Mapping;

public class Mapper : Profile
{
    public Mapper()
    {
        CreateMap<RegisterRequest, User>();

        CreateMap<User, UserInfoResponse>()
            .ForMember(dest => dest.Follower, src => src.MapFrom(x => x.Followers.Count()))
            .ForMember(dest => dest.Following, src => src.MapFrom(x => x.Followings.Count()));

        CreateMap<User, UserFollowListResponse>();
    }
}