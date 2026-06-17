using System.Linq.Expressions;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared.DTOs.Events;

namespace TheatreManagement.Server.Mappings
{
    public static class PlayMappings
    {
        public static Expression<Func<Play, PlayDto>> ToPlayDto = p => new PlayDto
        {
            PlayId = p.PlayId,
            Name = p.Name,
            Duration = p.Duration,
            AgeCategory = p.AgeCategory,
            IsActive = p.IsActive,
            LastEditTime = p.LastEditTime,
            SceneTypeName = p.SceneType.Name,
            SceneTypeId = p.SceneType.SceneTypeId,
            UserFullName = (p.User != null ? p.User.Surname + " " + p.User.Name + " " + p.User.FatherName : ""),
            IsUsed = p.PlayEvents.Any(pe => pe.Event != null && pe.Event.DeletionTime == null)
        };

    }
}
