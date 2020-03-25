using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Doppler.Sap.Job.Service.Dtos;

namespace Doppler.Sap.Job.Service.DopplerSapService
{
    public interface IDopplerSapService
    {
        public Task<HttpResponseMessage> SendCurrency(IList<CurrencyDto> currencyList);
    }
}
