using Authentication.Helper;
using Authentication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Dynamic.Core;

namespace Authentication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AuthenticationContext _context;

        public HomeController(ILogger<HomeController> logger, AuthenticationContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int? pageIndex,
            int? categoryId, string? description, double? fromPrice, double? toPrice,
            string? orderBy, string? orderType,
            string? op)
        {
            var products = (IQueryable<Product>)_context.Products.Where(p => p.IsOutOfStock == 0).Include(p => p.Category).AsQueryable();
            //filtering
            products = Filter(products, categoryId, description, fromPrice, toPrice, op);
            //sorting
            products = Sort(products, orderBy, orderType, op);
            //phan trang
            const int pageSize = 6;
            return View(await PaginatedList<Product>.CreateAsync(products, pageIndex ?? 1, pageSize));
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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

    }
}
