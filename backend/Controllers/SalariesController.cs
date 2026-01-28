using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalariesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SalariesController(AppDbContext context)
    {
        _context = context;
    }

    public class CreateSalaryRequest
    {
        public string SalaryMonth { get; set; } = string.Empty;
        public int SalaryYear { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public int Present { get; set; }
        public int LossOfPay { get; set; }
    }

    [HttpPost]
    public async Task<ActionResult<Salary>> CreateSalary(CreateSalaryRequest request)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == request.EmployeeCode);
        if (employee == null)
        {
            return BadRequest($"Employee with code {request.EmployeeCode} not found.");
        }

        var basicSalary = employee.BasicSalary;
        var basicPay = (basicSalary / 30m) * request.Present;
        var hra = (basicPay * 20m) / 100m;
        var da = (basicPay * 10m) / 100m;
        var grossPay = basicPay + hra + da;
        var pf = (basicPay * 12m) / 100m;
        var esi = (grossPay * 1.75m) / 100m;
        var netPay = grossPay - (pf + esi);

        var salary = new Salary
        {
            SalaryMonth = request.SalaryMonth,
            SalaryYear = request.SalaryYear,
            EmployeeCode = request.EmployeeCode,
            Present = request.Present,
            LossOfPay = request.LossOfPay,
            BasicSalary = basicSalary,
            BasicPay = basicPay,
            HRA = hra,
            DA = da,
            GrossPay = grossPay,
            PF = pf,
            ESI = esi,
            NetPay = netPay
        };

        _context.Salaries.Add(salary);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
        {
            return Problem(
                detail: $"Database error while saving salary (SQL error {sqlEx.Number}).",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return CreatedAtAction(nameof(GetAll), new { id = salary.Id }, salary);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Salary>>> GetAll()
    {
        var salaries = await _context.Salaries
            .OrderByDescending(s => s.SalaryYear)
            .ThenByDescending(s => s.SalaryMonth)
            .ToListAsync();
        return salaries;
    }
}


