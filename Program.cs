using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;      

namespace StudentAttendanceManagementSystem1
{
    class Program
    {
        static Dictionary<int, List<bool>> attendanceRecords = new Dictionary<int, List<bool>>();
        static List<User> users = new List<User>();
        static int studentIdCounter = 1;
        static object attendanceLock = new object(); // For concurrency control

        static void Main(string[] args)
        {
            users.Add(new Teacher("John", "teacher123"));
            users.Add(new Teacher("Ranju", "ranju353"));

            while (true)
            {
                Console.WriteLine("Login as: \n1. Teacher\n2. Student\n3. Exit");
                
                Console.Write(" Please select from the given options: ");
        
                string userInput = Console.ReadLine()?.Trim().ToLower();
                try
                {
                    switch (userInput)
                    {
                        case "1":
                        case "teacher":
                            LoginTeacher();
                            break;

                        case "2":
                        case "student":
                            LoginStudent();
                            break;

                        case "3":
                        case "exit":
                            Console.WriteLine("Exiting...");
                            return;

                        default:
                            throw new InvalidOperationException("Invalid choice. Try again.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static void LoginTeacher()
        {
            Console.WriteLine("Enter Teacher Name:");
            string name = Console.ReadLine();
            Console.WriteLine("Enter Password:");
            string password = Console.ReadLine();

            var teacher = users.Find(u => u is Teacher && u.Name == name && u.Password == password);
            if (teacher != null)
            {
                TeacherMenu();
            }
            else
            {
                Console.WriteLine("Invalid credentials.");
            }
        }

        static void LoginStudent()
        {
            Console.WriteLine("Enter Student Name:");
            string name = Console.ReadLine();
            Console.WriteLine("Enter Password:");
            string password = Console.ReadLine();

            var student = users.Find(u => u is Student && u.Name == name && u.Password == password);
            if (student != null)
            {
                ViewAttendance(((Student)student).Id);
            }
            else
            {
                Console.WriteLine("Invalid credentials.");
            }
        }

        static void TeacherMenu()
        {
            while (true)
            {
                Console.WriteLine("1. Add Student\n2. Mark Attendance\n3. View Attendance\n4. Exit");
                Console.Write(" Please select from the given options: ");

                int choice = GetValidIntegerInput();

                try
                {
                    switch (choice)
                    {
                        case 1:
                            AddStudent();
                            break;
                        case 2:
                            MarkAttendance();
                            break;
                        case 3:
                            ViewAttendanceForTeacher();
                            break;
                        case 4:
                            return;
                        default:
                            throw new InvalidOperationException("Invalid choice.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static void AddStudent()
        {
            try
            {
                Console.WriteLine("Enter Student Name:");
                string name = Console.ReadLine();

                if (string.IsNullOrEmpty(name) || !IsValidName(name))
                    throw new InvalidOperationException("Invalid name. Name should contain only alphabets.");

                Console.WriteLine("Enter Password:");
                string password = Console.ReadLine();

                if (string.IsNullOrEmpty(password))
                    throw new InvalidOperationException("Password cannot be empty.");

                var student = new Student(name, password, studentIdCounter++);
                users.Add(student);
                lock (attendanceLock)
                {
                    attendanceRecords[student.Id] = new List<bool>();
                }

                Console.WriteLine($"Student {name} added with ID {student.Id}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

       static void MarkAttendance()
{
    try
    {
        Console.WriteLine("Enter Student ID:");
        int id = GetValidIntegerInput();

        lock (attendanceLock)
        {
            if (!attendanceRecords.ContainsKey(id))
                throw new InvalidOperationException("Invalid Student ID.");
        }

        Console.WriteLine("Mark attendance:\n1. Present\n2. Absent");
        int choice = GetValidIntegerInput();

        bool isPresent;
        string statusMsg;

        switch (choice)
        {
            case 1:
                isPresent = true;
                statusMsg = "Present";
                break;
            case 2:
                isPresent = false;
                statusMsg = "Absent";
                break;
            default:
                throw new InvalidOperationException("Invalid choice. Try again.");
            
        }

        // Simulate multiple teachers accessing the same student record concurrently
        Console.WriteLine("\n multiple teachers marking attendance concurrently...\n");
        Thread[] teacherThreads = new Thread[3];

        for (int i = 0; i < teacherThreads.Length; i++)
        {
            int teacherNum = i + 1;
            teacherThreads[i] = new Thread(() =>
            {
                lock (attendanceLock)
                {
                    Console.WriteLine($"[Teacher {teacherNum}] accessing attendance record for Student ID {id}...");
                    Thread.Sleep(300); // Simulate processing delay
                    Console.WriteLine($"[Teacher {teacherNum}] marked attendance successfully.");
                }
            });
            teacherThreads[i].Start();
        }

        foreach (var t in teacherThreads)
        {
            t.Join();
        }
        
        lock (attendanceLock)
        {
            attendanceRecords[id].Add(isPresent);
        }

        Console.WriteLine($"\nMarked Student ID {id} as {statusMsg}.");
        MessageQueue.Send($"Student ID {id} marked {statusMsg}");
        Thread.Sleep(200);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
        static void ViewAttendance(int studentId)
        {
            lock (attendanceLock)
            {
                if (attendanceRecords.ContainsKey(studentId))
                {
                    int presentCount = attendanceRecords[studentId].FindAll(a => a).Count;
                    Console.WriteLine($"Attendance Status: No of Days Present  {presentCount}.");
                }
                else
                {
                    Console.WriteLine("No records found.");
                }
            }
        }

        static void ViewAttendanceForTeacher()
        {
            try
            {
                lock (attendanceLock)
                {
                    if (attendanceRecords.Count == 0)
                    {
                        Console.WriteLine("No attendance records found.");
                        return;
                    }

                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("| Student ID |     Name     |   Attendance Status   |");
                    Console.WriteLine("--------------------------------------------------");

                    foreach (var student in users.OfType<Student>())
                    {
                        int studentId = student.Id;
                        string status;

                        if (attendanceRecords.ContainsKey(studentId) && attendanceRecords[studentId].Count > 0)
                        {
                            bool lastAttendance = attendanceRecords[studentId].Last();
                            status = lastAttendance ? "Present" : "Absent";
                        }
                        else
                        {
                            status = "No Record";
                        }

                        Console.WriteLine($"| {studentId,-11} | {student.Name,-12} | {status,-20} |");
                    }

                    Console.WriteLine("--------------------------------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static bool IsValidName(string name)
        {
            foreach (char c in name)
            {
                if (!char.IsLetter(c) && c != ' ')
                    return false;
            }
            return true;
        }

        static int GetValidIntegerInput()
        {
            int result;
            while (!int.TryParse(Console.ReadLine(), out result))
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
            return result;
        }
    }
}