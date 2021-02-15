using GoalManagement.Domain.Entities;
using GoalManagement.Repository.Context;
using GoalManagement.Repository.Implementations;
using GoalManagement.Web.Models.Goal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GoalManagement.Domain.Interfaces;
using Microsoft.Extensions.Localization;
using GoalManagement.BLogic.Extensions;
using GoalManagement.BLogic;

namespace GoalManagement.Web.Controllers
{
    public class GoalController : Controller
    {
        private readonly IRepository<Goal> repository;
        private readonly IStringLocalizer<GoalController> localizer;

        public GoalController(GoalManagementContext _context, IStringLocalizer<GoalController> _localizer)
        {
            repository = new DBRepository<Goal>(_context);
            localizer = _localizer;
        }

        [ActionName("Index")]
        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            InitResource();

            var goals = await repository.FindAll()
                                        .Include(g => g.Parent)
                                        .ToListAsync();

            var nodesTreeView = InitTreeView(goals);

            ViewBag.Json = JsonConvert.SerializeObject(nodesTreeView);

            return View();
        }

        [ActionName("GetGoal")]
        [HttpGet]
        public async Task<ActionResult> GetGoalAsync(string idItem)
        {
            InitResource();

            idItem = string.Concat(idItem.Where(c => c >= '0' && c <= '9'));

            if (!int.TryParse(idItem, out var id))
            {
                id = 1;
            }

            var goals = await repository.FindAll()
                                        .Include(g => g.Parent)
                                        .ToListAsync();

            var showGoal = goals.Where(g => g.Id == id)
                                .First();

            var subGoals = GoalBLogic.GetAllSubGoals(showGoal, goals);

            var (plannedWorkLoad, actualExecutionTime) = GoalBLogic.CalculatePlannedWorkLoadAndActualExecutionTime(showGoal, goals);

            var goalIndexModel = new GoalIndexModel
            {
                Id = showGoal.Id,
                Name = showGoal.Name,
                Description = showGoal.Description,
                ArtistList = showGoal.ArtistList,
                RegistrationDate = showGoal.RegistrationDate,
                Status = showGoal.Status,
                PlannedWorkLoad = plannedWorkLoad,
                ActualExecutionTime = actualExecutionTime,
                ParentId = showGoal.ParentId,
                EndDate = showGoal.EndDate
            };

            var subGoalIndexModels = new List<GoalIndexModel>();

            foreach (var subGoal in subGoals)
            {
                subGoalIndexModels.Add(new GoalIndexModel
                {
                    Id = subGoal.Id,
                    Name = subGoal.Name,
                    Description = subGoal.Description,
                    ArtistList = subGoal.ArtistList,
                    RegistrationDate = subGoal.RegistrationDate,
                    Status = subGoal.Status,
                    PlannedWorkLoad = subGoal.PlannedWorkLoad,
                    ActualExecutionTime = subGoal.ActualExecutionTime,
                    ParentId = subGoal.ParentId,
                    EndDate = subGoal.EndDate
                });
            }

            return PartialView((goalIndexModel, subGoalIndexModels));
        }

        [HttpGet]
        public IActionResult Add(int parentId)
        {
            InitResource();
            return View(new GoalAddModel { ParentId = parentId });
        }

        [ActionName("Add")]
        [HttpPost]
        public async Task<IActionResult> AddAsync(GoalAddModel goalAddModel)
        {
            var goal = new Goal
            {
                Name = goalAddModel.Name,
                Description = goalAddModel.Description,
                ArtistList = goalAddModel.ArtistList,
                PlannedWorkLoad = goalAddModel.PlannedWorkLoad,
                ActualExecutionTime = goalAddModel.ActualExecutionTime,
                EndDate = goalAddModel.EndDate
            };

            if (goalAddModel.ParentId != 0)
            {
                goal.ParentId = goalAddModel.ParentId;
            }

            repository.Add(goal);
            await repository.SaveAsync();

            return RedirectToAction("Index", "Goal");
        }

        [ActionName("Edit")]
        [HttpGet]
        public async Task<IActionResult> EditAsync(int goalId)
        {
            InitResource();

            var goal = await repository.FindByIdAsync(goalId);

            if (goal == null)
            {
                return NotFound();
            }

            var goalEditModel = new GoalEditModel()
            {
                Id = goal.Id,
                Name = goal.Name,
                Description = goal.Description,
                ArtistList = goal.ArtistList,
                Status = goal.Status,
                EndDate = goal.EndDate
            };

            return View(goalEditModel);
        }

        [ActionName("Edit")]
        [HttpPost]
        public async Task<IActionResult> EditAsync(GoalEditModel goalEditModel)
        {
            InitResource();

            var goals = await repository.FindAll()
                                        .Include(g => g.Parent)
                                        .ToListAsync();

            var goal = goals.Where(g => g.Id == goalEditModel.Id)
                            .First();

            if (goal == null)
            {
                return NotFound();
            }

            var subGoals = GoalBLogic.GetAllSubGoals(goal, goals);

            var oldStatus = goal.Status;
            var newStatus = goalEditModel.Status;

            var errorMessage = @"Статус ""Suspended"" и ""Completed"" 
                                может быть присвоен только после статуса ""Executed"".";

            if (newStatus == GoalStatus.Suspended && !IsChangedStatusValid(oldStatus, newStatus))
            {
                ModelState.AddModelError("Status", errorMessage);
            }

            if (newStatus == GoalStatus.Completed)
            {
                if (!IsChangedStatusValid(oldStatus, newStatus))
                {
                    ModelState.AddModelError("Status", errorMessage);
                }
                else
                {
                    foreach (var subGoal in subGoals)
                    {
                        oldStatus = subGoal.Status;

                        if (!IsChangedStatusValid(oldStatus, newStatus))
                        {
                            ModelState.AddModelError("", @"В одной из подзадачи статус ""Suspended"" или ""Completed"".");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                if (newStatus == GoalStatus.Completed)
                {
                    foreach (var subGoal in subGoals)
                    {
                        subGoal.Status = newStatus;

                        repository.Update(subGoal);
                    }
                }

                goal.Name = goalEditModel.Name;
                goal.Description = goalEditModel.Description;
                goal.ArtistList = goalEditModel.ArtistList;
                goal.Status = goalEditModel.Status;
                goal.EndDate = goalEditModel.EndDate;

                repository.Update(goal);

                await repository.SaveAsync();

                return RedirectToAction("Index", "Goal");
            }

            return View(goalEditModel);
        }

        [ActionName("Delete")]
        [HttpGet]
        public async Task<IActionResult> DeleteAsync(int goalId)
        {
            InitResource();

            var goal = await repository.FindByIdAsync(goalId);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        [ActionName("Delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteAsync(Goal goal)
        {
            InitResource();

            var goals = await repository.FindAll()
                                        .Include(g => g.Parent)
                                        .ToListAsync();

            var maybeDeleteGoal = goals.Where(g => g.Id == goal.Id)
                                       .First();

            if (maybeDeleteGoal.SubGoals.Count != 0)
            {
                ModelState.AddModelError("", "У этой задачи есть подзадачи.");
            }

            if (ModelState.IsValid)
            {
                repository.Remove(goal.Id);

                await repository.SaveAsync();

                return RedirectToAction("Index", "Goal");
            }

            return View(goal);
        }

        private void InitResource()
        {
            ViewData["IndexHeader"] = localizer["IndexHeader"];
            ViewData["IndexAddGoal"] = localizer["IndexAddGoal"];
            ViewData["Name"] = localizer["Name"];
            ViewData["Description"] = localizer["Description"];
            ViewData["ArtistList"] = localizer["ArtistList"];
            ViewData["RegistrationDate"] = localizer["RegistrationDate"];
            ViewData["Status"] = localizer["Status"];
            ViewData["PlannedWorkLoad"] = localizer["PlannedWorkLoad"];
            ViewData["ActualExecutionTime"] = localizer["ActualExecutionTime"];
            ViewData["EndDate"] = localizer["EndDate"];
            ViewData["GetGoalSubGoal"] = localizer["GetGoalSubGoal"];
            ViewData["Edit"] = localizer["Edit"];
            ViewData["Delete"] = localizer["Delete"];
            ViewData["Add"] = localizer["Add"];
            ViewData["Return"] = localizer["Return"];
            ViewData["AddSubGoal"] = localizer["AddSubGoal"];
            ViewData["DeleteText"] = localizer["Delete"];
            ViewData["Cancel"] = localizer["Cancel"];
        }

        private IEnumerable<NodeTreeView> InitTreeView(IEnumerable<Goal> goals)
        {
            var nodesTreeView = new List<NodeTreeView>();

            foreach (var goal in goals)
            {
                if (goal.ParentId.HasValue)
                {
                    nodesTreeView.Add(new NodeTreeView { id = $"{goal.Id}", parent = goal.ParentId.ToString(), text = goal.Name });
                }
                else
                {
                    nodesTreeView.Add(new NodeTreeView { id = goal.Id.ToString(), parent = "#", text = goal.Name });
                }
            }

            return nodesTreeView;
        }

        private bool IsChangedStatusValid(GoalStatus oldStatus, GoalStatus newStatus)
        {
            var IsAssignedToSuspended = oldStatus == GoalStatus.Assigned && newStatus == GoalStatus.Suspended;
            var IsAssignedToCompleted = oldStatus == GoalStatus.Assigned && newStatus == GoalStatus.Completed;
            var IsSuspendedToCompleted = oldStatus == GoalStatus.Suspended && newStatus == GoalStatus.Completed;
            var IsCompletedToSuspended = oldStatus == GoalStatus.Completed && newStatus == GoalStatus.Assigned;

            if (IsAssignedToSuspended || IsAssignedToCompleted || IsSuspendedToCompleted || IsCompletedToSuspended)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
