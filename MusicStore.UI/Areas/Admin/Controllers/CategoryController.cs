using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using MusicStore.Utility;

namespace MusicStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ProjectConstant.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _uow; // yazılı olmayan yazım kuralı

        public CategoryController(IUnitOfWork uow)
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
            var categories = _uow.Category.GetAll(); // unitOfWork sayesinde categoriye ve onun metotlarına eriştik
            return Json(new { data = categories });
        }

        [HttpDelete] // silme işlemi
        public IActionResult Delete(int id)
        {
            var data = _uow.Category.Get(id);

            if (data == null)
                return Json(new { success = false, message = "Data Not Found!" });

            _uow.Category.Remove(data);
            _uow.Save();
            return Json(new { success = true, message = "Delete Operation Successfully" });
        }
        #endregion

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            if (id > 0)
            {
                var category = _uow.Category.Get((int)id);

                if (category != null)
                {
                    return View(category); // Güncelleme için
                }

                return NotFound();
            }

            return View(new Category()); // Ekleme için
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    // Create
                    _uow.Category.Add(category);
                }
                else
                {
                    // Update
                    _uow.Category.Update(category);
                }
                _uow.Save();
                return RedirectToAction("Index");
            }

            return View(category);
        }
    }
}
