using Microsoft.AspNetCore.Mvc;

namespace StreamChatInator
{
    public static class ResponseHelper
    {
        public static IActionResult Response502(string errorMessage)
        {
            return new ObjectResult(new {error=errorMessage})
            {
                StatusCode = 502
            };
        }

        public static IActionResult OkStatus(string status)
        {
            return new ObjectResult(new { status })
            {
                StatusCode = 200
            };
        }

        public static IActionResult OkStatusUsername(string status,string username)
        {
            return new ObjectResult(new { status,username })
            {
                StatusCode = 200
            };
        }
    }
}
