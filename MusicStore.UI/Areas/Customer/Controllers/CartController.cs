using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using MusicStore.Models.ViewModels;
using MusicStore.Utility;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MusicStore.UI.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(IUnitOfWork uow, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _uow = uow;
            _emailSender = emailSender;
            _userManager = userManager;

            /* Inject işlemi ile beraber classlara olan bağımlılığımızı mümkün mertebe minimuma indirmiş oluyoruz.
             * Artık biz interfaceler ile konuşuyoruz ve class bağımlılığımız ortadan kalkmış oluyor
             */
        }

        public ShoppingCartVM ShoppingCartVM { get; set; }

        public IActionResult Index()
        {
            // Hangi kullanıcı ile işlem yapıyoruz onun bilgilerini alıyoruz bu işlemde

            var claimsIdentity = (ClaimsIdentity)User.Identity; // Kullanıcıların bilgilerini getirdik
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                Order = new Order(),
                ShoppingCarts = _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value, includeProperty: "Product")
            };

            ShoppingCartVM.Order.OrderTotal = 0;
            ShoppingCartVM.Order.ApplicationUser = _uow.ApplicationUser
                .GetFirstOrDefault(u => u.Id == claims.Value, includeProperties: "Company");

            foreach (var cart in ShoppingCartVM.ShoppingCarts)
            {
                cart.Price = ProjectConstant.GetPriceBaseOnQuantity(cart.Count, cart.Product.Price);
                ShoppingCartVM.Order.OrderTotal += (cart.Product.Price * cart.Count);
                cart.Product.Description = ProjectConstant.ConvertToRawHtml(cart.Product.Description);

                cart.Product.Description = cart.Product.Description.Length > 50 ? cart.Product.Description.Substring(0, 49) + "..." : cart.Product.Description;
                // 50den büyük ise 49'a kadar var olarak yap

                //if (cart.Product.Description.Length > 50) // açıklamanın uzunluğu 50 karakterden fazla ise
                //{
                //    cart.Product.Description = cart.Product.Description.Substring(0, 49) + "...";
                //}
            }

            // cart.Price : Price bilgisi database de yok sadece ön tarafta işlem yaparken kullanmak için ekledik

            return View(ShoppingCartVM);
        }

        [HttpOptions]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity; // Kullanıcıların bilgilerini getirdik
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _uow.ApplicationUser.GetFirstOrDefault(u => u.Id == claims.Value);

            if (user == null)
                ModelState.AddModelError(string.Empty, "Verification email im empty!");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email");

            return RedirectToAction("Index");

        }

        public IActionResult Plus(int cartId)
        {
            var cart = _uow.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, "Product"); // Kullanıcının aldığı ürüne ulaştık

            if (cart != null) // Ürün kontrolü yapıyoruz
            {
                cart.Count += 1;
                cart.Price = ProjectConstant.GetPriceBaseOnQuantity(cart.Count, cart.Product.Price);
                _uow.Save();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _uow.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, "Product");

            if (cart != null)
            {
                var carts = _uow.ShoppingCart.GetAll(c => c.Id == cartId).Count();
                if (cart.Count == 1)
                {
                    _uow.ShoppingCart.Remove(cart);
                    HttpContext.Session.SetInt32(ProjectConstant.Shopping_Cart, (carts - 1));
                }
                else
                {
                    cart.Count -= 1;
                    cart.Price = ProjectConstant.GetPriceBaseOnQuantity(cart.Count, cart.Product.Price);
                }
                _uow.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _uow.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);

            var carts = _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).Count();

            _uow.ShoppingCart.Remove(cart);
            _uow.Save();
            HttpContext.Session.SetInt32(ProjectConstant.Shopping_Cart, (carts - 1));

            return RedirectToAction(nameof(Index));
        }
    }
}
