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

        protected IActionResult FailureResponse<T>(T? data, string message = "Error", List<string>? errors = null)
        {
            return BadRequest(ApiResponse<T>.Fail(data, message, errors));
        }

    }
}
