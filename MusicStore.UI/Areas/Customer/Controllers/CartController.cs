using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using MusicStore.Models.ViewModels;
using MusicStore.Utility;
using Stripe;
using System;
using System.Collections.Generic;
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

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public IActionResult Index()
        {
            // Hangi kullanıcı ile işlem yapıyoruz onun bilgilerini alıyoruz bu işlemde

            var claimsIdentity = (ClaimsIdentity)User.Identity; // Kullanıcıların bilgilerini getirdik
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                Order = new Models.Models.Order(),
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
                cart.Count += 1;
            cart.Price = ProjectConstant.GetPriceBaseOnQuantity(cart.Count, cart.Product.Price);
            _uow.Save();

            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _uow.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, "Product");

            if (cart.Count == 1)
            {
                var carts = _uow.ShoppingCart.GetAll(c => c.Id == cartId).Count();
                _uow.ShoppingCart.Remove(cart);
                _uow.Save();
                HttpContext.Session.SetInt32(ProjectConstant.Shopping_Cart, (carts - 1));
            }
            else
            {
                cart.Count -= 1;
                cart.Price = ProjectConstant.GetPriceBaseOnQuantity(cart.Count, cart.Product.Price);
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

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity; // Kullanıcıların bilgilerini getirdik
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                Order = new Models.Models.Order(),
                ShoppingCarts = _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value, includeProperty: "Product")
            };

            ShoppingCartVM.Order.ApplicationUser = _uow.ApplicationUser.GetFirstOrDefault(u => u.Id == claims.Value, "Company");

            foreach (var cart in ShoppingCartVM.ShoppingCarts)
            {
                cart.Price = ProjectConstant.GetPriceBaseOnQuantity(cart.Count, cart.Product.Price);

                ShoppingCartVM.Order.OrderTotal += (cart.Price * cart.Count);
            }

            ShoppingCartVM.Order.Name = ShoppingCartVM.Order.ApplicationUser.Name;
            ShoppingCartVM.Order.PhoneNumber = ShoppingCartVM.Order.ApplicationUser.PhoneNumber;
            ShoppingCartVM.Order.StreetAddress = ShoppingCartVM.Order.ApplicationUser.StreetAddress;
            ShoppingCartVM.Order.City = ShoppingCartVM.Order.ApplicationUser.City;
            ShoppingCartVM.Order.State = ShoppingCartVM.Order.ApplicationUser.State;
            ShoppingCartVM.Order.PostCode = ShoppingCartVM.Order.ApplicationUser.PostaCode;

            return View(ShoppingCartVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity; // Kullanıcıların bilgilerini getirdik
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.Order.ApplicationUser = _uow.ApplicationUser.GetFirstOrDefault(u => u.Id == claims.Value, "Company");

            ShoppingCartVM.ShoppingCarts = _uow.ShoppingCart.GetAll(s => s.ApplicationUserId == claims.Value, includeProperty: "Product");


            ShoppingCartVM.Order.PaymentStatus = ProjectConstant.PaymentStatusPending;

            ShoppingCartVM.Order.OrderStatus = ProjectConstant.StatusPending;

            ShoppingCartVM.Order.ApplicationUserId = claims.Value;

            ShoppingCartVM.Order.OrderDate = DateTime.Now;

            _uow.Order.Add(ShoppingCartVM.Order);
            _uow.Save();

            List<OrderDetails> orderDetails = new List<OrderDetails>();

            foreach (var shoppingCart in ShoppingCartVM.ShoppingCarts)
            {
                shoppingCart.Price = ProjectConstant.GetPriceBaseOnQuantity(shoppingCart.Count, shoppingCart.Product.Price);

                OrderDetails orderDetail = new OrderDetails()
                {
                    ProductId = shoppingCart.ProductId,
                    OrderId = ShoppingCartVM.Order.Id,
                    Count = shoppingCart.Count
                };

                ShoppingCartVM.Order.OrderTotal += orderDetail.Count * orderDetail.Price;
                _uow.OrderDetails.Add(orderDetail);
            }
            _uow.ShoppingCart.RemoveRange(ShoppingCartVM.ShoppingCarts);
            HttpContext.Session.SetInt32(ProjectConstant.Shopping_Cart, 0);

            if (stripeToken == null)
            {
                ShoppingCartVM.Order.PaymentDueDate = DateTime.Now.AddDays(30); // 30 gün eklendi
                ShoppingCartVM.Order.PaymentStatus = ProjectConstant.PaymentStatusDelayed;
                ShoppingCartVM.Order.OrderStatus = ProjectConstant.StatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.Order.OrderTotal * 100),
                    Currency = "$",
                    Description = "Order id : " + ShoppingCartVM.Order.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.BalanceTransactionId == null)
                    // limit kontrolü yapıyoruz
                    ShoppingCartVM.Order.PaymentStatus = ProjectConstant.PaymentStatusRejected;
                else
                    ShoppingCartVM.Order.TransactionId = charge.BalanceTransactionId;

                if (charge.Status.ToLower() == "succeded")
                {
                    ShoppingCartVM.Order.PaymentStatus = ProjectConstant.PaymentStatusApproved;
                    ShoppingCartVM.Order.OrderStatus = ProjectConstant.StatusApproved;
                    ShoppingCartVM.Order.PaymentDate = DateTime.Now;
                }
            }

            _uow.Save();
            return RedirectToAction("OorderConfirmation", "Cart", new { id = ShoppingCartVM.Order.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
