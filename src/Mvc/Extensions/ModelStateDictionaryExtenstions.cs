using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mvc.Extensions;

public static class ModelStateDictionaryExtenstions
{
    public static List<string> CollectErrorMessages(this ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
            return new List<string>();

        return modelState
            .Values
            .SelectMany(entry => entry.Errors)
            .Select(err => err.ErrorMessage)
            .Where(msg => !string.IsNullOrEmpty(msg))
            .ToList();
    }
}
