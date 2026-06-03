using System;
using System.Collections.Generic;
using System.IO;

namespace StudentPerformanceApp
{
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
                throw new ArgumentOutOfRangeException(nameof(grade), "Оцінка повинна бути v межах від 1 до 100.");
            grades.Add(grade);
        }

        public void UpdateGrades(List<int> newGrades)
        {
            if (newGrades == null) return;
            foreach (var grade in newGrades)
            {
                if (grade < 1 || grade > 100)
                    throw new ArgumentException("Всі оцінки мають бути від 1 до 100.");
            }
            grades = new List<int>(newGrades);
        }

        public double CalculateAverageGrade()
        {
            if (grades.Count == 0) return 0.0;
            double sum = 0;
            foreach (var grade in grades) sum += grade;
            return sum / grades.Count;
        }

        public string GetGradesString() => grades.Count == 0 ? "Оцінки відсутні" : string.Join(", ", grades);
    }

    public class StudentManager
    {
        private readonly List<Student> students;
        private readonly string dataFilePath;
        private bool isDataChanged;

        public StudentManager(string fileName)
        {
            students = new List<Student>();
            dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            isDataChanged = false;
            LoadDataFromFile();
        }

        public bool IsDataChanged => isDataChanged;
        public List<Student> GetAllStudents() => students;

        public void AddStudent(Student student)
        {
            if (student != null) { students.Add(student); isDataChanged = true; }
        }

        public bool RemoveStudentAt(int displayIndex)
        {
            int internalIndex = displayIndex - 1;
            if (internalIndex >= 0 && internalIndex < students.Count)
            {
                students.RemoveAt(internalIndex);
                isDataChanged = true;
                return true;
            }
            return false;
        }

        public List<Student> SearchByName(string substring)
        {
            var results = new List<Student>();
            if (string.IsNullOrWhiteSpace(substring)) return results;
            foreach (var s in students)
            {
                if (s.Name.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0) results.Add(s);
            }
            return results;
        }

        public List<Student> FilterByGroup(string groupCode)
        {
            var results = new List<Student>();
            if (string.IsNullOrWhiteSpace(groupCode)) return results;
            foreach (var s in students)
            {
                if (s.GroupCode.Equals(groupCode.Trim(), StringComparison.OrdinalIgnoreCase)) results.Add(s);
            }
            return results;
        }

        public void SaveDataToFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(dataFilePath, false, System.Text.Encoding.UTF8))
                {
                    foreach (var student in students)
                    {
                        string gradesData = string.Join(",", student.Grades);
                        writer.WriteLine($"{student.Name};{student.GroupCode};{gradesData}");
                    }
                }
                isDataChanged = false;
            }
            catch (Exception) { throw new IOException("Помилка запису у файл."); }
        }

        private void LoadDataFromFile()
        {
            if (!File.Exists(dataFilePath)) return;
            try
            {
                string[] lines = File.ReadAllLines(dataFilePath, System.Text.Encoding.UTF8);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split(';');
                    if (parts.Length < 2) continue;
                    List<int> grades = new List<int>();
                    if (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]))
                    {
                        foreach (string gPart in parts[2].Split(','))
                        {
                            if (int.TryParse(gPart, out int grade)) grades.Add(grade);
                        }
                    }
                    students.Add(new Student(parts[0], parts[1], grades));
                }
            }
            catch (Exception) { students.Clear(); }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Логіку збереження додано. Інтерфейс у розробці...");
        }
    }
}