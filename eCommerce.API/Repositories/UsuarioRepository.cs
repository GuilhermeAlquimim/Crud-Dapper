using eCommerce.API.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace eCommerce.API.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private IDbConnection _connection;
        public UsuarioRepository()
        {
            _connection = new SqlConnection(@"Server=GUI-DESKTOP;Database=eCommerce;Integrated Security=True;TrustServerCertificate=True");
        }

        public List<Usuario> GetAll()
        {
            return _connection.Query<Usuario>("SELECT TOP 100 * FROM [Usuarios];").ToList();

            /*List<Usuario> usuarios = new List<Usuario>();
            string sql = "SELECT U.*, C.*, EE.*, D.* FROM [Usuarios] U LEFT JOIN [Contatos] C ON C.UsuarioId = U.Id LEFT JOIN [EnderecosEntrega] EE ON EE.UsuarioId = U.Id LEFT JOIN [UsuariosDepartamentos] UD ON UD.UsuarioId = U.Id LEFT JOIN [Departamentos] D ON D.Id = UD.DepartamentoId;";
            _connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(sql,
                (usuario, contato, enderecoEntrega, departamento) =>
                {
                    if (usuarios.SingleOrDefault(x => x.Id == usuario.Id) == null)
                    {
                        usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                        usuario.Departamentos = new List<Departamento>();
                        usuario.Contato = contato;
                        usuarios.Add(usuario);
                    }
                    else
                        usuario = usuarios.SingleOrDefault(x => x.Id == usuario.Id);

                    // Verificação do endereço de entrega (evitar duplicidade)
                    if (enderecoEntrega != null)
                        if (usuario.EnderecosEntrega.SingleOrDefault(x => x.Id == enderecoEntrega.Id) == null)
                            usuario.EnderecosEntrega.Add(enderecoEntrega);

                    // Verificação do departamento (evitar duplicidade)
                    if (departamento != null)
                        if (usuario.Departamentos.SingleOrDefault(x => x.Id == departamento.Id) == null)
                            usuario.Departamentos.Add(departamento);

                    return usuario;
                });
            return usuarios;*/
        }

        public Usuario Get(int id)
        {
            List<Usuario> usuarios = new List<Usuario>();
            string sql = "SELECT U.*, C.*, EE.*, D.* FROM [Usuarios] U LEFT JOIN [Contatos] C ON C.UsuarioId = U.Id LEFT JOIN [EnderecosEntrega] EE ON EE.UsuarioId = U.Id LEFT JOIN [UsuariosDepartamentos] UD ON UD.UsuarioId = U.Id LEFT JOIN [Departamentos] D ON D.Id = UD.DepartamentoId WHERE U.Id = @Id;";
            _connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(sql,
                (usuario, contato, enderecoEntrega, departamento) =>
                {
                    if (usuarios.SingleOrDefault(x => x.Id == usuario.Id) == null)
                    {
                        usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                        usuario.Departamentos = new List<Departamento>();
                        usuario.Contato = contato;
                        usuarios.Add(usuario);
                    }
                    else
                        usuario = usuarios.SingleOrDefault(x => x.Id == usuario.Id);

                    // Verificação do endereço de entrega (evitar duplicidade)
                    if (enderecoEntrega != null)
                        if (usuario.EnderecosEntrega.SingleOrDefault(x => x.Id == enderecoEntrega.Id) == null)
                            usuario.EnderecosEntrega.Add(enderecoEntrega);

                    // Verificação do departamento (evitar duplicidade)
                    if (departamento != null)
                        if (usuario.Departamentos.SingleOrDefault(x => x.Id == departamento.Id) == null)
                            usuario.Departamentos.Add(departamento);

                    return usuario;
                }, new {Id = id});
            return usuarios.SingleOrDefault();
        }

        public void Insert(Usuario usuario)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();
            try
            {
                string sql = "INSERT INTO Usuarios(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                usuario.Id = _connection.Query<int>(sql, usuario, transaction).Single();

                if (usuario.Contato != null)
                {
                    usuario.Contato.UsuarioId = usuario.Id;
                    string sqlContato = "INSERT INTO [Contatos](UsuarioId, Telefone, Celular) VALUES (@UsuarioId, @Telefone, @Celular); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    usuario.Contato.Id = _connection.Query<int>(sqlContato, usuario.Contato, transaction).Single();
                }

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var endereco in usuario.EnderecosEntrega)
                    {
                        endereco.UsuarioId = usuario.Id;
                        string sqlEndereco = "INSERT INTO [EnderecosEntrega](UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        endereco.Id = _connection.Query<int>(sqlEndereco, endereco, transaction).Single();
                    }
                }

                if (usuario.Departamentos != null && usuario.Departamentos.Count > 0)
                {
                    foreach (var departamento in usuario.Departamentos)
                    {
                        string sqlUsuariosDepartamento = "INSERT INTO [UsuariosDepartamentos](UsuarioId, DepartamentoId) VALUES(@UsuarioId, @DepartamentoId);";
                        _connection.Execute(sqlUsuariosDepartamento, new {UsuarioId = usuario.Id, DepartamentoId = departamento.Id}, transaction);
                    }
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine("\nErro ao inserir os dados.\n", e.Message);
                try
                {
                    transaction.Rollback();
                    Console.WriteLine("\nRollBack executado com êxito.\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nErro ao executar o RollBack.\n", ex.Message);
                }
            }
            finally
            {
                _connection.Close();
            }

        }

        public void Update(Usuario usuario)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();
            try
            {
                string sqlCmdUsuario = "UPDATE [Usuarios] SET Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG, CPF = @CPF, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro, DataCadastro = @DataCadastro WHERE Id = @Id; SELECT CAST(SCOPE_IDENTITY() AS INT);";
                _connection.Execute(sqlCmdUsuario, usuario, transaction);

                if (usuario.Contato != null)
                {
                    string sqlCmdContato = "UPDATE [Contatos] SET Telefone = @Telefone, Celular = @Celular WHERE Id = @Id;";
                    _connection.Execute(sqlCmdContato, usuario.Contato, transaction);
                }

                string sqlDelEndereco = "DELETE FROM [EnderecosEntrega] WHERE UsuarioId = @Id";
                _connection.Execute(sqlDelEndereco, usuario, transaction);

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var endereco in usuario.EnderecosEntrega)
                    {
                        endereco.UsuarioId = usuario.Id;
                        string sqlEndereco = "INSERT INTO EnderecosEntrega(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        endereco.Id = _connection.Query<int>(sqlEndereco, endereco, transaction).Single();
                    }
                }

                string sqlDelUsuariosDepartamentos = "DELETE FROM [UsuariosDepartamentos] WHERE UsuarioId = @Id";
                _connection.Execute(sqlDelUsuariosDepartamentos, usuario, transaction);

                if (usuario.Departamentos != null && usuario.Departamentos.Count > 0)
                {
                    foreach (var departamento in usuario.Departamentos)
                    {
                        string sqlUsuariosDepartamento = "INSERT INTO [UsuariosDepartamentos](UsuarioId, DepartamentoId) VALUES(@UsuarioId, @DepartamentoId);";
                        _connection.Execute(sqlUsuariosDepartamento, new { UsuarioId = usuario.Id, DepartamentoId = departamento.Id }, transaction);
                    }
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine("\nErro ao atualizar os dados.\n", e.Message);
                try
                {
                    transaction.Rollback();
                    Console.WriteLine("\nRollBack executado com êxito.\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nErro ao executar o RollBack.\n", ex.Message);
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Delete(int id)
        {
            _connection.Execute("DELETE FROM [Usuarios] WHERE Id = @Id;", new { Id = id });
        }
    }
}
