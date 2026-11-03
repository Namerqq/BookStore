using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BookyMart.Utilities
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("BookyMart")
                ?? throw new InvalidOperationException("Thiếu chuỗi kết nối BookyMart.");
        }

        public SqlConnection Create()
        {
            return new SqlConnection(_connectionString);
        }
    }
}