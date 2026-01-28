using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
    {
        employee.EmployeeCode = employee.EmployeeCode?.Trim();

        var existing = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeCode == employee.EmployeeCode);
        if (existing != null)
        {
            return Conflict($"Employee with code '{employee.EmployeeCode}' already exists.");
        }

        _context.Employees.Add(employee);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var innerMessage = ex.InnerException?.Message ?? string.Empty;
            if (innerMessage.Contains("IX_Employees_EmployeeCode", StringComparison.OrdinalIgnoreCase) ||
                innerMessage.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict($"Employee with code '{employee.EmployeeCode}' already exists.");
            }

            return Problem(
                detail: "Database error while saving employee.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return CreatedAtAction(nameof(GetByCode), new { code = employee.EmployeeCode }, employee);
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<Employee>> GetByCode(string code)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == code);
        if (employee == null) return NotFound();
        return employee;
    }
}


