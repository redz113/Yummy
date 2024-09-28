using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace AppFoods.Utilities
{
    public static class ViewService
    {
        //Rendor View as string
        //Example use...This call will look for Views/Emails/Test.cshtml and set a string model
        //var test = await this.RenderViewAsync("Test", "I would like to inject this as my model!", false);

        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, RouteData routeData,
                                                                 string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());

            controller.ViewData.Model = model;
            controller.ControllerContext.ActionDescriptor = new ControllerActionDescriptor();
            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(actionContext, viewName, !partial);

                if (!viewResult.Success)
                {
                    throw new Exception("Could not find email template.");
                }
                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
