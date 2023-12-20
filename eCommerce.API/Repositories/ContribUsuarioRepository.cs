using eCommerce.API.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Data;

namespace eCommerce.API.Repositories
{
    public class ContribUsuarioRepository : IUsuarioRepository
    {
        private IDbConnection _connection;
        public ContribUsuarioRepository()
        {
            _connection = new SqlConnection(@"Server=GUI-DESKTOP;Database=eCommerce;Integrated Security=True;TrustServerCertificate=True");
        }

        public List<Usuario> GetAll()
        {
            return _connection.GetAll<Usuario>().ToList();
        }

        public Usuario Get(int id)
        {
            return _connection.Get<Usuario>(id);
        }

        public void Insert(Usuario usuario)
        {
            usuario.Id = (int)_connection.Insert(usuario);
        }

        public void Update(Usuario usuario)
        {
            _connection.Update(usuario);
        }

        public void Delete(int id)
        {
            _connection.Delete(Get(id));
        }
    }
}
