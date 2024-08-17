using Dapper;
using MesDataCollection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MesDataCollection.Repository
{
    public class UserRepository : BaseRepository
    {
        public UserRepository()
        {
        }
        
        public async Task<UserModel> login(string passwort)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<UserModel>(
                      "select  * from Mes_user where password=@password ",
                      new
                      {
                          password = passwort
                      });
                return result.FirstOrDefault();
            }
        }


        public async Task<List<UserModel>> getUser()
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<UserModel>(
                      "select *  from Mes_user " );
                return result.ToList();
            }
        }


        public async Task UpdateUser(string id, string passwort)
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(
                    "update Mes_user set  passwort=@passwort where id =@id  ",
                    new
                    {
                        passwort = passwort,
                        id= id
                    });
            }
        }

        public async Task deleteUser(string id)
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(
                    "delete from  Mes_user  where id =@id  ",
                    new
                    {
                        id = id
                    });
            }
        }

        public async Task addUser(UserModel user)
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(
                    "INSERT INTO `mes_user` ( `password`, `type`) VALUES ( @password, @type);",
                    new
                    {
                        password = user.password,
                        type = user.type
                    });
            }
        }

    }
}
