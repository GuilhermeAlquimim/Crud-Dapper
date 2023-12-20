using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using eCommerce.API.Models;
using Dapper.FluentMap;
using eCommerce.API.Mappers;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipsController : ControllerBase
    {
        private IDbConnection _connection;
        public TipsController()
        {
            _connection = new SqlConnection(@"Server=GUI-DESKTOP;Database=eCommerce;Integrated Security=True;TrustServerCertificate=True");
        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            string sql = "SELECT * FROM [Usuarios] WHERE Id = @Id;" + "SELECT * FROM [Contatos] WHERE UsuarioId = @Id;" + "SELECT * FROM [EnderecosEntrega] WHERE UsuarioId = @Id;" + "SELECT D.* FROM [UsuariosDepartamentos] UD INNER JOIN [Departamentos] D ON UD.DepartamentoId = D.Id WHERE UD.UsuarioId = @Id;";

            using (var multipleResultSets = _connection.QueryMultiple(sql, new { Id = id }))
            {
                var usuario = multipleResultSets.Read<Usuario>().SingleOrDefault();
                var contato = multipleResultSets.Read<Contato>().SingleOrDefault();
                var enderecos = multipleResultSets.Read<EnderecoEntrega>().ToList();
                var departamentos = multipleResultSets.Read<Departamento>().ToList();

                if (usuario != null)
                {
                    usuario.Contato = contato;
                    usuario.EnderecosEntrega = enderecos;
                    usuario.Departamentos = departamentos;

                    return Ok(usuario);
                }
            }

            return NotFound();
        }

        [HttpGet("stored/usuarios")]
        public IActionResult StoredGetAll()
        {
            var usuarios = _connection.Query<Usuario>("SelecionarUsuarios", commandType: CommandType.StoredProcedure).ToList();

            return Ok(usuarios);
        }

        [HttpGet("stored/usuarios/{id}")]
        public IActionResult StoredGet(int id)
        {
            var usuario = _connection.Query<Usuario>("SelecionarUsuario", new { Id = id }, commandType: CommandType.StoredProcedure).SingleOrDefault();

            return Ok(usuario);
        }

        [HttpGet("mapper1/usuarios")]
        public IActionResult Mapper1()
        {
            // Problema: O objeto "SegundoUsuario" possui propriedades com nomes diferentes aos da tabela "Usuarios"
            // Solução 1: Renomear as colunas pela query SQL.
            string sqlRenomeado = "SELECT Id Cod, Nome NomeCompleto, Email, Sexo, RG, CPF, NomeMae NomeCompletoMae, SituacaoCadastro Situacao, DataCadastro FROM [Usuarios]";
            
            var usuariosRenomeado = _connection.Query<SegundoUsuario>(sqlRenomeado).ToList();

            // var usuarios = _connection.Query<SegundoUsuario>("SELECT * FROM [Usuarios]").ToList();

            return Ok(usuariosRenomeado);
        }

        [HttpGet("mapper2/usuarios")]
        public IActionResult Mapper2()
        {
            // Problema: O objeto "SegundoUsuario" possui propriedades com nomes diferentes aos da tabela "Usuarios"
            // Solução 2: Via C#(POO) - Mapear as colunas por meio da biblioteca Dapper.FluentMap com nossa classe na pasta "Mapper"

            // Configuração do Mapper feita no "Program":
            // FluentMapper.Initialize(config => { config.AddMap(new SegundoUsuarioMap()); });

            var usuarios = _connection.Query<SegundoUsuario>("SELECT * FROM [Usuarios]").ToList();

            return Ok(usuarios);
        }
    }
}
