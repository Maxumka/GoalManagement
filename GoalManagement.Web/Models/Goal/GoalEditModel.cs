using System;
using System.ComponentModel.DataAnnotations;
using GoalManagement.Domain.Entities;

namespace GoalManagement.Web.Models.Goal
{
    public class GoalEditModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ArtistList { get; set; }

        public GoalStatus Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
