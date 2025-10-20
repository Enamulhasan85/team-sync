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
            return Ok(ApiResponse<T>.Ok(data, message));
        }

        protected IActionResult FailureResponse(string message = "Error", List<string>? errors = null)
        {
            return BadRequest(ApiResponse<object?>.Fail(null, message, errors));
        }

        protected IActionResult FailureResponse(List<string> errors)
        {
            return BadRequest(ApiResponse<object?>.Fail(null, "Error", errors));
        }
    }
}
