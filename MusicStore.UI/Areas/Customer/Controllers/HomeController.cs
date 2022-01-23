using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using MusicStore.Models.ViewModels;
using MusicStore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MusicStore.UI.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _uow;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork uow)
        {
            _logger = logger;
            _uow = uow;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _uow.Product.GetAll(includeProperty: "Category,CoverType");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var shoppingCount = _uow.ShoppingCart.GetAll(a => a.ApplicationUserId == claim.Value).Count();

                HttpContext.Session.SetInt32(ProjectConstant.Shopping_Cart, shoppingCount);
                // count olarak set ediyoruz
            }

            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = _uow.Product.GetFirstOrDefault(p => p.Id == id, "Category,CoverType");
            // tek bir ürün getireceğimiz için bu yöntemi kullanıyoruz

            if (product != null)
            {
                ShoppingCart cart = new ShoppingCart()
                {
                    Product = product,
                    ProductId = product.Id
                };

                return View(cart);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Geti çağırılmadan postuna işlem yapmasını engeller
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            cart.Id = 0;
            if (ModelState.IsValid)
            {
                // ClaismIdentity = Kullanıcıların özelliklerinin tutulduğu bir alan
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.ApplicationUserId = claim.Value;

                ShoppingCart fromDb = _uow.ShoppingCart.GetFirstOrDefault(
                    s => s.ApplicationUserId == cart.ApplicationUserId
                    && s.ProductId == cart.ProductId,
                    "Product");

                // aynı ürün bir kere daha var mı var ise güncelle yok ise ekle

                if (fromDb == null)
                {
                    _uow.ShoppingCart.Add(cart);
                }
                else
                {
                    fromDb.Count = cart.Count;
                }
                _uow.Save();

                var shoppincCount = _uow.ShoppingCart.GetAll(a => a.ApplicationUserId == cart.ApplicationUserId).Count(); // sayısını dön

                HttpContext.Session.SetInt32(ProjectConstant.Shopping_Cart, shoppincCount); // key, value
                // bu metodun içerisine ekliyeceğiz

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(cart);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
