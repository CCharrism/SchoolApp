using Microsoft.Data.Sqlite;

Console.WriteLine("Marking existing migrations as applied...");

var connectionString = "Data Source=schoolapp.db";
using var connection = new SqliteConnection(connectionString);
connection.Open();

var insertCommand = connection.CreateCommand();
insertCommand.CommandText = @"
INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES 
('20250723071234_AddSchoolsTable', '9.0.0'),
('20250723075106_AddSchoolSettings', '9.0.0'), 
('20250723081435_UpdateSchoolSettingsConstraints', '9.0.0'),
('20250723095215_AddBranchesAndCoursesSimplified', '9.0.0');";

insertCommand.ExecuteNonQuery();

Console.WriteLine("Migrations marked as applied.");
