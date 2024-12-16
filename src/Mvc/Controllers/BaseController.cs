using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace Mvc.Controllers;

[Authorize]
public class BaseController<T> : Controller
{
    private IMediator _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

    protected readonly ILogger<T> _logger;
    private readonly IToastNotification _toastNotification;

    public BaseController(IToastNotification toastNotification, ILogger<T> logger)
    {
        _toastNotification = toastNotification;
        _logger = logger;
    }

    public void ShowErrorToast(List<string>? messages)
    {
        ShowErrorToast("", messages);
    }

    public void ShowErrorToast(string title, List<string>? messages)
    {
        if (messages == null || messages.Count == 0)
            return;

        if (string.IsNullOrEmpty(title))
            title = "錯誤";

        foreach (var message in messages)
        {
            _toastNotification.AddErrorToastMessage(message, new ToastrOptions()
            {
                Title = title
            });
        }
    }
}
