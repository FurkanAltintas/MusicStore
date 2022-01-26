using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using MusicStore.Models.ViewModels;
using MusicStore.Utility;
using Stripe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MusicStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _uow;

        [BindProperty]
        public OrderDetailsVM orderDetailsVM { get; set; }

        public OrderController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            orderDetailsVM = new OrderDetailsVM
            {
                Order = _uow.Order.GetFirstOrDefault(o => o.Id == id, "User"),
                OrderDetails = _uow.OrderDetails.GetAll(o => o.OrderId == id, includeProperty: "Product")
            };

            return View(orderDetailsVM);
        }

        [Authorize(Roles =  ProjectConstant.Role_Admin + "," + ProjectConstant.Role_User_Employee)]
        public IActionResult StartProcessing(int id)
        {
            Order order = _uow.Order.GetFirstOrDefault(o => o.Id == id);
            order.OrderStatus = ProjectConstant.StatusInProcess;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles=ProjectConstant.Role_Admin + "," + ProjectConstant.Role_User_Employee)]
        public IActionResult ShipOrder()
        {
            Order order = _uow.Order.GetFirstOrDefault(o => o.Id == orderDetailsVM.Order.Id);
            order.TrackingNumber = orderDetailsVM.Order.TrackingNumber;
            order.Carrier = orderDetailsVM.Order.Carrier;
            order.OrderStatus = ProjectConstant.StatusShipped;
            order.ShippingDate = DateTime.Now;

            _uow.Save();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = ProjectConstant.Role_Admin + "," + ProjectConstant.Role_User_Employee)]
        public IActionResult CancelOrder(int id)
        {
            Models.Models.Order order = _uow.Order.GetFirstOrDefault(o => o.Id == id);

            if (order.PaymentStatus == ProjectConstant.StatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt16(order.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = order.TransactionId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                order.OrderStatus = ProjectConstant.StatusRefund;
                order.PaymentStatus = ProjectConstant.StatusRefund;
            }
            else
            {
                order.OrderStatus = ProjectConstant.StatusCancelled;
                order.PaymentStatus = ProjectConstant.StatusCancelled;
            }

            _uow.Save();

            return RedirectToAction(nameof(Index));
        }

        #region APICALLS

        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<Order> orders;

            if (User.IsInRole(ProjectConstant.Role_Admin) || User.IsInRole(ProjectConstant.Role_User_Employee)) // Rolüm admin veya employee ise
                orders = _uow.Order.GetAll(includeProperty: "User");
            else
                orders = _uow.Order.GetAll(u => u.ApplicationUserId == claim.Value, includeProperty: "User");

            switch (status)
            {
                case "pending":
                    orders = orders.Where(p => p.PaymentStatus == ProjectConstant.PaymentStatusDelayed);
                    break;

                case "inprocess":
                    orders = orders.Where(o => o.OrderStatus == ProjectConstant.StatusApproved || o.OrderStatus == ProjectConstant.StatusInProcess || o.OrderStatus == ProjectConstant.StatusPending);
                    break;

                case "completed":
                    orders = orders.Where(o => o.OrderStatus == ProjectConstant.StatusShipped);
                    break;

                case "rejected":
                    orders = orders.Where(o => o.OrderStatus == ProjectConstant.StatusCancelled || o.OrderStatus == ProjectConstant.StatusRefund || o.OrderStatus == ProjectConstant.PaymentStatusRejected);
                    break;


                default:
                    break;
            }


            orders = _uow.Order.GetAll(includeProperty: "User");
            return Json(new { data = orders, status = status });
        }

        #endregion
    }
}
