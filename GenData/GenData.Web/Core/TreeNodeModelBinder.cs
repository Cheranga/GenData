using System.Web.Mvc;
using GenData.Reflector;
using Newtonsoft.Json;

namespace GenData.Web.Core
{
    public class TreeNodeModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var postedFormData = controllerContext.HttpContext.Request.Form;
            var typeMetaInfoInstance = JsonConvert.DeserializeObject<TypeMetaInfo>(postedFormData["typeMetaInfo"]);

            return typeMetaInfoInstance;
        }
    }
}