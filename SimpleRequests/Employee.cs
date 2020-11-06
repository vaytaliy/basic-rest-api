using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Simple_Requests
{
    class Employee
    {
        public Employee(string name, int salary, int age)
        {
            Name = name;
            Salary = salary;
            Age = age;
        }
        
        [JsonPropertyName("employee_name")]
        public string Name { get; set; }
        [JsonPropertyName("employee_salary")]
        public int Salary { get; set; }
        [JsonPropertyName("employee_age")]
        public int Age { get; set; }
    
        public override string ToString()
        {
            return
                $"Name: {Name}" +
              $"\nSalary: {Salary}" +
              $"\nAge: {Age}";
        }
    
    }
}
