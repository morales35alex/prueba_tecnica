using System;
using System.Collections.Generic;

namespace prueba_tecnica.Models;

public partial class TipoMovimiento
{
    public int IdTipoMovimiento { get; set; }

    public string? Tipo { get; set; }

    public int? Estado { get; set; }

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
