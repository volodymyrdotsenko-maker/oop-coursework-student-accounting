using System;
using System.Collections.Generic;

namespace StudentPerformanceApp
{
    /// <summary>
    /// Представляє сутність студента та інформацію про його успішність.
    /// </summary>
    public class Student
    {
        private string name;
        private string groupCode;
        private List<int> grades;

        public Student(string name, string groupCode, List<int> grades)
        {
            Name = name;
            GroupCode = groupCode;
            this.grades = grades ?? new List<int>();
        }

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ім'я студента не може бути порожнім.");
                name = value.Trim();
            }
        }

        public string GroupCode
        {
            get => groupCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Код групи не може бути порожнім.");
                groupCode = value.Trim();
            }
        }

        public List<int> Grades => new List<int>(grades);

        public void AddGrade(int grade)
        {
            if (grade < 1 || grade > 100)
                throw new ArgumentOutOfRangeException(nameof(grade), "Оцінка повинна бути в межах від 1 до 100.");
            grades.Add(grade);
        }

        public double CalculateAverageGrade()
        {
            if (grades.Count == 0) return 0.0;
            double sum = 0;
            foreach (var grade in grades) sum += grade;
            return sum / grades.Count;
        }
    }

    // Тимчасовий порожній каркас програми для другого коміту
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Модель даних завантажено. Програма в процесі розробки...");
        }
    }
}