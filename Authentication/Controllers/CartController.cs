using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Authentication.Helper;
using Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Authentication.Areas.Identity.Data;

namespace PetStore.Controllers
{
    public class CartController : Controller
    {
        private readonly AuthenticationContext _context;
        private readonly ILogger<CartController> _logger;
        private readonly UserManager<MyUser> _userManager;

        public CartController(ILogger<CartController> logger, AuthenticationContext context, UserManager<MyUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            //Lay cart tu session
            Cart cart = HttpContext.Session.Get<Cart>("cart")??new Cart();
            return View(cart);
        }
        [Authorize]
        public async Task<IActionResult> CheckOut()
        {
            //Lay cart tu session
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            return View(cart);
        }

        public async Task<IActionResult> Payment()
        {
            //Lay cart tu session
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            //Lay current user
            MyUser? user = await _userManager.GetUserAsync(User);
            //Thuc hien thanh toan: Zalo, Momo...
            //Luu Order vao db: Order & OrderDetail
            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    Order order = new Order
                    {
                        Date = DateTime.Today,
                        CustomerId = user.Id,
                        EmployeeId = null,
                    };
                    //luu order header vao table Order
                    _context.Orders.Add(order);
                    _context.SaveChanges();
                    //luu order details vao table OrderDetail
                    foreach (Item item in cart.List.Values)
                    {
                        OrderDetail orderDetail = new OrderDetail
                        {
                            OrderId = order.Id,
                            ProductId = item.Id,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            Discount = item.Discount,
                        };
                        _context.OrderDetails.Add(orderDetail);
                    }
                    await _context.SaveChangesAsync();
                    //ket thuc transaction khi thuc hien thanh cong
                    tran.Commit();
                    //xoa cart trong session
                    HttpContext.Session.Remove("cart");
                }
                catch (Exception ex)
                {
                    //undo transaction
                    tran.Rollback();
                }
            }
            return View();
        }

        public async Task<IActionResult> Add(int id)
        {
            Product? p = _context.Products.Include(p => p.Category).First(p => p.Id == id);
               
            Item item = new Item
            {
                Id = p.Id,
                Category = p.Category.Name,
                Description = p.Description,
                Discount = p.Discount,
                Price = p.Price,
                Quantity = 1
            };
            //Lay cart tu session
            Cart cart = HttpContext.Session.Get<Cart>("cart")??new Cart();
            //Luu item vao cart
            cart.Add(item);
            //Luu cart vao session
            HttpContext.Session.Set<Cart>("cart", cart);
            //Quay ve /Home/Index de hien lai home page
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Remove(int id)
        {
            //Lay cart tu session
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            //Remove item tu cart
            cart.Remove(id);
            //Luu cart vao session
            HttpContext.Session.Set<Cart>("cart", cart);
            //Quay ve thuc hien action /Cart/Index
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Empty()
        {
            //Lay cart tu session
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            //Empty cart
            cart.Empty();
            //Luu cart vao session
            HttpContext.Session.Set<Cart>("cart", cart);
            //Quay ve thuc hien action /Cart/Index
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(int id, int quantity)
        {
            //Lay cart tu session
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            //Empty cart
            cart.Update(id, quantity);
            //Luu cart vao session
            HttpContext.Session.Set<Cart>("cart", cart);
            //Quay ve thuc hien action /Cart/Index
            return RedirectToAction("Index");
        }

    }
}
