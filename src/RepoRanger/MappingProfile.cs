using RepoRanger.Models;
using AutoMapper;

namespace RepoRanger;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GitHubRepository, Repository>();
    }
}
