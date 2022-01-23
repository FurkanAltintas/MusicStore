using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using MusicStore.Utility;

namespace MusicStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ProjectConstant.Role_Admin+","+ProjectConstant.Role_User_Employee)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _uow; // yazılı olmayan yazım kuralı

        public CompanyController(IUnitOfWork uow)
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
            var companies = _uow.Company.GetAll(); // unitOfWork sayesinde categoriye ve onun metotlarına eriştik
            return Json(new { data = companies });
        }

        [HttpDelete] // silme işlemi
        public IActionResult Delete(int id)
        {
            var data = _uow.Company.Get(id);

            if (data == null)
                return Json(new { success = false, message = "Data Not Found!" });

            _uow.Company.Remove(data);
            _uow.Save();
            return Json(new { success = true, message = "Delete Operation Successfully" });
        }
        #endregion

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            if (id > 0)
            {
                var company = _uow.Company.Get((int)id);

                if (company != null)
                {
                    return View(company); // Güncelleme için
                }

                return NotFound();
            }

            return View(new Company()); // Ekleme için
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    // Create
                    _uow.Company.Add(company);
                }
                else
                {
                    // Update
                    _uow.Company.Update(company);
                }
                _uow.Save();
                return RedirectToAction("Index");
            }

            return View(company);
        }
    }
}
