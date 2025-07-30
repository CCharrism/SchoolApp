using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using api.Data;

namespace api
{
    public class TestSqlServerConnection
    {
        public static async Task<bool> TestConnection(string connectionString, string testName)
        {
            try
            {
                Console.WriteLine($"\nTesting {testName}...");
                Console.WriteLine($"Connection String: {connectionString}");
                
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                Console.WriteLine($"✅ {testName} - Connection successful!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {testName} - Connection failed: {ex.Message}");
                return false;
            }
        }
        
        public static async Task TestAllConnections()
        {
            var connectionStrings = new Dictionary<string, string>
            {
                ["Named Instance"] = "Server=PPWKS-068\\HARRIS_SRV;Database=master;Integrated Security=true;TrustServerCertificate=true;",
                ["LocalDB"] = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;",
                ["SQL Express"] = "Server=.\\SQLEXPRESS;Database=master;Integrated Security=true;TrustServerCertificate=true;",
                ["Local Default"] = "Server=localhost;Database=master;Integrated Security=true;TrustServerCertificate=true;",
                ["Localhost with Port"] = "Server=localhost,1433;Database=master;Integrated Security=true;TrustServerCertificate=true;"
            };
            
            Console.WriteLine("Testing SQL Server Connections...");
            Console.WriteLine("=====================================");
            
            foreach (var (name, connectionString) in connectionStrings)
            {
                await TestConnection(connectionString, name);
                await Task.Delay(1000); // Small delay between tests
            }
            
            Console.WriteLine("\n=====================================");
            Console.WriteLine("Test completed. Use the working connection string in your appsettings.json");
        }
    }
}
