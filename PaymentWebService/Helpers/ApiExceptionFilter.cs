using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PaymentDTO;
using System.Collections.Generic;

namespace PaymentWebService
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var e = context.Exception;
            string message = "Server not available";
#if DEBUG
            message = e.Message;
#endif
            if (e is PaymentServiceException)
                message = e.Message;

            context.ExceptionHandled = true;
            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = 200;
            response.ContentType = "application/json";
            context.Result = new ObjectResult(new ResponseDTO { errorCode = (int)ERRORCODE.GENERALERROR, errors = new List<string>() { message } });
        }
    }
}
