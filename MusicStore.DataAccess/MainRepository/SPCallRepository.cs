using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MusicStore.DataAccess.Data;
using MusicStore.DataAccess.IMainRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.DataAccess.MainRepository
{
    public class SPCallRepository : ISPCallRepository
    {
        private readonly ApplicationDbContext _context;
        private static string _connectionString = "";

        public SPCallRepository(ApplicationDbContext context)
        {
            _context = context;
            _connectionString = _context.Database.GetDbConnection().ConnectionString;
        }

        public void Dispose()
        {
            _context.Dispose(); // İş bittikten sonra ramden atılacak
        }

        public void Execute(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open(); // bağlantıyı açtık
                connection.Execute(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
                // using içinde açtığımız için bağlantıyı kendi kapatıcak
            }
        }

        public IEnumerable<T> List<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<T>(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var result = SqlMapper.QueryMultiple(connection, procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
                var t1 = result.Read<T1>().ToList();
                var t2 = result.Read<T2>().ToList();

                if (t1 != null && t2 != null)
                {
                    return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(t1, t2);
                }
                //  Çoklu query gönderiyoruz.
            }
            return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(new List<T1>(), new List<T2>());
            // İf içerisine giremezse newleyerek geri döndürüyoruz hatanın önüne geçmiş oluyoruz
        }

        public T OneRecord<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection connection = new SqlConnection())
            {
                connection.Open();
                var value = connection.Query<T>(procedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return (T)Convert.ChangeType(value.FirstOrDefault(), typeof(T));
                // T tipinde döneceğimiz için datayı ve dönülecek tipi verdik
            }
        }

        public T Single<T>(string procedureName, DynamicParameters parameters = null)
        {
            using (SqlConnection connection = new SqlConnection())
            {
                connection.Open();
                return (T)Convert.ChangeType(connection.ExecuteScalar<T>(procedureName,parameters, commandType: System.Data.CommandType.StoredProcedure), typeof(T));
                // T tipinde döneceğimiz için datayı ve dönülecek tipi verdik
            }
        }
    }
}
