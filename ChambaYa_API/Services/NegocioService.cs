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
        Usuario? ValidarUsuario(string email, string clave);
        bool RegistrarUsuario(RegistroRequest u);
        
        List<MisPostulacionesDTO> ListarMisPostulaciones(int idUsuario);
        List<MisOfertasAdminDTO> ListarMisOfertasAdmin(int idUsuario);
        List<CandidatoDTO> ListarCandidatosPorOferta(int idOferta);
        bool EvaluarPostulante(EvaluarRequest req);
        bool ActualizarOferta(Oferta oferta);
        bool EliminarOferta(int idOferta);
        Oferta? ObtenerOferta(int idOferta);
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
                            FechaPublicacion = dr.GetDateTime(8),
                            IdCategoria = dr.FieldCount > 9 ? dr.GetInt32(9) : 0
                        });
                    }
                }
            }
            return lista;
        }

        public bool RegistrarUsuario(RegistroRequest u)
        {
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                string query = @"INSERT INTO Usuario (IdRol, Nombres, Email, Clave, Activo) 
                                 VALUES (@Rol, @Nombres, @Email, @Clave, 1)";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@Rol", 2);
                cmd.Parameters.AddWithValue("@Nombres", u.Nombres);
                cmd.Parameters.AddWithValue("@Email", u.Email);
                cmd.Parameters.AddWithValue("@Clave", u.Clave);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
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
                    cmd.Parameters.AddWithValue("@IdUsuario", 1); 
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
                    SqlCommand cmd = new SqlCommand("sp_listar_categorias", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
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
                }
            }
            return lista;
        }

        public Usuario? ValidarUsuario(string email, string clave)
        {
            Usuario? usuario = null;
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                try
                {
                    string query = @"SELECT U.IdUsuario, U.IdRol, R.NombreRol, U.Nombres, U.Email, U.Activo 
                                     FROM Usuario U
                                     INNER JOIN Rol R ON U.IdRol = R.IdRol
                                     WHERE U.Email = @Email AND U.Clave = @Clave AND U.Activo = 1";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Clave", clave);

                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            usuario = new Usuario
                            {
                                IdUsuario = dr.GetInt32(0),
                                IdRol = dr.GetInt32(1),
                                NombreRol = dr.GetString(2),
                                Nombres = dr.GetString(3),
                                Email = dr.GetString(4),
                                Activo = dr.GetBoolean(5)
                            };
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return usuario;
        }

        public List<MisPostulacionesDTO> ListarMisPostulaciones(int idUsuario)
        {
            var lista = new List<MisPostulacionesDTO>();
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_mis_postulaciones", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new MisPostulacionesDTO
                        {
                            Titulo = dr.GetString(0),
                            NombreCategoria = dr.GetString(1),
                            Ubicacion = dr.GetString(2),
                            Salario = dr.GetDecimal(3),
                            FechaPostulacion = dr.GetDateTime(4),
                            Estado = dr.GetString(5),
                            MensajeRespuesta = dr.IsDBNull(6) ? null : dr.GetString(6)
                        });
                    }
                }
            }
            return lista;
        }

        public List<MisOfertasAdminDTO> ListarMisOfertasAdmin(int idUsuario)
        {
            var lista = new List<MisOfertasAdminDTO>();
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_listar_mis_ofertas_admin", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new MisOfertasAdminDTO
                        {
                            IdOferta = dr.GetInt32(0),
                            Titulo = dr.GetString(1),
                            FechaPublicacion = dr.GetDateTime(2),
                            Activa = dr.GetBoolean(3),
                            TotalPostulantes = dr.GetInt32(4)
                        });
                    }
                }
            }
            return lista;
        }

        public List<CandidatoDTO> ListarCandidatosPorOferta(int idOferta)
        {
            var lista = new List<CandidatoDTO>();
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_listar_postulantes_por_oferta", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdOferta", idOferta);
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new CandidatoDTO
                        {
                            IdDetalle = dr.GetInt32(0),
                            Nombres = dr.GetString(1),
                            Email = dr.GetString(2),
                            FechaPostulacion = dr.GetDateTime(3),
                            Estado = dr.GetString(4),
                            MensajeRespuesta = dr.IsDBNull(5) ? null : dr.GetString(5)
                        });
                    }
                }
            }
            return lista;
        }

        public bool EvaluarPostulante(EvaluarRequest req)
        {
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_evaluar_postulante", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdDetalle", req.IdDetalle);
                cmd.Parameters.AddWithValue("@Estado", req.Estado);
                cmd.Parameters.AddWithValue("@Mensaje", req.Mensaje ?? (object)DBNull.Value);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool ActualizarOferta(Oferta oferta)
        {
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_actualizar_oferta", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdOferta", oferta.IdOferta);
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
        }

        public bool EliminarOferta(int idOferta)
        {
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_eliminar_oferta", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdOferta", idOferta);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public Oferta? ObtenerOferta(int idOferta)
        {
            Oferta? oferta = null;
            using (SqlConnection cn = new SqlConnection(_cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_obtener_oferta", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdOferta", idOferta);
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        oferta = new Oferta
                        {
                            IdOferta = dr.GetInt32(0),
                            IdCategoria = dr.GetInt32(1),
                            Titulo = dr.GetString(3),
                            Descripcion = dr.GetString(4),
                            Salario = dr.GetDecimal(5),
                            Ubicacion = dr.GetString(6),
                            Modalidad = dr.GetString(7),
                            Requisitos = dr.GetString(8),
                            FechaPublicacion = dr.GetDateTime(9)
                        };
                    }
                }
            }
            return oferta;
        }
    }
}

