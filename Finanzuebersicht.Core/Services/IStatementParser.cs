using System.Collections.Generic;
using System.IO;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services
{
    public interface IStatementParser
    {
        IEnumerable<Transaction> Parse(Stream csvStream);
    }
}
