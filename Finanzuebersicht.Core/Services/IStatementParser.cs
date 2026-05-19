using System.Collections.Generic;
using System.IO;

namespace Finanzuebersicht.Core.Services
{
    public interface IStatementParser
    {
        IEnumerable<TransactionDto> Parse(Stream csvStream);
    }
}
