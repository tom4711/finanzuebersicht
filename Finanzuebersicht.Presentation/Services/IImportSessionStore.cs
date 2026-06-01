using Finanzuebersicht.Models;

namespace Finanzuebersicht.Presentation.Services;

public interface IImportSessionStore
{
    void SetActiveSession(ImportPreviewResult preview);
    ImportPreviewResult? GetActiveSession();
    void Clear();
}
