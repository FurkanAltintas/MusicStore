using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MusicStore.Utility
{
    public static class SessionExtension
    {
        //  Tanımlı olan değerleri bir kere yapabilmemiz lazım

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
            // null ise default bir boş T nesnesi dönücek
        }

        public static void SetObject(this ISession session, string key, object value)
            {
            session.SetString(key, JsonConvert.SerializeObject(value));
            // string value kısmını JsonConvert ile yapıyoruz. Biz bu kısmı aynı zamanda frontend tarafında da kullanacağımız için böyle yapıyoruz
        }
    }
}
