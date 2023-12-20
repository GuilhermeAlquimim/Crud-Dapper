namespace eCommerce.API.Models
{
    public class Departamento
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public ICollection<Departamento>? Usuarios { get; set; }
    }
}
