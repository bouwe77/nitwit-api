using Data.Entities;
using SQLite;

namespace Data
{
    internal class TableCreator
    {
        public void CreateTablesIfNecessary(SQLiteConnection connection)
        {
            connection.CreateTable<User>();
            connection.CreateTable<Post>();
            connection.CreateTable<Following>();
            connection.CreateTable<ToDo>();
        }
    }
}
