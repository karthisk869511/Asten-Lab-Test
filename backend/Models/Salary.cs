namespace Backend.Models;

public class Salary
{
    public int Id { get; set; }
    public string SalaryMonth { get; set; } = string.Empty;
    public int SalaryYear { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public int Present { get; set; }
    public int LossOfPay { get; set; }

    public decimal BasicSalary { get; set; }
    public decimal BasicPay { get; set; }
    public decimal HRA { get; set; }
    public decimal DA { get; set; }
    public decimal GrossPay { get; set; }
    public decimal PF { get; set; }
    public decimal ESI { get; set; }
    public decimal NetPay { get; set; }
}


