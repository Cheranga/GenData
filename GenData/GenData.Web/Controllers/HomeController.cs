using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using GenData.Reflector;
using GenData.Web.Core;

namespace GenData.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult GetMetaInfoForType(string type)
        {
            //
            // TODO: Get the type from a loaded assembly
            //
            if (string.IsNullOrEmpty(type))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            var typeToRefer = Type.GetType(type, false);
            if (typeToRefer == null)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            //
            // TODO: Add Dipendency Injection
            //
            var reflector = new PocoClassReflector();

            var metaInfoForType = reflector.GetMetaInfoForType(typeToRefer);

            return new CamelCaseJsonResult(metaInfoForType);
        }

        [HttpPost]
        public ActionResult CreateObjectRepresentation([ModelBinder(typeof(TreeNodeModelBinder))] TypeMetaInfo typeMetaInfo)
        {
            var result = new PocoClassReflector().CreateObject(typeMetaInfo);

            var serializedData = string.Empty;
            var serializer = new DataContractSerializer(result.GetType());

            using (var sw = new StringWriter())
            {
                using (var xw = XmlWriter.Create(sw,new XmlWriterSettings{Indent = true}))
                {
                    serializer.WriteObject(xw, result);
                }

                serializedData = sw.ToString();
            }

            //var xmlDoc = new XmlDocument();
            //xmlDoc.LoadXml(serializedData);


            return Content(serializedData, "text/xml");
        }
        
    }
}