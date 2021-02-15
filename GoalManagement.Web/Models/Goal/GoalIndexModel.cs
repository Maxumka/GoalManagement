using System;
using System.ComponentModel.DataAnnotations;
using GoalManagement.Domain.Entities;

namespace GoalManagement.Web.Models.Goal
{
    public class NodeTreeView
    {
        public string id { get; set; }

        public string parent { get; set; }

        public string text { get; set; }
    }

    public class GoalIndexModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ArtistList { get; set; }

        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; }

        public GoalStatus Status { get; set; }

        public double PlannedWorkLoad { get; set; }

        public double ActualExecutionTime { get; set; }

        public int? ParentId { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
