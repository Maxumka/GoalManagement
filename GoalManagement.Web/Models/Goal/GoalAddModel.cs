using System;
using System.ComponentModel.DataAnnotations;

namespace GoalManagement.Web.Models.Goal
{
    public class GoalAddModel
    {
        public int? ParentId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ArtistList { get; set; }

        public double PlannedWorkLoad { get; set; }

        public double ActualExecutionTime { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
