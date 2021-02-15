using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace GoalManagement.Domain.Entities
{
    public enum GoalStatus
    {
        Assigned,
        Executed,
        Suspended,
        Completed
    }

    public sealed class Goal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ArtistList { get; set; }

        private DateTime registrationDate = DateTime.UtcNow;
        public DateTime RegistrationDate 
        {
            get => registrationDate;
            set => registrationDate = value;
        }

        private GoalStatus status = GoalStatus.Assigned;
        public GoalStatus Status 
        {
            get => status;
            set => status = value;
        }

        public double PlannedWorkLoad { get; set; }

        public double ActualExecutionTime { get; set; }

        public DateTime EndDate { get; set; }

        public Goal Parent { get; set; }

        public int? ParentId { get; set; }

        public ICollection<Goal> SubGoals { get; set; } = new List<Goal>();
    }
}
