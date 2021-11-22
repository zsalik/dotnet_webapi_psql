using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleQuotes;
using VehicleQuotes.Models;
using VehicleQuotes.ResourceModels;

namespace dotnet_webapi_psql.Controllers
{
    [Route("api/Makes/{makeId}/[controller]")]
    [ApiController]
    public class ModelsController : ControllerBase
    {
        private readonly VehicleQuotesContext _context;

        public ModelsController(VehicleQuotesContext context)
        {
            _context = context;
        }

        // GET: api/Models
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModelSpecification>>> GetModels([FromRoute] int makeId)
        {
            var make = await _context.Makes.FindAsync(makeId);

            if (make == null)
            {
                return NotFound();
            }

            var modelsToReturn = _context.Models
                .Where(m => m.MakeID == makeId)
                .Select(m => new ModelSpecification {
                    ID = m.ID,
                    Name = m.Name,
                    Styles = m.ModelStyles.Select(ms => new ModelSpecificationStyle {
                        BodyType = ms.BodyType.Name,
                        Size = ms.Size.Name,
                        Years = ms.ModelStyleYears.Select(msy => msy.Year).ToArray()
                    }).ToArray()
                });

            return await modelsToReturn.ToListAsync();
        }

        // GET: api/Models/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ModelSpecification>> GetModel([FromRoute] int makeId, int id)
        {
            var model = await _context.Models
                .Include(m => m.ModelStyles).ThenInclude(ms => ms.BodyType)
                .Include(m => m.ModelStyles).ThenInclude(ms => ms.Size)
                .Include(m => m.ModelStyles).ThenInclude(ms => ms.ModelStyleYears)
                .FirstOrDefaultAsync(m => m.MakeID == makeId && m.ID == id);

            if (model == null)
            {
                return NotFound();
            }

            return new ModelSpecification {
                ID = model.ID,
                Name = model.Name,
                Styles = model.ModelStyles.Select(ms => new  ModelSpecificationStyle {
                    BodyType = ms.BodyType.Name,
                    Size = ms.Size.Name,
                    Years = ms.ModelStyleYears.Select(msy => msy.Year).ToArray()
                }).ToArray()
            };
        }

        // PUT: api/Models/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutModel([FromRoute] int makeId, int id, ModelSpecification model)
        {
            if (id != model.ID)
            {
                return BadRequest();
            }

            var modelToUpdate = await _context.Models 
                .Include(m => m.ModelStyles)
                .FirstOrDefaultAsync(m => m.MakeID == makeId && m.ID == id);

            if (modelToUpdate == null)
            {
                return NotFound();
            }

            modelToUpdate.Name = model.Name;

            modelToUpdate.ModelStyles = model.Styles.Select(style => new ModelStyle {
                BodyType = _context.BodyTypes.Single(bodyType => bodyType.Name == style.BodyType),
                Size = _context.Sizes.Single(size => size.Name == style.Size),

                ModelStyleYears = style.Years.Select(year => new ModelStyleYear {
                    Year = year
                }).ToList()
            }).ToList();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                return Conflict();
            }

            return NoContent();
        }

        // POST: api/Models
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ModelSpecification>> PostModel([FromRoute] int makeId, ModelSpecification model)
        {
            // First, try to find the make specified by the incoming `makeId`.
            var make = await _context.Makes.FindAsync(makeId);

            // Respond with 404 if not found.
            if (make == null)
            {
                return NotFound();
            }

            // Build out a new `Model` entity, complete with all related data, based on
            // the `ModelSpecification` parameter.
            var modelToCreate = new Model {
                Make = make,
                Name = model.Name,

                ModelStyles = model.Styles.Select(style => new ModelStyle {
                    // Notice how we search both body type and size by their name field.
                    // We can do that because their names are unique.
                    BodyType = _context.BodyTypes.Single(bodyType => bodyType.Name == style.BodyType),
                    Size = _context.Sizes.Single(size => size.Name == style.Size),

                    ModelStyleYears = style.Years.Select(year => new ModelStyleYear {
                        Year = year
                    }).ToArray()
                }).ToArray()
            };

            // Add it to the DbContext.
            _context.Add(modelToCreate);

            try
            {
                // Try running the INSERTs.
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                // Return accordingly if an error happens.
                return Conflict();
            }

            // Get back the autogenerated ID of the record we just INSERTed.
            model.ID = modelToCreate.ID;

            // Finally, return a 201 including a location header containing the newly
            // created resource's URL and the resource itself in the response payload.
            return CreatedAtAction(
                nameof(GetModel),
                new { makeId = makeId, id = model.ID },
                model
            );
        }

        // DELETE: api/Models/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel([FromRoute] int makeId, int id)
        {
            // Try to find the record identified by the ids from the URL.
            var model = await _context.Models.FirstOrDefaultAsync(m => m.MakeID == makeId && m.ID == id);

            // Respond with a 404 if we can't find it.
            if (model == null)
            {
                return NotFound();
            }

            // Mark the entity for removal and run the DELETE.
            _context.Models.Remove(model);
            await _context.SaveChangesAsync();

            // Respond with a 204.
            return NoContent();
        }

        private bool ModelExists(int id)
        {
            return _context.Models.Any(e => e.ID == id);
        }
    }
}
