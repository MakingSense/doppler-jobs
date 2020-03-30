using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace Doppler.Sap.Job.Service.Database
{
    public class DopplerRepository : IDopplerRepository
    {
        private readonly string _connectionString;

        public DopplerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<object>> GetBillingClientInformation()
        {
            await using var conn = new SqlConnection(_connectionString);
            
            //TODO: Add sql sentence to get data for SAP
            const string query = "SELECT * FROM City";

            var result = await conn.QueryAsync(query);

            return result;
        }
    }
}
