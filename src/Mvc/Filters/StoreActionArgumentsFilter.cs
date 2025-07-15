using Microsoft.AspNetCore.Mvc.Filters;

namespace Mvc.Filters;

public class StoreActionArgumentsFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // 將整包 arguments 放到 HttpContext.Items，key 自行命名
        context.HttpContext.Items["__ActionArguments__"] = context.ActionArguments;
        base.OnActionExecuting(context);
    }
}
