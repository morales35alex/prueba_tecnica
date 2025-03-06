using System;
using System.Collections.Generic;

namespace prueba_tecnica.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string? Nombre { get; set; }

    public string? Dni { get; set; }

    public string? Correo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? Estado { get; set; }

    public virtual ICollection<Cuentum> Cuenta { get; set; } = new List<Cuentum>();
}
