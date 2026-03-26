using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Registration.Models;

namespace Registration.Controllers;

public class RegistrationController : Controller
{
    private readonly IRegistrationService _service;

    public RegistrationController(IRegistrationService service)
    {
        _service = service;
    }

    [HttpGet("/registration/start")]
    public IActionResult Start()
    {
        return View("Step1", new RegistrationStep1ViewModel());
    }

    [HttpPost("/registration/start")]
    public async Task<IActionResult> Start(RegistrationStep1ViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View("Step1", vm);

        var result = await _service.StartAsync(
            new Step1Data(vm.FirstName, vm.LastName, vm.Email),
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            ct
        );

        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Error);
            return View("Step1", vm);
        }

        return RedirectToAction("Step2", new { resumeToken = result.Value });
    }

    [HttpGet("/registration/step2")]
    public async Task<IActionResult> Step2(string resumeToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resumeToken))
            return RedirectToAction("Start");

        var regResult = await _service.GetByResumeTokenAsync(resumeToken, ct);
        if (regResult.IsFailure)
            return View("Error", regResult.Error);

        return View("Step2", new RegistrationStep2ViewModel
        {
            ResumeToken = resumeToken
        });
    }

    [HttpPost("/registration/step2")]
    public async Task<IActionResult> Step2(RegistrationStep2ViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View("Step2", vm);

        var result = await _service.SubmitStep2Async(
            vm.ResumeToken,
            new Step2Data(vm.Age, vm.City, vm.PostalCode),
            ct
        );

        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Error);
            return View("Step2", vm);
        }

        return RedirectToAction("Step3", new { resumeToken = vm.ResumeToken });
    }

    [HttpGet("/registration/step3")]
    public async Task<IActionResult> Step3(string resumeToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resumeToken))
            return RedirectToAction("Start");

        var regResult = await _service.GetByResumeTokenAsync(resumeToken, ct);
        if (regResult.IsFailure)
            return View("Error", regResult.Error);

        var reg = regResult.Value;

        return View("Step3", new RegistrationStep3ViewModel
        {
            IsMinor = reg.IsMinor.Value,
            ResumeToken = resumeToken,
        });
    }

    [HttpPost("/registration/step3")]
    public async Task<IActionResult> Step3(RegistrationStep3ViewModel vm, CancellationToken ct)
    {
        var regResult = await _service.GetByResumeTokenAsync(vm.ResumeToken, ct);
        if (regResult.IsFailure)
            return View("Error", regResult.Error);

        var reg = regResult.Value;

        if (reg.IsMinor.HasValue && reg.IsMinor.Value && string.IsNullOrWhiteSpace(vm.ParentName))
            ModelState.AddModelError("ParentName", "Imię rodzica jest wymagane dla osób < 18 lat.");

        if (!ModelState.IsValid)
            return View("Step3", vm);

        var result = await _service.SubmitStep3AndCompleteAsync(
            regResult.Value,
            new Step3Data(vm.ParentName, vm.ConsentGiven),
            ct
        );

        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Error);
            return View("Step3", vm);
        }

        return RedirectToAction("Step4", new { resumeToken = vm.ResumeToken });
    }

    [HttpGet("/registration/step4")]
    public async Task<IActionResult> Step4(string resumeToken, CancellationToken ct)
    {
        var regResult = await _service.GetByResumeTokenAsync(resumeToken, ct);
        if (regResult.IsFailure)
            return View("Error", regResult.Error);

        var reg = regResult.Value;

        return View("Step4", new RegistrationStep4ViewModel
        {
            FirstName = reg.FirstName,
            AssignedNumber = reg.AssignedNumber
        });
    }

    [HttpGet("/registration/resume/{resumeToken}")]
    public async Task<IActionResult> Resume(string resumeToken, CancellationToken ct)
    {
        var regResult = await _service.GetByResumeTokenAsync(resumeToken, ct);
        if (regResult.IsFailure)
            return View("Error", regResult.Error);

        var reg = regResult.Value;

        return reg.CurrentStep switch
        {
            1 => RedirectToAction("Step2", new { resumeToken = resumeToken }),
            2 => RedirectToAction("Step3", new { resumeToken = resumeToken }),
            _ => RedirectToAction("Step4", new { resumeToken = resumeToken })
        };
    }
}
