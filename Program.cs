using System;
using System.Collections.Generic;
using System.IO;

namespace StudentPerformanceApp
{
    /// <summary>
    /// Представляє модель студента з його оцінками.
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

    /// <summary>
    /// Керує колекцією студентів та забезпечує роботу з файлами.
    /// </summary>
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
        private static StudentManager manager;
        private const string FilePath = "students_database.csv";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            manager = new StudentManager(FilePath);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine("   СИСТЕМА ОБЛІКУ СТУДЕНТІВ ТА УСПІШНОСТІ НАВЧАННЯ");
                Console.WriteLine("==================================================");
                Console.WriteLine("1. Вивести список усіх студентів");
                Console.WriteLine("2. Додати нового студента");
                Console.WriteLine("3. Видалити студента за номером");
                Console.WriteLine("4. Додати оцінку студенту");
                Console.WriteLine("5. Редагувати всі оцінки студента");
                Console.WriteLine("6. Пошук студента за ім'ям");
                Console.WriteLine("7. Фільтрація студентів за кодом групи");
                Console.WriteLine("8. Зберегти зміни та вийти з програми");
                Console.WriteLine("==================================================");
                if (manager.IsDataChanged)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(" [*] Є незбережені зміни в базі даних.");
                    Console.ResetColor();
                }
                Console.Write("Оберіть пункт меню (1-8): ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": ShowAllStudents(); break;
                    case "2": CreateStudent(); break;
                    case "3": DeleteStudent(); break;
                    case "4": AppendGradeToStudent(); break;
                    case "5": EditStudentGrades(); break;
                    case "6": FindStudents(); break;
                    case "7": FilterStudents(); break;
                    case "8": ExitAndSave(); return;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nПомилка! Невідомий пункт меню. Натисніть Enter...");
                        Console.ResetColor();
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void ShowAllStudents()
        {
            Console.Clear();
            var list = manager.GetAllStudents();
            if (list.Count == 0)
            {
                Console.WriteLine("База даних порожня. Немає жодного студента.");
            }
            else
            {
                Console.WriteLine("{0,-5} {1,-30} {2,-12} {3,-20} {4}", "№", "ПІБ Студента", "Група", "Оцінки", "Сер. бал");
                Console.WriteLine(new string('-', 75));
                for (int i = 0; i < list.Count; i++)
                {
                    double avg = list[i].CalculateAverageGrade();
                    Console.WriteLine("{0,-5} {1,-30} {2,-12} {3,-20} {4:F2}", 
                        i + 1, list[i].Name, list[i].GroupCode, list[i].GetGradesString(), avg);
                }
            }
            Console.WriteLine("\nНатисніть Enter для повернення в меню...");
            Console.ReadLine();
        }

        static void CreateStudent()
        {
            Console.Clear();
            Console.WriteLine("--- ДОДАННЯ НОВОГО СТУДЕНТА ---");
            Console.Write("Введіть ПІБ студента: ");
            string name = Console.ReadLine();
            Console.Write("Введіть код групи: ");
            string group = Console.ReadLine();

            try
            {
                Student student = new Student(name, group, null);
                manager.AddStudent(student);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nСтудента успішно додано до локального списку!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nПомилка створення: {ex.Message}");
            }
            Console.ResetColor();
            Console.WriteLine("Натисніть Enter...");
            Console.ReadLine();
        }

        static void DeleteStudent()
        {
            Console.Clear();
            Console.WriteLine("--- ВИДАЛЕННЯ СТУДЕНТА ---");
            Console.Write("Введіть порядковий номер студента для видалення: ");
            if (int.TryParse(Console.ReadLine(), out int index))
            {
                if (manager.RemoveStudentAt(index))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nСтудента успішно видалено.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nПомилка: Студента з таким номером не існує.");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nПомилка: Некоректний формат номера.");
            }
            Console.ResetColor();
            Console.WriteLine("Натисніть Enter...");
            Console.ReadLine();
        }

        static void AppendGradeToStudent()
        {
            Console.Clear();
            Console.WriteLine("--- ДОДАННЯ ОЦІНКИ ---");
            Console.Write("Введіть номер студента: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= manager.GetAllStudents().Count)
            {
                var student = manager.GetAllStudents()[index - 1];
                Console.Write($"Введіть оцінку (1-100) для {student.Name}: ");
                if (int.TryParse(Console.ReadLine(), out int grade))
                {
                    try
                    {
                        student.AddGrade(grade);
                        manager.AddStudent(null); 
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nОцінку додано.");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\nПомилка: {ex.Message}");
                    }
                }
                else Console.WriteLine("Некоректний формат оцінки.");
            }
            else Console.WriteLine("Студента не знайдено.");
            
            Console.ResetColor();
            Console.ReadLine();
        }

        static void EditStudentGrades()
        {
            Console.Clear();
            Console.WriteLine("--- РЕДАГУВАННЯ ВСІХ ОЦІНОК ---");
            Console.Write("Введіть номер студента: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= manager.GetAllStudents().Count)
            {
                var student = manager.GetAllStudents()[index - 1];
                Console.WriteLine($"Поточні оцінки {student.Name}: {student.GetGradesString()}");
                Console.Write("Введіть нові оцінки через кому (наприклад: 85,92,70) або порожній рядок для видалення всіх: ");
                string input = Console.ReadLine();

                List<int> newGrades = new List<int>();
                bool valid = true;

                if (!string.IsNullOrWhiteSpace(input))
                {
                    string[] tokens = input.Split(',');
                    foreach (var token in tokens)
                    {
                        if (int.TryParse(token.Trim(), out int g) && g >= 1 && g <= 100) newGrades.Add(g);
                        else { valid = false; break; }
                    }
                }

                if (valid)
                {
                    student.UpdateGrades(newGrades);
                    manager.AddStudent(null);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nОцінки успішно оновлено!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nПомилка: Оцінки мають бути числами від 1 до 100, розділеними комами.");
                }
            }
            else Console.WriteLine("Студента не знайдено.");
            
            Console.ResetColor();
            Console.ReadLine();
        }

        static void FindStudents()
        {
            Console.Clear();
            Console.WriteLine("--- ПОШУК СТУДЕНТІВ ---");
            Console.Write("Введіть частину імені для пошуку: ");
            string query = Console.ReadLine();
            var results = manager.SearchByName(query);

            if (results.Count == 0) Console.WriteLine("\nЗбігів не знайдено.");
            else
            {
                Console.WriteLine($"\nЗнайдено студентів: {results.Count}\n");
                foreach (var s in results)
                    Console.WriteLine($"- {s.Name} (Група: {s.GroupCode}), Сер. бал: {s.CalculateAverageGrade():F2}");
            }
            Console.ReadLine();
        }

        static void FilterStudents()
        {
            Console.Clear();
            Console.WriteLine("--- ФІЛЬТРАЦІЯ ЗА ГРУПОЮ ---");
            Console.Write("Введіть точний код групи: ");
            string group = Console.ReadLine();
            var results = manager.FilterByGroup(group);

            if (results.Count == 0) Console.WriteLine("\nУ цій групі немає студентів.");
            else
            {
                Console.WriteLine($"\nСтуденти групи {group.ToUpper()}:\n");
                foreach (var s in results)
                    Console.WriteLine($"- {s.Name}, Оцінки: {s.GetGradesString()}");
            }
            Console.ReadLine();
        }

        static void ExitAndSave()
        {
            Console.Clear();
            Console.WriteLine("Збереження бази даних у файл...");
            try
            {
                manager.SaveDataToFile();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Дані успішно збережено у файл students_database.csv!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка збереження: {ex.Message}");
            }
            Console.ResetColor();
            Console.WriteLine("Дякуємо за використання програми. До побачення!");
            System.Threading.Thread.Sleep(2000);
        }
    }
}