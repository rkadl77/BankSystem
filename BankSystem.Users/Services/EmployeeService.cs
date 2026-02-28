using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankSystem.Users.Data;
using BankSystem.Users.Models;
using BankSystem.Users.DTOs;

namespace BankSystem.Users.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly UsersDbContext _context;
        private readonly IUserService _userService;

        public EmployeeService(UsersDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return null;

            var user = await _userService.GetUserByIdAsync(employee.UserId);

            return new EmployeeDto
            {
                Id = employee.Id,
                UserId = employee.UserId,
                FirstName = user?.FirstName ?? "",
                LastName = user?.LastName ?? "",
                Email = user?.Email ?? "",
                Position = employee.Position,
                Department = employee.Department,
                HireDate = employee.HireDate,
                IsActive = employee.IsActive
            };
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _context.Employees.ToListAsync();
            var result = new List<EmployeeDto>();

            foreach (var employee in employees)
            {
                var user = await _userService.GetUserByIdAsync(employee.UserId);
                result.Add(new EmployeeDto
                {
                    Id = employee.Id,
                    UserId = employee.UserId,
                    FirstName = user?.FirstName ?? "",
                    LastName = user?.LastName ?? "",
                    Email = user?.Email ?? "",
                    Position = employee.Position,
                    Department = employee.Department,
                    HireDate = employee.HireDate,
                    IsActive = employee.IsActive
                });
            }

            return result;
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            var createUserRequest = new CreateUserRequest
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Role = "employee"
            };

            var user = await _userService.CreateUserAsync(createUserRequest);

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Position = request.Position,
                Department = request.Department,
                HireDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return new EmployeeDto
            {
                Id = employee.Id,
                UserId = employee.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Position = employee.Position,
                Department = employee.Department,
                HireDate = employee.HireDate,
                IsActive = employee.IsActive
            };
        }

        public async Task<bool> FireEmployeeAsync(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            employee.IsActive = false;

            var user = await _context.Users.FindAsync(employee.UserId);
            if (user != null)
            {
                user.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(string department)
        {
            var employees = await _context.Employees
                .Where(e => e.Department == department && e.IsActive)
                .ToListAsync();

            var result = new List<EmployeeDto>();
            foreach (var employee in employees)
            {
                var user = await _userService.GetUserByIdAsync(employee.UserId);
                result.Add(new EmployeeDto
                {
                    Id = employee.Id,
                    UserId = employee.UserId,
                    FirstName = user?.FirstName ?? "",
                    LastName = user?.LastName ?? "",
                    Email = user?.Email ?? "",
                    Position = employee.Position,
                    Department = employee.Department,
                    HireDate = employee.HireDate,
                    IsActive = employee.IsActive
                });
            }

            return result;
        }
    }
}