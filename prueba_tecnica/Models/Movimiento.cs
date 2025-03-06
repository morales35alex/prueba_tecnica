using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace prueba_tecnica.Models;

public partial class Movimiento
{
    public int IdMovimiento { get; set; }

    public int? IdCuenta { get; set; }

    public decimal? Retiro { get; set; }

    public decimal? Deposito { get; set; }

    public decimal? Saldo { get; set; }

    public DateTime? FechaMovimiento { get; set; }

    public int? Estado { get; set; }

    public int? IdTipoMovimiento { get; set; }

    public virtual ICollection<Historial> Historials { get; set; } = new List<Historial>();
    [JsonIgnore]
    public virtual Cuentum? IdCuentaNavigation { get; set; }

    [JsonIgnore]
    public virtual TipoMovimiento? IdTipoMovimientoNavigation { get; set; }
}
