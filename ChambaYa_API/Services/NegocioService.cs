using ChambaYa_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ChambaYa_API.Services
{
    public interface INegocioService
    {
        List<Oferta> ListarOfertasActivas();
        bool ProcesarCarritoPostulacion(PostulacionCarrito carrito);
        bool CrearOferta(Oferta oferta);
        List<Categoria> ListarCategorias();
    }

    public class NegocioService : INegocioService
    {
        private readonly string _cadena;

        public NegocioService(IConfiguration config)
        {
            _cadena = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public List<Oferta> ListarOfertasActivas()
        {
            var lista = new List<Oferta>();
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_listar_ofertas_activas", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Oferta
                        {
                            IdOferta = dr.GetInt32(0),
                            NombreCategoria = dr.GetString(1),
                            Titulo = dr.GetString(2),
                            Descripcion = dr.GetString(3),
                            Salario = dr.GetDecimal(4),
                            Ubicacion = dr.GetString(5),
                            Modalidad = dr.GetString(6),
                            Requisitos = dr.GetString(7),
                            FechaPublicacion = dr.GetDateTime(8)
                        });
                    }
                }
            }
            return lista;
        }

        public bool ProcesarCarritoPostulacion(PostulacionCarrito carrito)
        {
            bool exito = false;
            string ofertasIds = string.Join(",", carrito.OfertasSeleccionadas);

            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_registrar_postulacion_masiva", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdUsuario", carrito.IdUsuario);
                    cmd.Parameters.AddWithValue("@OfertasIds", ofertasIds);

                    cn.Open();
                    int filas = cmd.ExecuteNonQuery();
                    exito = filas > 0;
                }
                catch (Exception)
                {
                    exito = false;
                }
            }
            return exito;
        }

        public bool CrearOferta(Oferta oferta)
        {
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_insertar_oferta", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdCategoria", oferta.IdCategoria);
                    cmd.Parameters.AddWithValue("@Titulo", oferta.Titulo);
                    cmd.Parameters.AddWithValue("@Descripcion", oferta.Descripcion);
                    cmd.Parameters.AddWithValue("@Salario", oferta.Salario);
                    cmd.Parameters.AddWithValue("@Ubicacion", oferta.Ubicacion);
                    cmd.Parameters.AddWithValue("@Modalidad", oferta.Modalidad);
                    cmd.Parameters.AddWithValue("@Requisitos", oferta.Requisitos);

                    cn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public List<Categoria> ListarCategorias()
        {
            var lista = new List<Categoria>();
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SELECT IdCategoria, NombreCategoria FROM Categorias", cn);
                    cmd.CommandType = CommandType.Text;
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Categoria
                            {
                                IdCategoria = dr.GetInt32(0),
                                NombreCategoria = dr.GetString(1)
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    try 
                    {
                        SqlCommand cmd = new SqlCommand("SELECT IdCategoria, NombreCategoria FROM Categoria", cn);
                        cmd.CommandType = CommandType.Text;
                        if (cn.State != ConnectionState.Open) cn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                lista.Add(new Categoria
                                {
                                    IdCategoria = dr.GetInt32(0),
                                    NombreCategoria = dr.GetString(1)
                                });
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            
            if (lista.Count == 0)
            {
                var ofertas = ListarOfertasActivas();
                lista = ofertas.Select(o => o.NombreCategoria)
                               .Where(n => !string.IsNullOrEmpty(n))
                               .Distinct()
                               .Select((n, i) => new Categoria { IdCategoria = i + 1, NombreCategoria = n })
                               .ToList();
            }
            
            return lista;
        }
    }
}
