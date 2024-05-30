using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FamilyCalendar.Common;

public class PartialViewRenderer(ICompositeViewEngine viewEngine,
      ITempDataProvider tempDataProvider,
      IServiceProvider serviceProvider) : IPartialViewRenderer
{
  private readonly ICompositeViewEngine _viewEngine = viewEngine;
  private readonly ITempDataProvider _tempDataProvider = tempDataProvider;
  private readonly IServiceProvider _serviceProvider = serviceProvider;

  public Task<string> RenderPartialViewToStringAsync(string viewName, object model)
  {
    var actionContext = GetActionContext();
    var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);
    return RenderPartialViewToStringAsync(viewName, model, actionContext, tempData);
  }

  public async Task<string> RenderPartialViewToStringAsync(string viewName, object model, ActionContext actionContext, ITempDataDictionary tempData)
  {
    var viewResult = _viewEngine.GetView(null, viewName, false);
    if (viewResult.View == null)
    {
      viewResult = _viewEngine.FindView(actionContext, viewName, false);
      if (viewResult.View == null)
      {
        throw new ArgumentNullException($"A view with the name {viewName} could not be found");
      }
    }

    var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
    {
      Model = model
    };

    using var stringWriter = new StringWriter();
    var viewContext = new ViewContext(
      actionContext,
      viewResult.View,
      viewData,
      tempData,
      stringWriter,
      new HtmlHelperOptions()
    );

    await viewResult.View.RenderAsync(viewContext);
    return stringWriter.ToString();
  }

  private ActionContext GetActionContext()
  {
    var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
    return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
  }
}
