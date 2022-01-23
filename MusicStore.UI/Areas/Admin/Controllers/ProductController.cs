using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using MusicStore.Models.ViewModels;
using MusicStore.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MusicStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ProjectConstant.Role_Admin)] // rol admin bu sayfayla ilgili çalışmalar yapabilecek
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _uow; // yazılı olmayan yazım kuralı
        private readonly IWebHostEnvironment _hostEnvironment; // wwwroot erişimi sağlanır

        public ProductController(IUnitOfWork uow, IWebHostEnvironment hostEnvironment)
        {
            _uow = uow;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS (ctrl + k + s ile hızlı seçim sayesinde ekledik)
        public IActionResult GetAll()
        {
            var products = _uow.Product.GetAll(includeProperty: "Category,CoverType"); // unitOfWork sayesinde categoriye ve onun metotlarına eriştik
            return Json(new { data = products });
        }

        [HttpDelete] // silme işlemi
        public IActionResult Delete(int id)
        {
            var data = _uow.Product.Get(id); //database üzerinden bilgiyi getirdik

            if (data == null) // bilgi mevcut mu diye kontrol ettik
                return Json(new { success = false, message = "Data Not Found!" });

            string webRootPath = _hostEnvironment.WebRootPath; // statik dosyaların yolunu aldık
            var imagePath = Path.Combine(webRootPath, data.ImageUrl.TrimStart('\\')); // birleştirme yaptık

            if (System.IO.File.Exists(imagePath)) // o yolda bu bilgi var mı
            {
                System.IO.File.Delete(imagePath); // varsa görseli siliyoruz
            }
            // kayıt bulma işlemi
            _uow.Product.Remove(data);
            _uow.Save();
            return Json(new { success = true, message = "Delete Operation Successfully" });
        }
        #endregion

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                Categories = CategoryDropDownList(),
                CoverTypes = CoverTypeDropDownList()
            };

            if (id == null)
                return View(productVM);

            productVM.Product = _uow.Product.Get(id.GetValueOrDefault());

            if (productVM.Product == null)
                return NotFound();

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"img\products");
                    // ana root dizinine img\products ekle
                    var extension = Path.GetExtension(files[0].FileName);

                    if (productVM.Product.ImageUrl != null)
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(imagePath)) // bu dosya var mı yok mu ?
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create)) // belirtilen yola dosyaları atma işlemi
                    {
                        files[0].CopyTo(fileStreams);
                    }

                    productVM.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var productData = _uow.Product.Get(productVM.Product.Id);
                        productVM.Product.ImageUrl = productData.ImageUrl;
                    }
                }

                if (productVM.Product.Id == 0)
                {
                    _uow.Product.Add(productVM.Product);
                }
                else
                {
                    _uow.Product.Update(productVM.Product);
                }

                _uow.Save();
                return RedirectToAction("Index");
            }
            else
            {
                productVM.Categories = CategoryDropDownList();
                productVM.CoverTypes = CoverTypeDropDownList();

                if (productVM.Product.Id != 0)
                {
                    productVM.Product = _uow.Product.Get(productVM.Product.Id);
                }
            }
            return View(productVM);
        }

        public IEnumerable<SelectListItem> CategoryDropDownList()
        {
            return _uow.Category.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
        }

        public IEnumerable<SelectListItem> CoverTypeDropDownList()
        {
            return _uow.CoverType.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
        }
    }
}
