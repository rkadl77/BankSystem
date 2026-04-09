using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankSystem.Users.DTOs;

namespace BankSystem.Users.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> GetEmployeeByIdAsync(Guid id);
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request);
        Task<bool> FireEmployeeAsync(Guid id);
        Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(string department);
    }
}