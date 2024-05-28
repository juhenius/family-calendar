using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FamilyCalendar.Common;

public interface IPartialViewRenderer
{
  Task<string> RenderPartialViewToStringAsync(string viewName, object model);
  Task<string> RenderPartialViewToStringAsync(string viewName, object model, ActionContext actionContext, ITempDataDictionary tempData);
}
