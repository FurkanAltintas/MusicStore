using Microsoft.AspNetCore.Mvc;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using System;

namespace MusicStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ApplicationUserController : Controller
    {
        private readonly IUnitOfWork _uow; // yazılı olmayan yazım kuralı

        public ApplicationUserController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS (ctrl + k + s ile hızlı seçim sayesinde ekledik)
        public IActionResult GetAll()
        {
            var categories = _uow.ApplicationUser.GetAll(); // unitOfWork sayesinde categoriye ve onun metotlarına eriştik
            return Json(new { data = categories });
        }

        [HttpDelete] // silme işlemi
        public IActionResult Delete(int id)
        {
            var data = _uow.ApplicationUser.Get(id);

            if (data == null)
                return Json(new { success = false, message = "Data Not Found!" });

            _uow.ApplicationUser.Remove(data);
            _uow.Save();
            return Json(new { success = true, message = "Delete Operation Successfully" });
        }
        #endregion

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            if (id > 0)
            {
                var applicationUser = _uow.ApplicationUser.Get((int)id);

                if (applicationUser != null)
                {
                    return View(applicationUser); // Güncelleme için
                }

                return NotFound();
            }

            return View(new ApplicationUser()); // Ekleme için
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                if (Convert.ToInt32(applicationUser.Id) == 0)
                {
                    // Create
                    _uow.ApplicationUser.Add(applicationUser);
                }
                else
                {
                    // Update
                    _uow.ApplicationUser.Update(applicationUser);
                }
                _uow.Save();
                return RedirectToAction("Index");
            }

            return View(applicationUser);
        }
    }
}
