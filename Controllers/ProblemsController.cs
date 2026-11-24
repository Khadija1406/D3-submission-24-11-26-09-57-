using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PulseMvc.Models.ViewModels;
using PulseMvc.Repositories.Interfaces;

namespace PulseMvc.Controllers;

public class ProblemsController : Controller
{
    private readonly IProblemRepository _problemRepository;
    private readonly ILogger<ProblemsController> _logger;

    public ProblemsController(IProblemRepository problemRepository, ILogger<ProblemsController> logger)
    {
        _problemRepository = problemRepository;
        _logger = logger;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "user-alex-rivera-001";
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;
            var problems = (await _problemRepository.GetAllAsync(userId)).ToList();

            // If the current user has no problems yet (e.g. freshly registered),
            // provide all default seeded problems so they immediately see the records!
            if (!problems.Any())
            {
                problems = (await _problemRepository.GetAllAsync(null)).ToList();
            }

            return Json(problems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving problems");
            return StatusCode(500, new { message = "Error loading problems" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetById(string id)
    {
        var problem = await _problemRepository.GetByIdAsync(id);
        if (problem == null)
        {
            return NotFound(new { message = "Problem not found" });
        }
        return Json(problem);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProblemRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { success = false, message = "Title is required" });
        }

        try
        {
            var userId = GetCurrentUserId();
            var newId = await _problemRepository.CreateAsync(request, userId);
            return Json(new { success = true, id = newId, message = "Problem saved ✨" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating problem");
            return StatusCode(500, new { success = false, message = "Could not save problem" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Id))
        {
            return BadRequest(new { success = false, message = "Invalid request" });
        }

        var success = await _problemRepository.UpdateStatusAsync(request.Id, request.Status);
        return Json(new { success });
    }

    [HttpPost]
    public async Task<IActionResult> AddOccurrence([FromBody] AddOccurrenceRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.ProblemId))
        {
            return BadRequest(new { success = false, message = "Invalid request" });
        }

        var date = string.IsNullOrWhiteSpace(request.Date) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : request.Date;
        var success = await _problemRepository.AddOccurrenceAsync(request.ProblemId, date, request.Note ?? "");
        return Json(new { success });
    }

    [HttpPost]
    public async Task<IActionResult> SetTriedSolution([FromBody] SetTriedSolutionRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.ProblemId) || string.IsNullOrEmpty(request.SolutionKey))
        {
            return BadRequest(new { success = false, message = "Invalid request" });
        }

        var success = await _problemRepository.SetTriedSolutionAsync(request.ProblemId, request.SolutionKey, request.Status, request.Note ?? "");
        return Json(new { success });
    }

    [HttpDelete]
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest(new { success = false, message = "ID is required" });
        }

        var success = await _problemRepository.DeleteAsync(id);
        return Json(new { success, message = success ? "Problem deleted" : "Problem not found" });
    }
}
