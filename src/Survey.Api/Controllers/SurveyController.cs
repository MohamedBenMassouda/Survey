using Microsoft.AspNetCore.Mvc;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Api.Controllers;

public class SurveyController(IUnitOfWork unitOfWork) : BaseController<SurveyModel>(unitOfWork)
{
    [HttpPost]
    public IActionResult Create([FromBody] SurveyModel model)
    {
        // Custom logic for creating a survey can be added here

        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }
}