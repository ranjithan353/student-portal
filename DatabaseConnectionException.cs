using System;

namespace StudentAttendanceManagementSystem1
{
    class DatabaseConnectionException : Exception
    {
        public DatabaseConnectionException(string message) : base(message) { }
    }
}