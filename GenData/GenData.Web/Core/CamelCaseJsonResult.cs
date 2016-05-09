using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GenData.Web.Core
{
    public class CamelCaseJsonResult : ActionResult
    {
        private readonly object response;

        private readonly JsonSerializerSettings serializationSettings;

        public CamelCaseJsonResult(object response)
        {
            this.response = response;
            this.serializationSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var httpResponse = context.HttpContext.Response;


            httpResponse.ContentType = "application/json";

            httpResponse.Write(JsonConvert.SerializeObject(response, this.serializationSettings));

        }
    }
}