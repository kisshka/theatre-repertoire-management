using System.Linq.Expressions;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.DTOs.Events;

namespace TheatreManagement.Server.Mappings
{
    public static class InstitutionMappings
    {
        public static Expression<Func<Institution, InstitutionDto>> ToInstitutionDto = i => new InstitutionDto
        {
            InstitutionId = i.InstitutionId,
            Name = i.Name,
            Town = i.Town,
            Street = i.Street,
            House = i.House,
            PhoneNumber = i.PhoneNumber,
            Comment = i.Comment,
            InstitutionTypeName = i.Type.Name,
            InstitutionTypeId = i.Type.InstitutionTypeId,
        };

    }
}
