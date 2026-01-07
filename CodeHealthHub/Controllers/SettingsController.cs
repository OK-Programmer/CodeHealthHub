using CodeHealthHub.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CodeHealthHub.Models;
using CodeHealthHub.Models.ViewModels;

namespace CodeHealthHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController(AppDbContext dbContext) : ControllerBase
    {
        private readonly AppDbContext _dbContext = dbContext;

        [HttpGet("colours")]
        public IActionResult GetColours()
        {
            // TODO: Fetch all pie chart colours from the database
            List<PieChartColour> colours = _dbContext.PieChartColours.ToList();

            SettingsViewModel model = new SettingsViewModel();
            foreach (var colour in colours)
            {
                switch (colour.CategoryName)
                {
                    case "Default":
                        model.DefaultColour = colour.HexCode ?? model.DefaultColour;
                        break;
                    case "Info":
                        model.InfoColour = colour.HexCode ?? model.InfoColour;
                        break;
                    case "Minor":
                        model.MinorColour = colour.HexCode ?? model.MinorColour;
                        break;
                    case "Major":
                        model.MajorColour = colour.HexCode ?? model.MajorColour;
                        break;
                    case "Critical":
                        model.CriticalColour = colour.HexCode ?? model.CriticalColour;
                        break;
                    case "Blocker":
                        model.BlockerColour = colour.HexCode ?? model.BlockerColour;
                        break;
                }
            }

            // Return the list of color hex codes
            return Ok(model);
        }

        [HttpPost("colours")]
        public IActionResult ChangeColours([FromBody] SettingsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // Update the pie chart colours in the database
                var colours = new List<PieChartColour>
                {
                    new PieChartColour { CategoryName = "Default", HexCode = model.DefaultColour },
                    new PieChartColour { CategoryName = "Info", HexCode = model.InfoColour },
                    new PieChartColour { CategoryName = "Minor", HexCode = model.MinorColour },
                    new PieChartColour { CategoryName = "Major", HexCode = model.MajorColour },
                    new PieChartColour { CategoryName = "Critical", HexCode = model.CriticalColour },
                    new PieChartColour { CategoryName = "Blocker", HexCode = model.BlockerColour }
                };

                // Clear existing colours and add the new ones
                _dbContext.PieChartColours.RemoveRange(_dbContext.PieChartColours);
                _dbContext.PieChartColours.AddRange(colours);
                _dbContext.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
