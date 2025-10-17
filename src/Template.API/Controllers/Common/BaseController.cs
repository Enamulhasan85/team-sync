using Microsoft.AspNetCore.Mvc;
using Template.API.Models;
using Template.API.Models.Common;

namespace Template.API.Controllers.Common
{
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult SuccessResponse<T>(T data, string message = "Success")
        {
            return Ok(new ApiResponse<T>(data, message));
        }

        protected IActionResult SuccessResponse(string message = "Success")
        {
            return Ok(new ApiResponse(message));
        }

        protected IActionResult NotFoundResponse(string message = "Resource not found")
        {
            return NotFound(new ApiResponse(message));
        }

        protected IActionResult BadRequestResponse(string message = "Bad request")
        {
            return BadRequest(new ApiResponse(message));
        }

        protected IActionResult BadRequestResponse(IEnumerable<string> errors)
        {
            return BadRequest(new ApiResponse(errors.ToList()));
        }

        protected IActionResult CreatedResponse<T>(T data, string message = "Resource created successfully")
        {
            return StatusCode(201, new ApiResponse<T>(data, message));
        }
    }
}
