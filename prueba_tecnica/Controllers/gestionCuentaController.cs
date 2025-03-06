using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using prueba_tecnica.Models;
using System.Reflection.Metadata.Ecma335;

namespace prueba_tecnica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class gestionCuentaController : ControllerBase
    {
        public readonly ModelsContext _context;

        private Random random = new Random();
        public gestionCuentaController (ModelsContext context)
        {
            _context = context;
        }

        [HttpPost("crearCuenta")]
        public async Task<ActionResult> crearCuenta( Cliente datos)
        {
            var transaction = _context.Database.BeginTransaction();
                if(datos == null)                
                    return BadRequest("No se encontraron datos");

            try
            {
            
                int cambio1 = 0;
                int cambio2 = 0;
                int cambio3 = 0;
                int cambio4 = 0;
                decimal saldoInicial=1000.00m;
                Cuentum cuenta = new Cuentum();
                

                if (validarDatos(datos.Dni, 1)) // Validar si no existe la cuenta
                        return BadRequest("El DNI ingresado ya existe verifique");

                    if (validarDatos(datos.Correo, 2)) // Validar si no existe la correo
                        return BadRequest("El correo ingresado ya existe verifique");

                    
                        var nuevoRegistro = new Cliente
                        {
                            Nombre= datos.Nombre,
                            Dni= datos.Dni,
                            Correo= datos.Correo,
                            Estado= datos.Estado,
                            FechaCreacion=DateTime.Now
                        };

                        await _context.Clientes.AddAsync(nuevoRegistro);
                        cambio1 = await _context.SaveChangesAsync();

                        // Creacion cuenta
                        cuenta.IdCliente = nuevoRegistro.IdCliente;
                        cuenta.NumeroCuenta = generarNumeroCuenta();
                        cuenta.FechaCreacion = DateTime.Now;
                
                        await _context.Cuenta.AddAsync(cuenta);
                        cambio2 = await _context.SaveChangesAsync();







                Movimiento movimiento =  movimientoCuentaInicial((int) cuenta.IdCuenta, saldoInicial,1);
                Historial historial =  historialCuenta(movimiento.IdMovimiento, "MOVIMIENTO DE CREACION DE CUENTA INICIAL");



                if (cambio1 > 0 && cambio2 > 0 && movimiento != null && historial!=null)
                {
                    await transaction.CommitAsync();
                    return Ok(new
                    {
                        ok = true,
                        cliente=nuevoRegistro,
                        cuenta=cuenta,
                        mensaje = "Registro agregado exitosamente!!"
                    });
                }else{
                    await transaction.RollbackAsync();
                    return Ok(new
                    {
                        ok = false,
                        mensaje = "Hubo un error al guardar el registro!!"
                    });

                }





            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Se encontro el siguiente error {ex}");
            }
        }

        private bool validarDatos(string parametro, int tipo)
        {
            
                switch (tipo)
                {
                    case 1: // validar DNI
                            return _context.Clientes.Any(x=>x.Dni==parametro);
                           
                    case 2: // Validar Correo
                            return _context.Clientes.Any(x => x.Correo == parametro);
                           
                    case 3:
                            if (int.TryParse(parametro, out int numeroCuenta))
                            {
                                return _context.Cuenta.Any(x => x.NumeroCuenta == numeroCuenta);
                            }
                             return false; // Si el parámetro no es numérico, retornar false.
                default:
                    return false;
                
                }

            }

        private int generarNumeroCuenta()
        {
            string numeroCuenta;
            bool existe;

            do
            {
                // Generar un número de cuenta aleatorio de 10 dígitos
                numeroCuenta = random.Next(1000000000, int.MaxValue).ToString();

                // Verificar si ya existe en la base de datos
                existe = _context.Cuenta.Any(x => x.NumeroCuenta == int.Parse(numeroCuenta));

            } while (existe); // Repetir hasta que se genere un número único

            return int.Parse(numeroCuenta);
        }

        [HttpGet("consultarCuenta")]
        public async Task<ActionResult> consultarCuenta(int cuenta)
        {
            try
            {
                var saldo = await (from c in _context.Cuenta
                             join m in _context.Movimientos on c.IdCuenta equals m.IdCuenta
                             where c.NumeroCuenta==cuenta
                             orderby m.FechaMovimiento descending

                             select new
                             {
                                 m.Saldo
                             }
                             ).FirstOrDefaultAsync();
                if(saldo != null)
                {
                    return Ok(new { 
                        ok=true,
                        saldo=saldo.Saldo
                    });
                }
                else
                {
                    return Ok(new
                    {
                        ok = false,
                        mensaje = "No existen movimientos en esta cuenta"
                    });

                }

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Se encontro el siguiente error {ex}");
            }
        }

        [HttpPost("depositosRetiros")]
        public async Task<ActionResult> depositosRetiros( int cuenta, decimal valor, string? detalle, int tipoTransaccion)
        {
            
            try
            {
                switch (tipoTransaccion)
                {
                    case 1:
                        return Ok(deposito(cuenta, valor, detalle, tipoTransaccion));

                    case 2:
                        return Ok( retiro(cuenta, valor, detalle,tipoTransaccion) );                      

                   
                }

                return BadRequest("Surgio un erro");
                                           
              

            }
            catch (Exception ex)
            {
              

                return StatusCode(500, $"Se encontro el siguiente error {ex}");
            }


        }

        [HttpGet("saldoTransacciones")]
        public async Task<ActionResult> saldoTransacciones(int cuenta)
        {
            try
            {
                var historial = await (from c in _context.Cuenta
                                   join m in _context.Movimientos on c.IdCuenta equals m.IdCuenta
                                   join h in _context.Historials on m.IdMovimiento  equals h.IdMovimiento
                                   join tip in _context.TipoMovimientos on m.IdTipoMovimiento equals tip.IdTipoMovimiento
                                   where c.NumeroCuenta == cuenta
                                   orderby m.FechaMovimiento descending

                                   select new
                                   {
                                       m.FechaMovimiento,
                                       valor_movimiento=tip.IdTipoMovimiento==1?m.Deposito:m.Retiro,
                                       tipo_movimiento=tip.Tipo,
                                       h.Detalle
                                   }
                             ).ToListAsync();
                var saldo = ultimoMovimiento(cuenta);
                if (historial != null)
                {
                    return Ok(new
                    {
                        ok = true,
                        saldo = saldo.Saldo,
                        transacciones=historial
                    });
                }
                else
                {
                    return Ok(new
                    {
                        ok = false,
                        mensaje = "No existen movimientos en esta cuenta"
                    });

                }

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Se encontro el siguiente error {ex}");
            }
        }

        private object retiro(int cuenta, decimal valor, string detalle,int tipoTransaccion)
        {           

            Cuentum? cuentaVerif = _context.Cuenta.FirstOrDefault(x=>x.NumeroCuenta == cuenta);
            
            if (cuentaVerif != null)
            {
                var saldo = ultimoMovimiento((int)cuentaVerif.NumeroCuenta);
                if (saldo.Saldo>=valor)
                {
                        var retiro = movimientoCuenta((int)cuentaVerif.NumeroCuenta, valor,tipoTransaccion );
                    if (retiro != null)
                    {
                        var historial = historialCuenta(retiro.IdMovimiento, detalle);
                        if (historial != null)
                        {
                            return new {Ok=true, mensaje= "Retiro Exitoso"  , saldo = retiro.Saldo, Tipo_Movimiento = _context.TipoMovimientos.FirstOrDefault(x => x.IdTipoMovimiento == retiro.IdTipoMovimiento).Tipo };
                        }
                        else
                        {
                            return new { Ok = false, mensaje = "Error al generar el historial" };
                        }
                    }
                    else
                    {
                        return new { Ok = false, mensaje = "Error al generar el movimiento" };
                    }

                }
                else
                {
                     return new { Ok = false, mensaje = "El valor ha retirar excede el saldo, no hay fondo suficientes" };
                }
               

            }
            else
            {
                return new { Ok = true, mensaje = "No se encontro registro con la cuenta ingresada" };
            }
        }

        private object deposito(int cuenta, decimal valor, string detalle, int tipoTransaccion)
        {

            Cuentum? cuentaVerif = _context.Cuenta.FirstOrDefault(x => x.NumeroCuenta == cuenta);

            if (cuentaVerif != null)
            {
                
                    var deposito = movimientoCuenta((int)cuentaVerif.NumeroCuenta, valor, tipoTransaccion);
                    if (deposito != null)
                    {
                        var historial =  historialCuenta(deposito.IdMovimiento, detalle);
                        if (historial != null)
                        {
                            return new { Ok = true, mensaje = "Deposito Exitoso", saldo=deposito.Saldo, Tipo_Movimiento=_context.TipoMovimientos.FirstOrDefault(x=>x.IdTipoMovimiento==deposito.IdTipoMovimiento).Tipo };
                        }
                        else
                        {
                            return new { Ok = false, mensaje = "Error al generar el historial" };
                        }
                    }
                    else
                    {
                        return new { Ok = false, mensaje = "Error al generar el movimiento" };
                    }

               


            }
            else
            {
                return new { Ok = true, mensaje = "El numero de cuenta no existe verifique" };
            }
        }

        private Movimiento movimientoCuenta(int cuenta, decimal valor, int tipoTransaccion)
        {
          
            Movimiento? ultimoMovimientoVar = ultimoMovimiento(cuenta);

            // Determinar el saldo actual basado en el último movimiento
            

            Movimiento movimiento = new Movimiento();
            //creacion movimiento
            movimiento.IdCuenta = ultimoMovimientoVar.IdCuenta;
            movimiento.Deposito = tipoTransaccion==1?valor:null;
            movimiento.Retiro= tipoTransaccion == 2 ? valor : null;
            movimiento.Saldo = calcularSaldo((decimal)ultimoMovimientoVar.Saldo,valor,tipoTransaccion);
            movimiento.FechaMovimiento = DateTime.Now;
            movimiento.IdTipoMovimiento = tipoTransaccion;
            movimiento.Estado = 1;//Activo
              _context.Movimientos.Add(movimiento);
               _context.SaveChanges();
            return movimiento;

        }

        
        private decimal calcularSaldo(decimal ultimoMovimiento,decimal valor, int tipoTransaccion)
        {    

            return tipoTransaccion == 1 ? ultimoMovimiento + valor : ultimoMovimiento - valor;


        }

        private Historial historialCuenta(int idMovimiento, string detalle)
        {
            Historial historial = new Historial();
            //Historial
            historial.IdMovimiento = idMovimiento;
            historial.Detalle = detalle;
            historial.Estado = 1;// activo u otro estado
            historial.FechaMovimiento = DateTime.Now;
             _context.Historials.Add(historial);
             _context.SaveChanges();
            return historial;

        }

        private Movimiento? ultimoMovimiento(int  numeroCuenta) 
        {
            var cuenta = _context.Cuenta.FirstOrDefault(x => x.NumeroCuenta == numeroCuenta);
            return  _context.Movimientos
                                       .Where(m => m.IdCuenta == cuenta.IdCuenta)
                                       .OrderByDescending(m => m.FechaMovimiento)
                                       .FirstOrDefault();
        }

        private Movimiento movimientoCuentaInicial(int idCuenta, decimal valor, int tipoTransaccion)
        {

         

           
            Movimiento movimiento = new Movimiento();
            //creacion movimiento
            movimiento.IdCuenta = idCuenta;
            movimiento.Deposito = tipoTransaccion == 1 ? valor : null;
            movimiento.Retiro = tipoTransaccion == 2 ? valor : null;
            movimiento.Saldo =  valor;
            movimiento.FechaMovimiento = DateTime.Now;
            movimiento.IdTipoMovimiento = tipoTransaccion;
            movimiento.Estado = 1;//Activo
            _context.Movimientos.Add(movimiento);
            _context.SaveChanges();
            return movimiento;

        }





    }
}
