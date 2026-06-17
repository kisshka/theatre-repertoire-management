using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs.Events
{
    public class StationarDto
    {
        public int StationarId { get; set; }
        public int HallTypeId { get; set; }
        public string? HallTypeName { get; set; }
        public string? Type { get; set; }
    }

    public class TourDto
    {
        public int TourId { get; set; }
        public string? Country { get; set; }
        public string? Area { get; set; }
    }

    public class InstitutionDto
    {
        public int InstitutionId { get; set; }
        [DisplayName("Название")]
        public string? Name { get; set; }

        [DisplayName("Город")]
        public string? Town { get; set; }

        [DisplayName("Улица")]
        public string? Street { get; set; }

        [DisplayName("Дом")]
        public string? House { get; set; }

        [DisplayName("Номер телефона")]
        public string? PhoneNumber { get; set; }
        [DisplayName("Заметка")]
        public string? Comment { get; set; }

        public int InstitutionTypeId { get; set; }
        [DisplayName("Тип")]
        public string InstitutionTypeName { get; set; } = string.Empty;

        // Для отображения
        public string FullAdress
        {
            get
            {
                return $"г.{Town} ул.{Street} дом {House}";
            }
        }
        //public string CommentView
        //{
        //    get
        //    {
        //        if (!string.IsNullOrWhiteSpace(Comment) && Comment.Length > 50)
        //        {
        //            return Comment.Substring(0, 50) + "...";
        //        }
        //        return Comment ?? string.Empty;
        //    }
        //}
    }

    public class HallTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class InstitutionTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
