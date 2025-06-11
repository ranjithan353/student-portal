namespace StudentAttendanceManagementSystem1
{
    class Student : User
    {
        public int Id { get; private set; }

        public Student(string name, string password, int id) : base(name, password)
        {
            Id = id;
        }
    }
}