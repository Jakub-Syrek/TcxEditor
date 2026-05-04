using TcxEditor.Models;

namespace TcxEditor.Repositories;

public interface ITcxRepository
{
    TcxDatabase Load(string path);
    void Save(TcxDatabase db, string path);
}
