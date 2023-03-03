using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cognito.Auth.Mvc.Webapp.Controllers
{
    
    public class ProductsController : Controller
    {
        [Authorize]
        // GET: ProductsController
        public ActionResult Index()
        {
            return View();
        }

        
    }
}
