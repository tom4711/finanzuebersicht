using Finanzuebersicht.Models;

namespace Finanzuebersicht.Presentation.Services;

public class ImportSessionStore : IImportSessionStore
{
    private ImportPreviewResult? _activeSession;

    public void SetActiveSession(ImportPreviewResult preview)
    {
        _activeSession = preview;
    }

    public ImportPreviewResult? GetActiveSession() => _activeSession;

    public void Clear()
    {
        _activeSession = null;
    }
}
