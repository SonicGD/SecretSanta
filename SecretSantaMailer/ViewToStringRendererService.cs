using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace SecretSantaMailer
{
    public class ViewRenderService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;

        public ViewRenderService(IServiceScopeFactory scopeFactory, IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider)
        {
            _scopeFactory = scopeFactory;
            _viewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            //_serviceProvider = serviceProvider;
        }

        public async Task<string> RenderToStringAsync<T>(string viewName, T model)
        {
            var actionContext = GetActionContext();
            var view = FindView(actionContext, viewName);

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<T>(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                return output.ToString();
            }
        }

        private IView FindView(ActionContext actionContext, string viewName)
        {
            var getViewResult = _viewEngine.GetView(null, viewName, true);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = _viewEngine.FindView(actionContext, viewName, true);
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
            var errorMessage = string.Join(
                Environment.NewLine,
                new[] {$"Unable to find view '{viewName}'. The following locations were searched:"}.Concat(
                    searchedLocations));

            throw new InvalidOperationException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext {RequestServices = _scopeFactory.CreateScope().ServiceProvider};
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}
