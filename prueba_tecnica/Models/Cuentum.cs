using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace prueba_tecnica.Models;

public partial class Cuentum
{
    public int IdCuenta { get; set; }

    public int? IdCliente { get; set; }

    public int? NumeroCuenta { get; set; }

    public DateTime? FechaCreacion { get; set; }

    [JsonIgnore]
    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
