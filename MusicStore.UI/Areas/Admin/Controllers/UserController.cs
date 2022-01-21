using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStore.DataAccess.Data;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using System;
using System.Linq;

namespace MusicStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS (ctrl + k + s ile hızlı seçim sayesinde ekledik)
        public IActionResult GetAll()
        {
            /* Kullanıcıları getireceğiz. */
            var users = _context.ApplicationUsers.Include(c => c.Company).ToList(); // kullanıcıları getirdik onunla birlikte company bilgilerini de getirdik
            var userRoles = _context.UserRoles.ToList(); // UserRole tablosundan rolleri de getirdik (A)
            var roles = _context.Roles.ToList(); // rolleri getirdik
            //  AspNetUsers - AspNetUserRoles - AspNetRoles

            foreach (var user in users) // kullanıcılar var mı ?
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId; // rol id aldık
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name; // rol adını aldık

                if (user.Company == null) // null ise
                {
                    user.Company = new Company()
                    {
                        Name = string.Empty
                    };
                }
            }

            /* 
                      SQL SORGUSU 

                Select ur.RoleId, r.Name from AspNetUsers as u
                JOIN AspNetUserRoles ur
                ON ur.UserId = u.Id
                JOIN AspNetRoles r
                ON r.Id = ur.RoleId

                Çıktı :
                RoleId: c9005561-0d5f-4680-b8e2-df5df7b93f7e
                Name: Admin

             */

            return Json(new { data = users });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id) // gönderilecek bilgiyi formun gövdesinden alır
        {
            /* 
              [FromQuery] -Sorgu dizesinden değerleri alır.
              [FromRoute] -Rota verilerinden değerleri alır.
              [FromForm] -Postalanan Form alanlarındaki değerleri alır.
              [FromBody] -İstek gövdesinden değerleri alır.
              [FromHeader] -HTTP başlıklarındaki değerleri alır.
             */

            var data = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (data == null)
                return Json(new { success = false, message = "Error while locking/unlocking" });

            if (data.LockoutEnd != null && data.LockoutEnd > DateTime.Now) // bitiş tarihi var ise ve bugünden büyük ise
                data.LockoutEnd = DateTime.Now; // bugünün tarihine ata
            else
                data.LockoutEnd = DateTime.Now.AddYears(10); // 10 yıl sonrasına verdik

            _context.SaveChanges();
            return Json(new { success = true, message = "Operation Successfully" });

        }
        #endregion
    }
}
