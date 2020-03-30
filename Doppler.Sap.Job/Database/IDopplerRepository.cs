using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.Sap.Job.Service.Database
{
    public interface IDopplerRepository
    {
        public Task<IEnumerable<object>> GetBillingClientInformation();
    }
}
