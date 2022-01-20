using Dapper;
using Microsoft.AspNetCore.Mvc;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using MusicStore.Utility;

namespace MusicStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _uow; // yazılı olmayan yazım kuralı

        public CoverTypeController(IUnitOfWork uow)
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
            // var coverTypes = _uow.CoverType.GetAll(); // unitOfWork sayesinde categoriye ve onun metotlarına eriştik
            var coverTypes = _uow.Sp_Call.List<CoverType>(ProjectConstant.Proc_CoverType_GetAll, null); // store procedure ile listeleme
            return Json(new { data = coverTypes });
        }

        [HttpDelete] // silme işlemi
        public IActionResult Delete(int id)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Id", id);

            var deleteData = _uow.Sp_Call.OneRecord<CoverType>(ProjectConstant.Proc_CoverType_Get, parameter);

            if (deleteData != null)
            {
                return Json(new { success = false, message = "Data Not Found!" });
            }

            _uow.Sp_Call.Execute(ProjectConstant.Proc_CoverType_Delete, parameter);
            _uow.Save();
            return Json(new { success = true, message = "Delete Operation Successfully" });
        }
        #endregion

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            if (id > 0)
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Id", id);

                var coverType = _uow.Sp_Call.OneRecord<CoverType>(ProjectConstant.Proc_CoverType_Get, parameter);

                if (coverType != null)
                {
                    return View(coverType); // Güncelleme için
                }

                return NotFound();
            }

            return View(new CoverType()); // Ekleme için
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Name", coverType.Name);
                if (coverType.Id == 0)
                {
                    // Create
                    _uow.Sp_Call.Execute(ProjectConstant.Proc_CoverType_Create, parameter);
                }
                else
                {
                    // Update
                    parameter.Add("@Id", coverType.Id);
                    _uow.Sp_Call.Execute(ProjectConstant.Proc_CoverType_Update, parameter);
                }
                _uow.Save();
                return RedirectToAction("Index");
            }

            return View(coverType);
        }
    }
}
