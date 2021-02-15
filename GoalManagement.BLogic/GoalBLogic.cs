using System;
using System.Collections.Generic;
using System.Text;
using GoalManagement.Domain.Entities;
using System.Linq;
using GoalManagement.BLogic.Extensions;

namespace GoalManagement.BLogic
{
    public static class GoalBLogic
    {
        public static (double, double) CalculatePlannedWorkLoadAndActualExecutionTime(Goal goal, List<Goal> goals)
        {
            var subGoals = GetAllSubGoals(goal, goals);

            var plannedWorkLoad = subGoals == null
                                ? goal.PlannedWorkLoad
                                : subGoals.Aggregate(goal.PlannedWorkLoad, (x, y) => x + y.PlannedWorkLoad);

            var actualExecutionTime = subGoals == null
                                    ? goal.ActualExecutionTime
                                    : subGoals.Aggregate(goal.ActualExecutionTime, (x, y) => x + y.ActualExecutionTime);

            return (plannedWorkLoad, actualExecutionTime);
        }

        public static IEnumerable<Goal> GetAllSubGoals(Goal goal, List<Goal> goals)
        {
            return goals.ToTree((p, c) => c.ParentId == p.Id)
                        .ToTreeEnumerable()
                        .Where(d => d.Data.Id == goal.Id)
                        .First()
                        .GetAllDescendants();
        }
    }
}
