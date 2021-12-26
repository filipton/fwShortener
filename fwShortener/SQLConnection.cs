namespace fwShortener;
using Microsoft.Data.Sqlite;

public class SQLConnection
{
    public static SqliteConnection Connection;
    
    public static void Init()
    {
        Connection = new SqliteConnection("Data Source=urls.db");
        Connection.Open();
        
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS urls (id varchar(12) PRIMARY KEY, url varchar(255));";
        command.ExecuteNonQuery();
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

    public static string GerALlUrls()
    {
        string output = "LIST: \n";
        
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"SELECT * FROM urls;";
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                output += $"{reader.GetString(0)} => {reader.GetString(1)}\n";
            }
        }

        return output;
    }

    public static void SetUrl(string id, string url)
    {
        SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"INSERT INTO urls (id, url) VALUES ($id, $url);";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$url", url);
        command.ExecuteNonQuery();
    }
    
    private static Random random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}