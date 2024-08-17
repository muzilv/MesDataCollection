using System;

namespace MesDataCollection
{
    public class UserModel
    {
        public int id { get; set; }
        public string password { get; set; }
        public string type { get; set; }
        public DateTime create_date { get; set; }
    }
}
