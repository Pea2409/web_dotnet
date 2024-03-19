﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020570.BusinessLayers;
using SV20T1020570.DomainModels;
using SV20T1020570.Web.Models;
using System;

namespace SV20T1020570.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Adminstrator},{WebUserRoles.Employee}")]

    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 20;
        private const string PRODUCT_SEARCH = "product_search"; // tên biến để lưu trong session

        public IActionResult Index()
        {
            // Lấy đầu vào tìm kiếm hiện đang lưu lại trong session
            ProductSearchInput? input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);

            // Trưởng hợp trong session chưa có điều kiện thì tạo điều kiện mới
            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(input);
        }
        public IActionResult Search(ProductSearchInput input)
        {
            int rowCount = 0;
            var data = ProductDataService.ListProducts(out rowCount, input.Page, input.PageSize, input.SearchValue ?? ""
                ,input.CategoryID,input.SupplierID);
            var model = new ProductSearchResult()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };

            //Lưu lại điều kiện tìm kiếm vào trong session
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);

            return View(model);
        }
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            ViewBag.IsEdit = false;
            Product model = new Product()
            {
                ProductID = 0,
                Photo = "no-image.png"
            };
            return View("Edit", model);
        }
        public IActionResult Edit(int id = 0)
        {
            ViewBag.IsEdit = true;
            ViewBag.Title = "Cập nhật thông tin mặt hàng";
            Product? model = ProductDataService.GetProduct(id);
            return View(model);
        }
        public IActionResult Delete(int id)
        {
            if (Request.Method == "POST")
            {
                ProductDataService.DeleteProduct(id);
                return RedirectToAction("Index");
            }
            var model = ProductDataService.GetProduct(id);
            if (model == null)
                return RedirectToAction("Index");
            ViewBag.AllowDelete = !ProductDataService.IsUsedProduct(id);

            return View(model);
        }
        [HttpPost]
        public IActionResult Save(Product data, IFormFile? uploadPhoto)
        {
            try
            {
                if (uploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}"; // tên file sẽ lưu
                    string folder = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, "images\\products"); // đường dẫn đến thư mục lưu file
                    string filePath = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadPhoto.CopyTo(stream);
                    }
                    data.Photo = fileName;
                }
                // Kiểm soát đầu vào và đưa các thông báo lỗi vào trong ModelState (nếu có)
                if (string.IsNullOrWhiteSpace(data.ProductName))
                    ModelState.AddModelError(nameof(data.ProductName), "Tên mặt hàng không được để trống");
                if (string.IsNullOrWhiteSpace(data.Unit))
                    ModelState.AddModelError(nameof(data.Unit), "Vui lòng nhập đơn vị tính");
                
                //Thông qua thuộc tính IsValid của ModelState để kiểm tra xem có tồn tại lỗi hay không
                if (!ModelState.IsValid)
                {
                    ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";
                    ViewBag.IsEdit = data.ProductID == 0 ? false : true;
                    return View("Edit", data);
                }

                if (data.ProductID == 0)
                {
                    int id = ProductDataService.AddProduct(data);
                    
                }
                else
                {
                    bool result = ProductDataService.UpdateProduct(data);
                    
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "Không thể lưu dữ liệu. Vui lòng thử lại!");
                return View("Edit", data);
            }
        }
        public IActionResult Photo(int id, string method, long photoId = 0)
        {
            switch(method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh";
                    ProductPhoto model = new ProductPhoto()
                    {
                        ProductID = id,
                        PhotoID = 0
                    };
                    return View(model);
                case "edit":
                    ViewBag.Title = "Thay đổi ảnh";
                    ProductPhoto? data = ProductDataService.GetPhoto(photoId);
                    if (data == null)
                        return RedirectToAction("Edit");
                    return View(data);
                case "delete":
                    ProductDataService.DeletePhoto(photoId);
                    //TODO: Xoá ảnh (xoá trực tiếp, không cần xác nhận)
                    return RedirectToAction("Edit", new {id = id});
                default:
                    return RedirectToAction("Index");
            }
        }
        public IActionResult Attribute(int id, string method, int attributeId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính";
                    ProductAttribute model = new ProductAttribute()
                    {
                        ProductID = id,
                        AttributeID = 0
                    };
                    return View(model);
                case "edit":
                    ViewBag.Title = "Thay đổi thuộc tính";
                    ProductAttribute? data = ProductDataService.GetAttribute(attributeId);
                    if (data == null)
                        return RedirectToAction("Edit");
                    return View(data);
                case "delete":
                    ProductDataService.DeleteAttribute(attributeId);
                    //TODO: Xoá thuộc tính (xoá trực tiếp, không cần xác nhận)
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public IActionResult SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            try
            {
                
                if (uploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}"; // tên file sẽ lưu
                    string folder = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, "images\\products"); // đường dẫn đến thư mục lưu file
                    string filePath = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadPhoto.CopyTo(stream);
                    }
                    data.Photo = fileName;
                }
                if (data.PhotoID == 0)
                {
                    long id = ProductDataService.AddPhoto(data);
                }
                else
                {
                    bool result = ProductDataService.UpdatePhoto(data);
                }
                return RedirectToAction("Index");
               

            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        [HttpPost]
        public IActionResult SaveAttribute(ProductAttribute data)
        {
            try
            {
                
                if (data.AttributeID == 0)
                {
                    long id = ProductDataService.AddAttribute(data);
                }
                else
                {
                    bool result = ProductDataService.UpdateAttribute(data);
                }
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}