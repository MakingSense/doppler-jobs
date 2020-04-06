using System.Collections.Generic;
using System.Threading.Tasks;
using Doppler.Sap.Job.Service.Database.Entities;

namespace Doppler.Sap.Job.Service.Database
{
    public interface IDopplerRepository
    {
        public Task<IEnumerable<UserBilling>> GetUserBillingInformation();
    }
}
