﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Web.Models;
using EmployeeManagement.Web.Repository;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagement.Web.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagement.Web.EmployeeManagement.Core.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IManager _manager;
        private readonly EmployeeViewModel employeeViewModel;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly INotificationRepository _notificationRepository;

        public EmployeeController
            (
                AppDbContext context,
                IEmployeeRepository employeeRepository,
                IManager manager,
                UserManager<IdentityUser> userManager,
                INotificationRepository notificationRepository
            )
        {
            _context = context;
            _employeeRepository = employeeRepository;
            _manager = manager;
            employeeViewModel = new EmployeeViewModel();
            _userManager = userManager;
            _notificationRepository = notificationRepository;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _employeeRepository.GetAllEmployees());
        }

        public IActionResult Create()
        {
            ViewData["DepartmentName"] = _employeeRepository.DepartmentListName();
            ViewData["RolesName"] = _employeeRepository.RoleListName();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Eid,Name,Surname,Address,Qualification,ContactNumber,Did,RoleId")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.UserId = await _manager.AddUserManager(employee);
                if(employee.UserId != null)
                {
                    _employeeRepository.AddEmployee(employee);

                    var user = _userManager.GetUserAsync(HttpContext.User).Result;
                    Employee currentEmployee = await _employeeRepository.GetEmployeeByUserId(user.Id);

                    await _notificationRepository.AddEmployeeNotification(currentEmployee.Name + " "+ currentEmployee.Surname, employee.Did);

                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["DepartmentName"] = _employeeRepository.DepartmentListId(employee.Did);
            ViewData["RolesName"] = _employeeRepository.RoleListName();
            return View(employee);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Employee employee = _employeeRepository.GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["DepartmentName"] = _employeeRepository.DepartmentListName(employee.Did);
            ViewData["RolesName"] = _employeeRepository.RoleListName();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Eid,Name,Surname,Address,Qualification,ContactNumber,Did,UserId,RoleId")] Employee employee)
        {
            if (id != employee.Eid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    if (await _manager.UpdateUserManager(employee))
                    {
                        _employeeRepository.UpdateEmployee(employee);


                        var user = _userManager.GetUserAsync(HttpContext.User).Result;
                        Employee currentEmployee = await _employeeRepository.GetEmployeeByUserId(user.Id);

                        await _notificationRepository.EditEmployeeNotification(currentEmployee.Name + " "+ currentEmployee.Surname, employee.Did);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Eid))
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
            ViewData["DepartmentName"] = _employeeRepository.DepartmentListId(employee.Did);
            ViewData["RolesName"] = _employeeRepository.RoleListName();
            return View(employee);
        }

        public async Task<IActionResult> Delete(int id)
        {
            Employee employee = _employeeRepository.GetEmployeeById(id);
            if (await _manager.DeleteUserManager(employee.UserId))
            {
                _employeeRepository.DeleteEmployee(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Eid == id);
        }

    }
}
