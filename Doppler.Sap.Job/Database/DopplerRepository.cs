using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Doppler.Sap.Job.Service.Database.Entities;
using Doppler.Sap.Job.Service.Settings;
using Microsoft.Extensions.Options;

namespace Doppler.Sap.Job.Service.Database
{
    public class DopplerRepository : IDopplerRepository
    {
        private readonly DopplerRepositorySettings _dopplerSapServiceSettings;

        public DopplerRepository(IOptionsMonitor<DopplerRepositorySettings> dopplerSapServiceSettings)
        {
            _dopplerSapServiceSettings = dopplerSapServiceSettings.CurrentValue;
        }

        public async Task<IEnumerable<UserBilling>> GetUserBillingInformation()
        {
            await using var conn = new SqlConnection(_dopplerSapServiceSettings.ConnectionString);
            
            //TODO: Add sql sentence to get data for SAP
            const string query = "SELECT * FROM City";

            var result = await conn.QueryAsync<UserBilling>(query);

            return result;
        }
    }
}
