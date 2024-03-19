
using AutoMapper;
using MicroBlog.Core.Dtos.AuthDto.Request;
using MicroBlog.Core.Entities;

namespace MicroBlog.Service.Mapping;

public class Mapper : Profile
{
    public Mapper()
    {
        CreateMap<RegisterRequest, User>();
    }
}