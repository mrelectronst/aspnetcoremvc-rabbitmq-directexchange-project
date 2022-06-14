using AspNetCore.RabbitMQ.ExampleProject.Models;
using AspNetCore.RabbitMQ.ExampleProject.Models.DataContext;
using AspNetCore.RabbitMQ.ExampleProject.Services.RabbitMQ;
using AspNetCore.RabbitMQ.ExampleProject.Services.RabbitMQ.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.RabbitMQ.ExampleProject.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;

        public ProductsController(AppDbContext context, IRabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return _context.Product != null ?
                        View(await _context.Product.ToListAsync()) :
                        Problem("Entity set 'AppDbContext.Product'  is null.");
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product
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
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Stock,ImageName")] Product product, IFormFile imageFile)
        {
            //if (!ModelState.IsValid) return View(product);

            if (imageFile is { Length: > 0 })
            {
                var randomImageName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", randomImageName);

                await using FileStream stream = new(path: path, FileMode.Create);

                await imageFile.CopyToAsync(stream);

                _rabbitMQPublisher.Publish(new ProductImageCreatedEvent()
                {
                    ImageName = randomImageName,
                });

                product.ImageName = randomImageName;
            }


            _context.Add(product);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Stock,ImageName")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
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
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product
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
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Product == null)
            {
                return Problem("Entity set 'AppDbContext.Product'  is null.");
            }
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
