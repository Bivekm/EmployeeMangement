﻿using EmployeeManagement.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Web.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private AppDbContext _context;
        private RoleManager<IdentityRole> _roleManager;

        public EmployeeRepository(AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployees()
        {
             var joinDbContext = _context.Employees.Include(e => e.Department);
            return await joinDbContext.ToListAsync();
        }

        

        public Employee GetEmployeeById(int Eid)
        {
            return _context.Employees.Find(Eid);
        }


        public async void AddEmployee(Employee Employee)
        {
            await _context.Employees.AddAsync(Employee);
            SaveEmployee();
        }

        public void UpdateEmployee(Employee Employee)
        {
            _context.Update(Employee);
            SaveEmployee();
        }
        public void DeleteEmployee(int id)
        {
            Employee Employee = GetEmployeeById(id);
            _context.Employees.Remove(Employee);
            SaveEmployee();
        }

        public SelectList DepartmentListName()
        {
            return new SelectList(_context.Departments, "Did", "DepartmentName");
        }
        public SelectList DepartmentListName(int id)
        {
            return new SelectList(_context.Departments, "Did", "DepartmentName", id);
        }
        public SelectList DepartmentListId(int id)
        {
            return new SelectList(_context.Departments, "Did", "Did", id);
        }

        public void SaveEmployee()
        {
            _context.SaveChanges();
        }

        public async Task<IEnumerable<Employee>> FilterEmployee(int Did)
        {
            var Join = _context.Employees.Include(e => e.Department);
            List<Employee> employees = await Join.ToListAsync();
            return employees.Where(x => x.Did == Did);
        }

        public async Task<Employee> GetEmployeeByUserId(string userId)
        {
            return await _context.Employees.FirstAsync(x => x.UserId == userId);
        }

        public List<IdentityRole> RoleListName()
        {
            return new List<IdentityRole>(_roleManager.Roles.ToList());
        }



    }
}
