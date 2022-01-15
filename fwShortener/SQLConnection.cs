namespace fwShortener;
using Microsoft.Data.Sqlite;

public class SQLConnection
{
    public static SqliteConnection Connection;
    
    public static void Init()
    {
        Connection = new SqliteConnection("Data Source=urls.db");
        Connection.Open();

        GenCommand("CREATE TABLE IF NOT EXISTS urls (id varchar(12) PRIMARY KEY, url varchar(255));").ExecuteNonQuery();
        GenCommand("CREATE TABLE IF NOT EXISTS reports (url varchar(255), msg varchar(16384));").ExecuteNonQuery();
    }

    public static string GetUrl(string id)
    {
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"SELECT * FROM urls WHERE id=$id;";
        command.Parameters.AddWithValue("$id", id);
        
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string url = reader.GetString(1);
                return url;
            }
        }

        return "";
    }
    
    public static async Task<bool> RemoveUrl(string id)
    {
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"DELETE FROM urls WHERE id=$id;";
        command.Parameters.AddWithValue("$id", id);
        return await command.ExecuteNonQueryAsync() > 0;
    }
    
    public static async Task<bool> RemoveReport(string rid)
    {
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"DELETE FROM reports WHERE rowid=$rid;";
        command.Parameters.AddWithValue("$rid", rid);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public record UrlsRecord(string Id, string Url);
    public static async Task <List<UrlsRecord>> GetUrls(int from = 0, int to = int.MaxValue)
    {
        List<UrlsRecord> urls = new List<UrlsRecord>();

        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"SELECT * FROM urls LIMIT {from},{to};";
        using (var reader = await command.ExecuteReaderAsync())
        {
            for (int i = 0; reader.Read(); i++)
            {
                urls.Add(new UrlsRecord(reader.GetString(0), reader.GetString(1)));
            }
        }

        return urls;
    }
    
    public record ReportRecord(string RowId, string Url, string Msg);
    public static async Task <List<ReportRecord>> GetReports(int from = 0, int to = int.MaxValue)
    {
        List<ReportRecord> urls = new List<ReportRecord>();

        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"SELECT rowid, * FROM reports LIMIT {from},{to};";
        using (var reader = await command.ExecuteReaderAsync())
        {
            for (int i = 0; reader.Read(); i++)
            {
                urls.Add(new ReportRecord( reader.GetString(0),reader.GetString(1), reader.GetString(2)));
            }
        }

        return urls;
    }

    public static async Task<int> GetUrlsCount()
    {
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM urls;";
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
    
    public static async Task<int> GetReportsCount()
    {
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM reports;";
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public static void SetUrl(string id, string url)
    {
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"INSERT INTO urls (id, url) VALUES ($id, $url);";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$url", url);
        command.ExecuteNonQuery();
    }
    
    public static string GetAllReports()
    {
        string output = "LIST: \n";
        
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"SELECT * FROM reports;";
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                output += $"{reader.GetString(0)} => {reader.GetString(1)}\n";
            }
        }

        return output;
    }
    
    public static void AddReport(string url, string msg)
    {
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"INSERT INTO reports (url, msg) VALUES ($url, $msg);";
        command.Parameters.AddWithValue("$url", url);
        command.Parameters.AddWithValue("$msg", msg);
        command.ExecuteNonQuery();
    }

    public static SqliteCommand GenCommand(string cmd)
    {
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = cmd;
        return command;
    }
    
    private static Random random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}