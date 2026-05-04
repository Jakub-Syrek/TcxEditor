using TcxEditor.Models;
using TcxEditor.Services;

namespace TcxEditor.Repositories;

// Repository pattern — abstracts TCX file persistence behind an interface
public class TcxFileRepository : ITcxRepository
{
    public TcxDatabase Load(string path) => TcxService.Load(path);
    public void Save(TcxDatabase db, string path) => TcxService.Save(db, path);
}
