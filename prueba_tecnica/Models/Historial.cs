using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace prueba_tecnica.Models;

public partial class Historial
{
    public int IdHistorial { get; set; }

    public int? IdMovimiento { get; set; }

    public string? Detalle { get; set; }

    public DateTime? FechaMovimiento { get; set; }

    public int? Estado { get; set; }
    [JsonIgnore]
    public virtual Movimiento? IdMovimientoNavigation { get; set; }
}
