using System.Data.Common;

namespace Sample.Server.Data;

public static class DatabaseErrors
{
    // EF Core often wraps the actual SQL error (e.g. "A network-related or instance-specific error...")
    // in a generic exception; dig out the database error since that's the useful message.
    public static string Describe(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is DbException)
            {
                return current.Message;
            }
        }

        return ex.Message;
    }
}
