namespace SocietiesManagementSystem.Data.Repositories;

public interface IRepository<T>
{
    T? GetById(int id);
    IEnumerable<T> GetAll();
    int Insert(T entity);
    bool Update(T entity);
    bool Delete(int id);
}
