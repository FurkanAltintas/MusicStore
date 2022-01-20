using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.IMainRepository
{
    public interface IRepository<T> where T : class
    {
        // Generic tipler T tipinde belirtilir (yazılı olmayan bir kural)
        // Bunu nerede kullanıcaksak kullanalım Repository tanımlarken vereceğimiz tip class olmak zorunda

        T Get(int id); // id değerine göre T değerini getir (Tek bir tane getirir)

        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>,
                              IOrderedQueryable<T>> orderBy = null,
                              string includeProperty = null);

        // Expression<Func<T, bool>> filter = null, (bir filtreleme ayarladık)
        // Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null (sıralama yaptık)
        //  Filtreleme yapabiliriz, sıralayabiliriz ve o tablonun ilişkili olduğu tablonunda değerlerini getirmemiz gerekebilir.

        T GetFirstOrDefault(Expression<Func<T, bool>> filter = null, string includeProperties = null);

        void Add(T entity); // Ekleme

        void Remove(int id); // Kayıt silme id değerine göre

        void Remove(T entity); // Kayıt silme

        void RemoveRange(IEnumerable<T> entities); // Çoklu Silme
    }
}
