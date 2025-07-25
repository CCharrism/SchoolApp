using System.Data.SQLite;

var connectionString = "Data Source=api/schoolapp.db";

Console.WriteLine("Adding IsActive column to Users table...");

try
{
    using var connection = new SQLiteConnection(connectionString);
    connection.Open();

    // First, check if the column already exists
    var checkColumnCommand = connection.CreateCommand();
    checkColumnCommand.CommandText = "PRAGMA table_info(Users);";
    
    bool hasIsActiveColumn = false;
    using (var reader = checkColumnCommand.ExecuteReader())
    {
        while (reader.Read())
        {
            var columnName = reader.GetString("name");
            if (columnName == "IsActive")
            {
                hasIsActiveColumn = true;
                break;
            }
        }
    }

    if (!hasIsActiveColumn)
    {
        // Add the IsActive column
        var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = "ALTER TABLE Users ADD COLUMN IsActive INTEGER NOT NULL DEFAULT 1;";
        alterCommand.ExecuteNonQuery();
        Console.WriteLine("IsActive column added successfully.");
    }
    else
    {
        Console.WriteLine("IsActive column already exists.");
    }

    // Mark existing migrations as applied if they aren't already
    var insertMigrationsCommand = connection.CreateCommand();
    insertMigrationsCommand.CommandText = @"
    INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES 
    ('20250723071234_AddSchoolsTable', '9.0.0'),
    ('20250723075106_AddSchoolSettings', '9.0.0'), 
    ('20250723081435_UpdateSchoolSettingsConstraints', '9.0.0'),
    ('20250723095215_AddBranchesAndCoursesSimplified', '9.0.0'),
    ('20250725073326_AddIsActiveToUsers', '9.0.0');";
    
    var affectedRows = insertMigrationsCommand.ExecuteNonQuery();
    Console.WriteLine($"Marked {affectedRows} migrations as applied.");

    Console.WriteLine("Database update completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
