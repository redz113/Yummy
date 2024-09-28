using Microsoft.AspNetCore.Mvc;

namespace AppFoods.Services
{
    public interface IMyController
    {
        Task<IActionResult> IndexAsync();

        Task<IActionResult> CreateAsync();

        // [HttpPost]
        // Task<IActionResult> CreateAsync(dynamic model);

        Task<IActionResult> EditAsync(int id);

        // [HttpPost]
        // Task<IActionResult> EditAsync(int id, dynamic model);

        Task<IActionResult> DeleteAsync(int id);

        Task<IActionResult> DetailAsync(int id);
    }
}
