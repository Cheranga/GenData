using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

            var metaInfoForType = new Reflector.Reflector().GetMetaInfoForType(typeToRefer);

            return new CamelCaseJsonResult(metaInfoForType);
        }

        [HttpPost]
        public ActionResult CreateObjectRepresentation([ModelBinder(typeof(TreeNodeModelBinder))] TypeMetaInfo typeMetaInfo)
        {
            return RedirectToAction("About");
        }
    }
}