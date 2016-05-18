using System.Web.Mvc;
using GenData.Reflector;
using Newtonsoft.Json;

namespace GenData.Web.Core
{
    public class SubmittedTypeDataModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var postedFormData = controllerContext.HttpContext.Request.Form;
            var assemblyType = postedFormData["AssemblyType"];
            var typeMetaInfoInstance = JsonConvert.DeserializeObject<TypeMetaInfo>(postedFormData["typeMetaInfo"]);

            return new SubmittedTypeData
            {
                AssemblyType = assemblyType,
                TypeMetaInfo = typeMetaInfoInstance
            };
        }
    }
}