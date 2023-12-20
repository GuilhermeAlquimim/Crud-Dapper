using Dapper.FluentMap.Mapping;
using eCommerce.API.Models;

namespace eCommerce.API.Mappers
{
    public class SegundoUsuarioMap : EntityMap<SegundoUsuario>
    {
        public SegundoUsuarioMap()
        {
            Map(x => x.Cod).ToColumn("Id");
            Map(x => x.NomeCompleto).ToColumn("Nome");
            Map(x => x.NomeCompletoMae).ToColumn("NomeMae");
            Map(x => x.Situacao).ToColumn("SituacaoCadastro");
        }
    }
}
