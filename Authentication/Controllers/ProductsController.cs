using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Authentication.Helper;
using System.Linq.Dynamic.Core;
using Microsoft.Data.SqlClient;

namespace Authentication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly AuthenticationContext _context;

        public ProductsController(AuthenticationContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(int? pageIndex,
            int? categoryId, string? description, double? fromPrice, double? toPrice,
            string? orderBy, string? orderType,
            string? op)
        {
            var products = (IQueryable<Product>)_context.Products.Include(p => p.Category);
            //filtering
            products = Filter(products, categoryId, description, fromPrice, toPrice, op);
            //sorting
            products = Sort(products, orderBy, orderType, op);
            //phan trang
            const int pageSize = 5;
            return View(await PaginatedList<Product>.CreateAsync(products.AsNoTracking(), pageIndex ?? 1, pageSize));
        }

        private IQueryable<Product> Filter(IQueryable<Product> products, int? categoryId, string? description, double? fromPrice, double? toPrice, string? op)
        {
            FilterInfo filterInfo = new FilterInfo();
            switch (op)
            {
                case "search-do":
                    filterInfo = new FilterInfo
                    {
                        CategoryId = categoryId,
                        Description = description,
                        FromPrice = fromPrice,
                        ToPrice = toPrice,
                    };
                    break;
                case "search-clear":
                    break;
                default:
                    filterInfo = HttpContext.Session.Get<FilterInfo>("filterInfo") ?? filterInfo;
                    break;
            }
            HttpContext.Session.Set<FilterInfo>("filterInfo", filterInfo);
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", filterInfo.CategoryId);
            ViewBag.FilterInfo = filterInfo;
            if (filterInfo.CategoryId != null && filterInfo.CategoryId != -1)
            {
                products = products.Where(p => p.CategoryId == filterInfo.CategoryId);
            }
            if (!string.IsNullOrEmpty(filterInfo.Description))
            {
                products = products.Where(p => p.Description.Contains(filterInfo.Description));
            }
            if (filterInfo.FromPrice != null)
            {
                products = products.Where(p => p.Price >= filterInfo.FromPrice);
            }
            if (filterInfo.ToPrice != null)
            {
                products = products.Where(p => p.Price <= filterInfo.ToPrice);
            }
            return products;
        }

        private IQueryable<Product> Sort(IQueryable<Product> products, string? orderBy, string? orderType, string? op)
        {
            //Must run: Install-Package System.Linq.Dynamic.Core
            SortInfo sortInfo = new SortInfo { OrderBy = "Id", OrderType = "ASC" };
            switch (op)
            {
                case "sort-do":
                    sortInfo = new SortInfo { OrderBy = orderBy, OrderType = orderType };
                    break;
                case "sort-clear":
                    break;
                default:
                    sortInfo = HttpContext.Session.Get<SortInfo>("sortInfo") ?? sortInfo;
                    break;
            }
            HttpContext.Session.Set<SortInfo>("sortInfo", sortInfo);
            List<string> fieldList = new List<string> { "Id", "Description",
                "Discount", "Price", "Category" };
            ViewBag.FieldList = new SelectList(fieldList, sortInfo.OrderBy);
            ViewBag.SortInfo = sortInfo;
            return products.OrderBy($"{sortInfo.OrderBy} {sortInfo.OrderType}");
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Price,Discount,CategoryId,IsOutOfStock")] Product product)
        {
            // Parse and validate the values
            bool isValidPrice = double.TryParse(product.Price.ToString(), out double parsedPrice);
            bool isValidDiscount = double.TryParse(product.Discount.ToString(), out double parsedDiscount);
            bool isValidCategoryId = int.TryParse(product.CategoryId.ToString(), out int parsedCategoryId);
            bool isValidIsOutOfStock = int.TryParse(product.IsOutOfStock.ToString(), out int parsedIsOutOfStock);

            if (isValidPrice && isValidDiscount && isValidCategoryId && isValidIsOutOfStock)
            {
                var sql = "INSERT [dbo].[Product] ([description], [price], [discount], [CategoryId], [IsOutOfStock]) VALUES (@Description, @Price, @Discount, @CategoryId, @IsOutOfStock)";
                await _context.Database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("@Description", product.Description),
                    new SqlParameter("@Price", parsedPrice),
                    new SqlParameter("@Discount", parsedDiscount),
                    new SqlParameter("@CategoryId", parsedCategoryId),
                    new SqlParameter("@IsOutOfStock", parsedIsOutOfStock));
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Invalid input data for Price, Discount, CategoryId, or IsOutOfStock");
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Price,Discount,CategoryId,IsOutOfStock")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            // Parse and validate the values
            bool isValidPrice = double.TryParse(product.Price.ToString(), out double parsedPrice);
            bool isValidDiscount = double.TryParse(product.Discount.ToString(), out double parsedDiscount);
            bool isValidCategoryId = int.TryParse(product.CategoryId.ToString(), out int parsedCategoryId);
            bool isValidIsOutOfStock = int.TryParse(product.IsOutOfStock.ToString(), out int parsedIsOutOfStock);

            if (isValidPrice && isValidDiscount && isValidCategoryId && isValidIsOutOfStock)
            {
                try
                {
                    var sql = "UPDATE [dbo].[Product] SET Description = @Description, Price = @Price, Discount = @Discount, CategoryId = @CategoryId, IsOutOfStock = @IsOutOfStock WHERE Id = @Id";
                    await _context.Database.ExecuteSqlRawAsync(sql,
                        new SqlParameter("@Id", product.Id),
                        new SqlParameter("@Description", product.Description),
                        new SqlParameter("@Price", parsedPrice),
                        new SqlParameter("@Discount", parsedDiscount),
                        new SqlParameter("@CategoryId", parsedCategoryId),
                        new SqlParameter("@IsOutOfStock", parsedIsOutOfStock));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool isValidIsOutOfStock = int.TryParse(id.ToString(), out int parsedIsOutOfStock);
            var sql = "UPDATE [dbo].[Product] SET IsOutOfStock = 1 WHERE Id = @Id";
            await _context.Database.ExecuteSqlRawAsync(sql, new SqlParameter("@Id", id));
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        public IActionResult MonthlyRevenue()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MonthlyRevenue(int month, int year)
        {
            var orders = await _context.Orders
                .Where(o => o.Date.Month == month && o.Date.Year == year)
                .Select(o => new MonthlyRevenueViewModel
                {
                    OrderId = o.Id,
                    OrderDate = o.Date,
                    TotalPrice = o.OrderDetails.Sum(od => od.Price * od.Quantity),
                    TotalDiscount = o.OrderDetails.Sum(od => od.Discount),
                    TotalRevenue = o.OrderDetails.Sum(od => (od.Price * od.Quantity) - od.Discount)
                })
                .ToListAsync();

            ViewBag.Month = month;
            ViewBag.Year = year;
            ViewBag.TotalRevenue = orders.Sum(o => o.TotalRevenue);

            return View(orders);
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
