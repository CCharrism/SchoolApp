using Microsoft.Data.Sqlite;

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

// Now add the IsActive column
var alterCommand = connection.CreateCommand();
alterCommand.CommandText = "ALTER TABLE Users ADD COLUMN IsActive INTEGER NOT NULL DEFAULT 1;";
alterCommand.ExecuteNonQuery();
Console.WriteLine("IsActive column added to Users table.");

// Insert the migration record
var migrationCommand = connection.CreateCommand();
migrationCommand.CommandText = @"
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES 
('20250725073326_AddIsActiveToUsers', '9.0.0');";
migrationCommand.ExecuteNonQuery();
Console.WriteLine("IsActive migration marked as applied.");
