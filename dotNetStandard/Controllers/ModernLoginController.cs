using Atomus.Database;
using Atomus.Service;
using System.Threading.Tasks;

namespace Atomus.Page.Login.Controllers
{
    internal static class ModernLoginController
    {
        internal static async Task<IResponse> SearchAsync(this ICore core, string EMAIL, string ACCESS_NUMBER)
        {
            IServiceDataSet serviceDataSet;

            serviceDataSet = new ServiceDataSet { ServiceName = core.GetAttribute("ServiceName") };
            serviceDataSet["LOGIN"].ConnectionName = core.GetAttribute("DatabaseName");
            serviceDataSet["LOGIN"].CommandText = core.GetAttribute("ProcedureLogin");
            serviceDataSet["LOGIN"].AddParameter("@EMAIL", DbType.NVarChar, 100);
            serviceDataSet["LOGIN"].AddParameter("@ACCESS_NUMBER", DbType.NVarChar, 4000);

            serviceDataSet["LOGIN"].NewRow();
            serviceDataSet["LOGIN"].SetValue("@EMAIL", EMAIL);
            serviceDataSet["LOGIN"].SetValue("@ACCESS_NUMBER", ACCESS_NUMBER);

            return await core.ServiceRequestAsync(serviceDataSet);
        }
    }
}
