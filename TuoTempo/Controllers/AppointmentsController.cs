using FirebirdSql.Data.FirebirdClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using TuoTempo.Models;
using Newtonsoft.Json;
using System.Text;
using System.Collections;
using System.Net;
using RadioWeb.Models.Repos;
using System.Globalization;
using System.Data;
using System.Data.Odbc;
using System.Resources;
using System.Web.Http.Results;
using TuoTempo.Models.Repos;
using Newtonsoft.Json.Linq;

namespace TuoTempo.Controllers
{

    //      a.Documentación técnica: https://postman.tuotempo.com/
    //      b.Notion: https://www.notion.so/tuotempo-enterprise/Guia-de-Integraci-n-TuoTempo-ES-bb77b8c05b5744b7ad4647228d3afe04

    [BasicAuthentication]
    public class AppointmentsController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString; // Ajusta esto según tu conexión a Firebird

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DateTime startTime = DateTime.UtcNow;

        public class MyResponse
        {
            public string result { get; set; }

            [JsonProperty("return")]
            public object returnObject { get; set; }
        }

        private string RemoveDiacritics(string input)
        {
            string stFormD = input.Normalize(NormalizationForm.FormD);
            int len = stFormD.Length;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[i]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[i]);
                }
            }
            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }
        private string QuotedString(string valor)
        {

            string retorno = "";
            if (!String.IsNullOrEmpty(valor))
            {
                retorno = "'" + valor.Replace("'", "''") + "'";
            }
            else
            {
                retorno = "''";
            }

            return retorno;


        }

        private Int64 UpsertPatient(AppointmentRequest appointmentRequest, string cleanedIdNumber)
        {
            Int64 oidPaciente = -1;
            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();

                // Construir la consulta SQL de actualización
                string updateQuery = @"
                UPDATE PACIENTE
                SET USERNAME='TUOTEMPO',
                CID = @insuranceId,
                PACIENTE = @patientName,
                COMENTARIO = 'EDITADO DESDE TUOTEMPO',
                FECHAN= @birthdate,
                SEXO = @gender,                               
                DNI = @idNumber,
                OTROS1='F',
                TRAC=@TRAC,
                EMAIL = @email
                WHERE ";

                if (appointmentRequest.user.user_lid != null && !string.IsNullOrEmpty( appointmentRequest.user.user_lid))
                {
                    updateQuery += " oid= @OID RETURNING OID ";
                }
                else if (cleanedIdNumber.Length>12)
                {
                    updateQuery = updateQuery.Replace("DNI", "CIP");
                    updateQuery += " upper(CIP) = @idNumber and FECHAN=@birthdate  RETURNING OID ";
                }
                else
                {
                    updateQuery += " upper(DNI) = @idNumber and FECHAN=@birthdate  RETURNING OID ";
                }

             


                string sexo = (appointmentRequest.user.gender == "F") ? "M" : "H";
                string trac = (appointmentRequest.user.gender == "F") ? "5" : "1";
                DateTime fechaNacimiento;
                string cadenaFechaN = "";
                if (DateTime.TryParse(appointmentRequest.user.birthdate.ToString(), out fechaNacimiento))
                {
                    cadenaFechaN = fechaNacimiento.ToString("yyyy-MM-dd");
                }
                using (var command = new FbCommand(updateQuery, connection))
                {
                    if (appointmentRequest.availability.insurance_lid==null)
                    {
                        appointmentRequest.availability.insurance_lid = "3820159";
                    }
                    command.Parameters.AddWithValue("@insuranceId", appointmentRequest.availability.insurance_lid);
                    command.Parameters.AddWithValue("@patientName", $"{RemoveDiacritics(appointmentRequest.user.second_name.ToUpper())}, {RemoveDiacritics(appointmentRequest.user.first_name.ToUpper())}");
                    command.Parameters.AddWithValue("@email", appointmentRequest.user.contact.email);
                    command.Parameters.AddWithValue("@TRAC", trac);
                    command.Parameters.AddWithValue("@gender", sexo);  
                    command.Parameters.AddWithValue("@idNumber", cleanedIdNumber);
                    command.Parameters.AddWithValue("@birthdate", cadenaFechaN);
                    if (appointmentRequest.user.user_lid != null && !string.IsNullOrEmpty(appointmentRequest.user.user_lid))
                    {
                        command.Parameters.AddWithValue("@OID", appointmentRequest.user.user_lid);
                    }
                   


                    try
                    {

                        oidPaciente = (int)command.ExecuteScalar();
                    }
                    catch (Exception ex)
                    {
                        // Manejar excepciones según sea necesario

                    }

                    if (oidPaciente == -1)
                    {
                        string insertQuery = "";
                        if (cleanedIdNumber.Length > 12)
                        {
                            insertQuery = @"
                            INSERT INTO PACIENTE (COD_PAC, CID, PACIENTE, FECHAN, SEXO, USERNAME, AVISO, CIP, EMAIL,COMENTARIO)
                            VALUES (gen_id(GENCODPAC, 1), @insuranceId, @patientName, @birthdate, @gender, 'TUOTEMPO', 'F', @idNumber, @email,'INSERTADO DESDE TUOTEMPO')
                            RETURNING OID;";
                        }
                        else
                        {
                            insertQuery = @"
                            INSERT INTO PACIENTE (COD_PAC, CID, PACIENTE, FECHAN, SEXO, USERNAME, AVISO, DNI, EMAIL,COMENTARIO)
                            VALUES (gen_id(GENCODPAC, 1), @insuranceId, @patientName, @birthdate, @gender, 'TUOTEMPO', 'F', @idNumber, @email,'INSERTADO DESDE TUOTEMPO')
                            RETURNING OID;";
                        }
                        // No se encontró ningún registro que cumpla con las condiciones,
                        // por lo tanto, realizar una inserción
                     

                        using (var insertCommand = new FbCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@insuranceId", appointmentRequest.availability.insurance_lid);
                            insertCommand.Parameters.AddWithValue("@patientName", $"{RemoveDiacritics(appointmentRequest.user.second_name.ToUpper())}, {RemoveDiacritics(appointmentRequest.user.first_name.ToUpper())}");
                            insertCommand.Parameters.AddWithValue("@birthdate", cadenaFechaN);
                            insertCommand.Parameters.AddWithValue("@gender", sexo);
                            insertCommand.Parameters.AddWithValue("@idNumber", cleanedIdNumber);
                            insertCommand.Parameters.AddWithValue("@email", appointmentRequest.user.contact.email);
                            insertCommand.Parameters.AddWithValue("@TRAC", trac);

                         

                            try
                            {

                                oidPaciente = (int)insertCommand.ExecuteScalar();
                            }
                            catch (Exception ex)
                            {
                                // Manejar excepciones según sea necesario

                            }

                        }

                    }
                }
                // Retornar el OID del paciente actualizado o insertado
                return oidPaciente;
            }
        }
        private void LogError(Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            logger.Error(ex, $"Error después de {duration.TotalMilliseconds} ms en /api/appointment - POST. Detalle: {ex.Message}");
        }

        private void LogRequestInfo(string userId)
        {
            var startTime = DateTime.UtcNow;
            var clientIp = HttpContext.Current?.Request?.UserHostAddress;

            // Registrar información de la solicitud
            logger.Info($"Inicio de solicitud: {startTime}. IP del cliente: {clientIp}, Usuario: {userId}, Endpoint: /api/appointment - POST");
        }

        private string CleanIdNumber(string idNumber)
        {
            // Validar y limpiar el número de identificación
            return idNumber?.ToUpper()?.Replace(" ", "")?.Replace("-", "");
        }

        public void ProcesarDireccion(Int64 pacienteId, DIRECCION nuevaDireccion)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            using (FbConnection oConexion = new FbConnection(connectionString))
            {
                oConexion.Open();
                using (FbTransaction transaction = oConexion.BeginTransaction())
                {
                    try
                    {
                        // Paso 1: Borrar todas las direcciones del paciente con OWNER igual al ID del paciente y username igual a "tuotempo"
                        string deleteDireccionQuery = "DELETE FROM DIRECCION WHERE OWNER = @PacienteId AND username = 'TUOTEMPO'";
                        using (FbCommand deleteCommand = new FbCommand(deleteDireccionQuery, oConexion, transaction))
                        {
                            deleteCommand.Parameters.Add(new FbParameter("@PacienteId", pacienteId));
                            deleteCommand.ExecuteNonQuery();
                        }

                        // Paso 2: Insertar la nueva dirección basada en el JSON
                        string insertDireccionQuery = "INSERT INTO DIRECCION (OID, OWNER, DIRECCION, CP, POBLACION, PROVINCIA, PAIS, IOR_TIPO, username) VALUES (gen_id(GENUID, 1), ";
                        insertDireccionQuery += "@PacienteId, @Direccion, @CP, @Poblacion, @Provincia, @Pais, 0, 'TUOTEMPO')";

                        using (FbCommand insertCommand = new FbCommand(insertDireccionQuery, oConexion, transaction))
                        {
                            insertCommand.Parameters.Add(new FbParameter("@PacienteId", pacienteId));
                            insertCommand.Parameters.Add(new FbParameter("@Direccion", nuevaDireccion.DIRECCION1));
                            insertCommand.Parameters.Add(new FbParameter("@CP", nuevaDireccion.CP));
                            insertCommand.Parameters.Add(new FbParameter("@Poblacion", nuevaDireccion.POBLACION));
                            insertCommand.Parameters.Add(new FbParameter("@Provincia", nuevaDireccion.PROVINCIA));
                            insertCommand.Parameters.Add(new FbParameter("@Pais", nuevaDireccion.PAIS));

                            insertCommand.ExecuteNonQuery();
                        }

                        // Confirmar la transacción
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // En caso de error, realizar un rollback de la transacción
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void ProcesarTelefono(Int64 ownerId, TELEFONO nuevoTelefono)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            using (FbConnection oConexion = new FbConnection(connectionString))
            {
                oConexion.Open();
                using (FbTransaction transaction = oConexion.BeginTransaction())
                {
                    try
                    {
                        // Paso 1: Borrar todos los teléfonos del propietario y con username igual a "TUOTEMPO"
                        string deleteTelefonoQuery = "DELETE FROM TELEFONO WHERE LOCALIZACION=@LOCALIZACION and OWNER = @OwnerId AND username = 'TUOTEMPO'";
                        using (FbCommand deleteCommand = new FbCommand(deleteTelefonoQuery, oConexion, transaction))
                        {
                            deleteCommand.Parameters.Add(new FbParameter("@OwnerId", ownerId));
                            deleteCommand.Parameters.Add(new FbParameter("@LOCALIZACION", nuevoTelefono.LOCALIZACION));
                            deleteCommand.ExecuteNonQuery();
                        }

                        // Paso 2: Insertar la nueva dirección basada en el JSON
                        string InsertTelefono = "INSERT INTO TELEFONO (OID, USERNAME,OWNER, numero, LOCALIZACION, IOR_TIPO) VALUES (gen_id(GENUID, 1),'TUOTEMPO', @OWNER, @NUMERO, @LOCALIZACION, @IOR_TIPO) RETURNING OID";



                        using (FbCommand insertCommand = new FbCommand(InsertTelefono, oConexion, transaction))
                        {
                            insertCommand.Parameters.Add(new FbParameter("@OWNER", ownerId));
                            insertCommand.Parameters.Add(new FbParameter("@NUMERO", nuevoTelefono.NUMERO));
                            insertCommand.Parameters.Add(new FbParameter("@LOCALIZACION", nuevoTelefono.LOCALIZACION));
                            insertCommand.Parameters.Add(new FbParameter("@IOR_TIPO", nuevoTelefono.IOR_TIPO));


                            insertCommand.ExecuteNonQuery();
                        }


                        // Confirmar la transacción
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // En caso de error, realizar un rollback de la transacción
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("tuotempo/appointments")]
        public IHttpActionResult AddApointment([FromBody] AppointmentRequest oPaciente)
        {

            logger.Info("Solicitud de AddApointment recibida: {0}", JsonConvert.SerializeObject(oPaciente));

            try
            {
                DateTime fechaExplo;

                string cadenaFechaExploracoin = "";
                string idHueco = oPaciente.availability.availability_lid;
                string[] parts = idHueco.Split('_');
                string ior_aparatoHorario = "";// parts[parts.Length - 2];
                string ior_aparato = ""; // parts[parts.Length - 1];
                if (parts.Length >= 3)
                {
                    // Obtiene los dos últimos números
                    ior_aparatoHorario = parts[parts.Length - 2];
                    ior_aparato = parts[parts.Length - 1];

                }

                if (DateTime.TryParse(oPaciente.availability.date, out fechaExplo))
                {
                    cadenaFechaExploracoin = fechaExplo.ToString("yyyy-MM-dd");
                }
                // Validar la entrada
                if (oPaciente == null)
                {
                    return BadRequest("La solicitud no contiene datos válidos.");
                }
                // AQUÍ: Ejecutar la consulta para verificar si ya existe una cita en la fecha, hora y aparato especificados
                using (FbConnection conn = new FbConnection(connectionString))
                {
                    conn.Open();

                    string checkCommand = @"
                SELECT e.IOR_PACIENTE
                FROM EXPLORACION e
                WHERE e.ESTADO<>1 AND e.FECHA=@FECHA AND e.HORA=@HORA AND e.IOR_APARATO=@IOR_APARATO";

                    using (FbCommand cmdCheck = new FbCommand(checkCommand, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@FECHA", cadenaFechaExploracoin);
                        cmdCheck.Parameters.AddWithValue("@HORA", oPaciente.availability.start_time);
                        cmdCheck.Parameters.AddWithValue("@IOR_APARATO", ior_aparato);

                        using (var reader = cmdCheck.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ErrorResponse oError = new ErrorResponse();
                                oError.Result = "ERROR";
                                oError.ErrorCode = "APPOINTMENT_ALREADY_EXISTS";
                                oError.ErrorMessage = "Ya existe una cita en la fecha y hora especificadas.";
                                oError.DebugMessage = "Conflict with an existing appointment.";

                                // Si hay registros, devolver un error
                                return Content(HttpStatusCode.Conflict, oError);
                            }
                        }
                    }
                }

                // Obtener el usuario autenticado (si está disponible)
                var userId = User.Identity.IsAuthenticated ? User.Identity.Name : "Anónimo";

                // Registrar la solicitud
                LogRequestInfo(userId);



                // Validar y limpiar el número de identificación
                var cleanedIdNumber = CleanIdNumber(oPaciente.user.id_number.number);

                // Realizar la inserción o actualización en la base de datos
                var patientId = UpsertPatient(oPaciente, cleanedIdNumber);


                DIRECCION oDireccion = new DIRECCION();
                oDireccion.DIRECCION1 = oPaciente.user.address.street + " " + oPaciente.user.address.street_number;
                oDireccion.CP = oPaciente.user.address.zipcode;
                oDireccion.POBLACION = oPaciente.user.address.city;
                oDireccion.PROVINCIA = oPaciente.user.address.province;
                oDireccion.PAIS = oPaciente.user.address.country;

                ProcesarDireccion(patientId, oDireccion);

                TELEFONO oTelefono = new TELEFONO();
                oTelefono.NUMERO = oPaciente.user.contact.mobile;
                oTelefono.LOCALIZACION = "MOVIL";
                oTelefono.IOR_TIPO = 3664933;

                ProcesarTelefono(patientId, oTelefono);


                TELEFONO oTelefonoFijo = new TELEFONO();
                oTelefonoFijo.NUMERO = oPaciente.user.contact.landline;
                oTelefonoFijo.LOCALIZACION = "FIJO";
                oTelefonoFijo.IOR_TIPO = 3664932;


                ProcesarTelefono(patientId, oTelefonoFijo);


                EXPLORACION oExploNueva = new EXPLORACION();
                using (FbConnection conn = new FbConnection(connectionString))
                {
                    conn.Open();

                    string insertCommand = @"
                                INSERT INTO EXPLORACION 
                                (
                                    OID, VERS, OWNER, USERNAME, IOR_EMPRESA, FECHA, HORA, CANTIDAD, ESTADO, 
                                    FACTURADA, PAGADO, APLAZADO, INFORMADA, FECHA_IDEN, RECOGIDO, 
                                    IOR_GPR, IOR_ENTIDADPAGADORA, IOR_PACIENTE, IOR_APARATO, IOR_GRUPO, IOR_TIPOEXPLORACION, IOR_MONEDA,  PAGAR,BORRADO, INTOCABLE,HORA_IDEN,NOMODIFICA,NOFACTURAB,
                                    ENVIAR_MAIL,PERMISO,CANAL,IOR_CODIGORX,IDCITAONLINE
                                    
                                )
                                VALUES
                                (
                                    GEN_ID(GENUID,1), @VERS, @OWNER, @USERNAME, @IOR_EMPRESA, @FECHA,  @HORA, @CANTIDAD, @ESTADO, 
                                    @FACTURADA, @PAGADO, @APLAZADO, @INFORMADA,@FECHA_IDEN,  @RECOGIDO, 
                                    @IOR_GPR, @IOR_ENTIDADPAGADORA, @IOR_PACIENTE, @IOR_APARATO, @IOR_GRUPO, @IOR_TIPOEXPLORACION, @IOR_MONEDA,  @PAGAR,@BORRADO,@INTOCABLE,@HORA_IDEN, @NOMODIFICA,@NOFACTURAB,
                                    @ENVIAR_MAIL,@PERMISO,@CANAL,@IOR_CODIGORX,@IDCITAONLINE
                                )
                                RETURNING OID;";

                    string oPrecio="0";
                    string queryPrecio = "select  p.CANTIDAD FROM PRECIOS p  WHERE p.IOR_ENTIDADPAGADORA =" + oPaciente.availability.insurance_lid + " AND p.IOR_TIPOEXPLORACION=" + oPaciente.availability.activity_lid;
                    using (FbCommand oCommandParaPrecio = new FbCommand(queryPrecio, conn))
                    {
                        
                        if (oCommandParaPrecio.ExecuteScalar() != null)
                        {
                            oPrecio = oCommandParaPrecio.ExecuteScalar().ToString();
                        }
                        else
                        {
                            oPrecio = "";
                        }


                    }


                using (FbCommand cmd = new FbCommand(insertCommand, conn))
                    {
                        // Agregar los parámetros aquí
                        cmd.Parameters.AddWithValue("@VERS", 1);
                        cmd.Parameters.AddWithValue("@OWNER", oPaciente.availability.location_lid);
                        cmd.Parameters.AddWithValue("@USERNAME", "TUOTEMPO");
                        cmd.Parameters.AddWithValue("@IOR_EMPRESA", 4);
                        cmd.Parameters.AddWithValue("@HORA", oPaciente.availability.start_time);
                        cmd.Parameters.AddWithValue("@FECHA", cadenaFechaExploracoin);
                        // Reemplaza la coma por un punto
                        string precio = oPrecio.Replace(',', '.');

                        // Intenta convertir el precio a decimal
                        if (decimal.TryParse(precio, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cantidad))
                        {
                            cmd.Parameters.AddWithValue("@CANTIDAD", cantidad);
                        }
                        else
                        {
                            return BadRequest("Invalid price format.");
                        }
                       
                        cmd.Parameters.AddWithValue("@ESTADO", oExploNueva.ESTADO);
                        cmd.Parameters.AddWithValue("@FACTURADA", oExploNueva.FACTURADA);
                        cmd.Parameters.AddWithValue("@RECOGIDO", oExploNueva.RECOGIDO);
                        cmd.Parameters.AddWithValue("@PAGADO", "F");
                        cmd.Parameters.AddWithValue("@APLAZADO", oExploNueva.APLAZADO);
                        cmd.Parameters.AddWithValue("@INFORMADA", oExploNueva.INFORMADA);
                        cmd.Parameters.AddWithValue("@FECHA_IDEN", oExploNueva.FECHA_IDEN);
                        cmd.Parameters.AddWithValue("@HORA_IDEN", DateTime.Now.ToString("HH:mm"));
                        cmd.Parameters.AddWithValue("@INTOCABLE", oExploNueva.INTOCABLE);
                        cmd.Parameters.AddWithValue("@IOR_GPR", 2);
                        cmd.Parameters.AddWithValue("@IOR_ENTIDADPAGADORA", oPaciente.availability.insurance_lid);
                        cmd.Parameters.AddWithValue("@IOR_PACIENTE", patientId);
                        cmd.Parameters.AddWithValue("@IOR_APARATO", ior_aparato);
                        cmd.Parameters.AddWithValue("@IOR_GRUPO", oPaciente.availability.resource_lid);
                        cmd.Parameters.AddWithValue("@BORRADO", "F");
                        cmd.Parameters.AddWithValue("@IOR_CODIGORX", "-1");
                        cmd.Parameters.AddWithValue("@IOR_TIPOEXPLORACION", oPaciente.availability.activity_lid);
                        cmd.Parameters.AddWithValue("@IOR_MONEDA", 1386);
                        cmd.Parameters.AddWithValue("@PAGAR", "F");
                        cmd.Parameters.AddWithValue("@PERMISO", "T");
                        cmd.Parameters.AddWithValue("@CANAL", "1");
                        cmd.Parameters.AddWithValue("@NOMODIFICA", "F");
                        cmd.Parameters.AddWithValue("@NOFACTURAB", "F");
                        cmd.Parameters.AddWithValue("@ENVIAR_MAIL", "F");
                        cmd.Parameters.AddWithValue("@IDCITAONLINE", idHueco);

                        oPaciente.user.user_lid = patientId.ToString();
                        // Ejecutar el comando
                        oPaciente.app_lid = (int)cmd.ExecuteScalar();

                        LOGUSUARIOS oLog = new LOGUSUARIOS
                        {
                            OWNER = (int)oPaciente.app_lid,
                            FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            TEXTO = "Alta exploración",
                            USUARIO = "TUOTEMPO",
                            DATA = DateTime.Now.ToString("dd/MM/yyyy")

                        };
                        LogUsuariosRepositorio.Insertar(oLog);
                    }
                }

                // Crear una respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = oPaciente
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Manejar excepciones y registrar errores
                LogError(ex);
                return InternalServerError();
            }

        }


        //[System.Web.Http.HttpDelete]
        [System.Web.Http.HttpDelete]
        [System.Web.Http.Route("tuotempo/appointments/{id}")]
        public IHttpActionResult CancelAppointment(string id)
        {


            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new FbCommand("UPDATE EXPLORACION SET estado = '1', USERMOD = 'TUOTEMPO' WHERE oid = @IdExploracion", connection, transaction))
                    {
                        command.Parameters.Add("@IdExploracion", FbDbType.Integer).Value = id;

                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }

                connection.Close();
                LOGUSUARIOS oLog = new LOGUSUARIOS
                {
                    OWNER = int.Parse( id),
                    FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    TEXTO = "Cancela exploración",
                    USUARIO = "TUOTEMPO",
                    DATA = DateTime.Now.ToString("dd/MM/yyyy")

                };
                LogUsuariosRepositorio.Insertar(oLog);
            }

            var appointmentReturn = new AppointmentResponse();


            // Crea la conexión
            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();


                string query = @"
                    SELECT P.*
                    FROM CITAS_TUOTEMPO('0', '0', @oidexplo, null,null) p;";

                // Crea el comando
                using (var command = new FbCommand(query, connection))
                {
                    // Añade el parámetro @Oid al comando

                    command.Parameters.Add("@oidexplo", FbDbType.VarChar).Value = id;

                    // Ejecuta el comando y usa un FbDataReader para leer los resultados
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) // Si hay al menos un resultado
                        {
                            appointmentReturn = CreateAppointmentResponseFromReader(reader);


                        }
                    }
                }
                // Crear una respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = appointmentReturn
                };

                return Ok(response);
            }
            // Crea la conexión



        }


        //[System.Web.Http.HttpDelete]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("tuotempo/reschedule/")]
        public IHttpActionResult RescheduleAppointment([FromBody] AppointmentRequest oPaciente)
        {

            var appointmentReturn = new AppointmentResponse();

            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();

                // Primero, verificar si la exploración existe
                var checkExistenceCommand = new FbCommand("SELECT COUNT(*) FROM EXPLORACION WHERE OID = @OID", connection);
                checkExistenceCommand.Parameters.Add(new FbParameter("@OID", oPaciente.app_lid));

                int exists = (int)checkExistenceCommand.ExecuteScalar();

                if (exists == 0)
                {
                    ErrorResponse oError = new ErrorResponse();
                    oError.Result = "ERROR";
                    oError.ErrorCode = "APPOINTMENT_DOES_NOT_EXIST";
                    oError.ErrorMessage = "No existe la cita para replanificar.";
                    oError.DebugMessage = "Conflict with a non existing appointment.";

                    // Si hay registros, devolver un error
                    return Content(HttpStatusCode.Conflict, oError);
                   
                }

                using (var transaction = connection.BeginTransaction())
                {
                    var updateQuery = new StringBuilder("UPDATE EXPLORACION SET ");
                    var parameters = new List<FbParameter>();

                    // Añadir campos para actualizar, solo si no son nulos
                    if (oPaciente.availability != null)
                    {
                        if (!string.IsNullOrEmpty(oPaciente.availability.date))
                        {
                            DateTime fechaCita;
                            string fechaCitaN = "";
                            if (DateTime.TryParse(oPaciente.availability.date, out fechaCita))
                            {
                                fechaCitaN = fechaCita.ToString("yyyy-MM-dd");
                            }
                            updateQuery.Append("FECHA = @FechaCita, ");
                            parameters.Add(new FbParameter("@FechaCita", fechaCitaN));
                        }

                        if (!string.IsNullOrEmpty(oPaciente.availability.start_time))
                        {
                                        
                            updateQuery.Append("HORA = @hora, ");
                            parameters.Add(new FbParameter("@HORA", oPaciente.availability.start_time));
                        }

                        if (!string.IsNullOrEmpty(oPaciente.availability.location_lid))
                        {

                            updateQuery.Append("OWNER = @CENTRO, ");
                            parameters.Add(new FbParameter("@CENTRO", oPaciente.availability.location_lid));
                        }

                        if (!string.IsNullOrEmpty(oPaciente.availability.resource_lid))
                        {

                            updateQuery.Append("IOR_GRUPO = @GRUPO, ");
                            parameters.Add(new FbParameter("@GRUPO", oPaciente.availability.resource_lid));
                        }


                        if (!string.IsNullOrEmpty(oPaciente.availability.activity_lid))
                        {

                            updateQuery.Append("IOR_TIPOEXPLORACION = @IOR_TIPOEXPLORACION, ");
                            parameters.Add(new FbParameter("@IOR_TIPOEXPLORACION", oPaciente.availability.activity_lid));
                        }

                        if (!string.IsNullOrEmpty(oPaciente.availability.insurance_lid))
                        {

                            updateQuery.Append("IOR_ENTIDADPAGADORA = @IOR_ENTIDADPAGADORA, ");
                            parameters.Add(new FbParameter("@IOR_ENTIDADPAGADORA", oPaciente.availability.insurance_lid));
                        }

                    }



                    // Remover la última coma si hay parámetros
                    if (parameters.Count > 0)
                    {
                        updateQuery.Length -= 2; // Remueve la coma y el espacio extra
                        updateQuery.Append(" WHERE OID = @OID");
                        parameters.Add(new FbParameter("@OID", oPaciente.app_lid));

                        using (var command = new FbCommand(updateQuery.ToString(), connection, transaction))
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.Add(param);
                            }

                            command.ExecuteNonQuery();
                            transaction.Commit();

                            LOGUSUARIOS oLog = new LOGUSUARIOS
                            {
                                OWNER = (int) oPaciente.app_lid,
                                FECHA = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                                TEXTO = "Exploración replanificada",
                                USUARIO = "TUOTEMPO",
                                DATA = DateTime.Now.ToString("dd/MM/yyyy")
                             
                            };
                            LogUsuariosRepositorio.Insertar(oLog);

                        }
                    }
                    else
                    {
                        // Manejar el caso donde no hay campos para actualizar
                        transaction.Rollback();
                    }

                   

                  
                }

                string query = @"
                    SELECT P.*
                    FROM CITAS_TUOTEMPO('0', '0', @oidexplo, null,null) p;";

                // Crea el comando
                using (var command = new FbCommand(query, connection))
                {
                    // Añade el parámetro @Oid al comando

                    command.Parameters.Add("@oidexplo", FbDbType.Integer).Value = oPaciente.app_lid.ToString();

                    // Ejecuta el comando y usa un FbDataReader para leer los resultados
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) // Si hay al menos un resultado
                        {
                            appointmentReturn = CreateAppointmentResponseFromReader(reader);


                        }
                    }
                }
                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = appointmentReturn
                };

                return Ok(response);
            }         


        
     


        }



        private AppointmentResponse CreateAppointmentResponseFromReader(IDataReader reader)
        {
            var appointmentReturn = new AppointmentResponse
            {
                app_lid = reader["APP_LID"].ToString(),
                created = reader["created"] != DBNull.Value ? reader["created"].ToString() : "",
                cancelled = reader["CANCElLED"] != DBNull.Value && reader["CANCElLED"].ToString() == "T",
                modified = reader["modified"] != DBNull.Value ? reader["modified"].ToString() : "",
                status = reader["status"].ToString(),
                checkedin = reader["checkedin"] != DBNull.Value ? reader["checkedin"].ToString() : "",
                start_visit = reader["start_visit"] != DBNull.Value ? reader["start_visit"].ToString() : "",
                end_visit = reader["end_visit"] != DBNull.Value ? reader["end_visit"].ToString() : "",
                communication = new communication
                {
                    preparation = reader["preparation"] != DBNull.Value ? reader["preparation"].ToString() : "",
                    reminder = reader["reminder"] != DBNull.Value ? reader["reminder"].ToString() : "",
                },
                disable_reschedule = reader["disable_reschedule"] != DBNull.Value && reader["disable_reschedule"].ToString() == "T",
                disable_cancel = reader["disable_cancel"] != DBNull.Value && reader["disable_cancel"].ToString() == "T"

            };

            Availabilities oHueco = new Availabilities
            {
                availability_lid = reader["availability_lid"] != DBNull.Value ? reader["availability_lid"].ToString() : "",
                date = reader["adate"] != DBNull.Value ? reader["adate"].ToString() : "",
                start_time = reader["start_time"] != DBNull.Value ? reader["start_time"].ToString() : "",
                end_time = reader["end_time"] != DBNull.Value ? reader["end_time"].ToString() : "",
                location_lid = reader["location_lid"] != DBNull.Value ? reader["location_lid"].ToString() : "",
                activity_lid = reader["activity_lid"] != DBNull.Value ? reader["activity_lid"].ToString() : "",
                insurance_lid = reader["insurance_lid"] != DBNull.Value ? reader["insurance_lid"].ToString() : "",
                price = reader["price"] != DBNull.Value ? reader["price"].ToString() : "",
                resource_lid = reader["resource_lid"] != DBNull.Value ? reader["resource_lid"].ToString() : "",
            };

            appointmentReturn.availability = oHueco;

            string sexo = reader["GENDER"].ToString();


            user oPaciente = new user();
          
            oPaciente.user_lid = reader["user_lid"] != DBNull.Value ? reader["user_lid"].ToString() : "";
            oPaciente.first_name = reader["first_name"] != DBNull.Value ? reader["first_name"].ToString() : "";
            oPaciente.second_name = reader["second_name"] != DBNull.Value ? reader["second_name"].ToString() : "";
            oPaciente.third_name = reader["third_name"] != DBNull.Value ? reader["third_name"].ToString() : "";
            oPaciente.birthdate = reader["birthdate"] != DBNull.Value ? reader["birthdate"].ToString() : "";
            oPaciente.gender = sexo;

            IdNumber idNumber = new IdNumber();
            idNumber.number = reader["id_number"] != DBNull.Value ? reader["id_number"].ToString() : "";
            idNumber.type = reader["id_type"] != DBNull.Value ? Convert.ToInt32(reader["id_type"]) : 1;
            oPaciente.id_number = idNumber;

            Contact contact = new Contact();
            contact.email = reader["CONTACT_EMAIL"] != DBNull.Value ? reader["CONTACT_EMAIL"].ToString() : "";
            contact.mobile = reader["CONTACT_MOBILE"] != DBNull.Value ? reader["CONTACT_MOBILE"].ToString() : "";
            oPaciente.contact = contact;

            Privacy privacy = new Privacy();
            CommunicationPreferences communicationPreferences = new CommunicationPreferences();
            communicationPreferences.SMS = reader["PSMS"] != DBNull.Value ? reader["PSMS"].ToString() == "T" : false;
            communicationPreferences.email = reader["PEMAIL"] != DBNull.Value ? reader["PEMAIL"].ToString() == "T" : false;
            communicationPreferences.phone = reader["PPHONE"] != DBNull.Value ? reader["PPHONE"].ToString() == "T" : false;
            privacy.communication_preferences = communicationPreferences;

            privacy.primary = reader["PPRIMARY"] != DBNull.Value ? reader["PPRIMARY"].ToString() == "T" : false;
            privacy.promotions = reader["PPROMOTIONS"] != DBNull.Value ? reader["PPROMOTIONS"].ToString() == "T" : false;
            privacy.review = reader["PREVIEW"] != DBNull.Value ? reader["PREVIEW"].ToString() == "T" : false;
            privacy.dossier = reader["PDOSSIER"] != DBNull.Value ? reader["PDOSSIER"].ToString() == "T" : false;
            oPaciente.privacy = privacy;



            address userAddress = new address
            {
                street = reader["street"]?.ToString() ?? string.Empty,
                street_number = reader["street_number"]?.ToString() ?? string.Empty,
                city = reader["city"]?.ToString() ?? string.Empty,
                province = reader["province"]?.ToString() ?? string.Empty,
                country = reader["country"]?.ToString() ?? string.Empty,
            };
            oPaciente.address = userAddress;

            appointmentReturn.user = oPaciente;



            return appointmentReturn;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/appointments/updates_since/{timestamp}")]
        public IHttpActionResult Updates_since(string timestamp)
        {

            // Suponiendo que timestamp es una cadena que contiene el timestamp Unix, por ejemplo "1624633549"
            long unixTimestamp = long.Parse(timestamp);
            System.DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimestamp).ToLocalTime();

            // Ahora tienes dateTime como DateTime y puedes formatearlo como desees
            // Por ejemplo, para formatearlo como 'YYYY-MM-DD'
            string fechaFormateada = dateTime.ToString("yyyy-MM-dd");
            string fechaFormateadaFin = dateTime.AddYears(50).ToString("yyyy-MM-dd");




            var appointmentReturnList = new List<AppointmentResponse>();

            // Crea la conexión
            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();


                string query = @"
                    SELECT P.*
                    FROM CITAS_TUOTEMPO('0', '0', '0', @fechainicio, null) p;
                       ";

                // Crea el comando
                using (var command = new FbCommand(query, connection))
                {
                    // Añade el parámetro @Oid al comando
                    command.Parameters.Add("@fechainicio", FbDbType.Date).Value = fechaFormateada;
                    command.Parameters.Add("@fechafin", FbDbType.Date).Value = fechaFormateadaFin;

                    // Ejecuta el comando y usa un FbDataReader para leer los resultados
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) // Si hay al menos un resultado
                        {
                            var appointmentReturn = CreateAppointmentResponseFromReader(reader);
                            appointmentReturnList.Add(appointmentReturn);

                        }
                    }
                }

                // Crear una respuesta JSON
                var response = new MyResponse
                {
                    result = "OK",
                    returnObject = appointmentReturnList
                };

                return Ok(response);
            }
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/appointments/resources/{resource_lid}")]
        public IHttpActionResult GetAppointmentsByResource(string resource_lid, [FromUri] string start_date = null, [FromUri] string end_date = null)
        {
            {

                // Ahora tienes dateTime como DateTime y puedes formatearlo como desees
                // Por ejemplo, para formatearlo como 'YYYY-MM-DD'
                string fechaFormateada = DateTime.Parse(start_date).ToString("yyyy-MM-dd");
                string fechaFormateadaFin = DateTime.Parse(end_date).ToString("yyyy-MM-dd");




                var appointmentReturnList = new List<AppointmentResponse>();

                // Crea la conexión
                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();


                    string query = @"
                    SELECT P.*
                    FROM CITAS_TUOTEMPO('0', @oidaparato, '0', @fechainicio, @fechafin) p;
                       ";

                    // Crea el comando
                    using (var command = new FbCommand(query, connection))
                    {
                        // Añade el parámetro @Oid al comando
                        command.Parameters.Add("@fechainicio", FbDbType.Date).Value = fechaFormateada;
                        command.Parameters.Add("@fechafin", FbDbType.Date).Value = fechaFormateadaFin;
                        command.Parameters.Add("@oidaparato", FbDbType.Integer).Value = resource_lid;

                        // Ejecuta el comando y usa un FbDataReader para leer los resultados
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read()) // Si hay al menos un resultado
                            {
                                var appointmentReturn = CreateAppointmentResponseFromReader(reader);
                                appointmentReturnList.Add(appointmentReturn);

                            }
                        }
                    }

                    // Crear una respuesta JSON
                    var response = new MyResponse
                    {
                        result = "OK",
                        returnObject = appointmentReturnList
                    };

                    return Ok(response);
                }
            }

        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/appointments/users/{user_lid}")]
        public IHttpActionResult GetAppointmentsByPatient(string user_lid, [FromUri] string start_date = null, [FromUri] string end_date = null)
        {
            {

                // Ahora tienes dateTime como DateTime y puedes formatearlo como desees
                // Por ejemplo, para formatearlo como 'YYYY-MM-DD'
                string fechaFormateada = DateTime.Parse(start_date).ToString("yyyy-MM-dd");
                string fechaFormateadaFin = "";

                if (end_date==null)
                {
                    fechaFormateadaFin= DateTime.Now.AddYears(50).ToString("yyyy-MM-dd");
                }
                else
                {
                    fechaFormateadaFin = DateTime.Parse(end_date).ToString("yyyy-MM-dd");

                }          




                var appointmentReturnList = new List<AppointmentResponse>();

                // Crea la conexión
                using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();


                    string query = @"
                    SELECT P.*
                    FROM CITAS_TUOTEMPO(@oidPaciente, '0', '0', @fechainicio, @fechafin) p;
                       ";

                    // Crea el comando
                    using (var command = new FbCommand(query, connection))
                    {
                        // Añade el parámetro @Oid al comando
                        command.Parameters.Add("@fechainicio", FbDbType.Date).Value = fechaFormateada;
                        command.Parameters.Add("@fechafin", FbDbType.Date).Value = fechaFormateadaFin;
                        command.Parameters.Add("@oidPaciente", FbDbType.VarChar).Value = user_lid;

                        // Ejecuta el comando y usa un FbDataReader para leer los resultados
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read()) // Si hay al menos un resultado
                            {
                                var appointmentReturn = CreateAppointmentResponseFromReader(reader);
                                appointmentReturnList.Add(appointmentReturn);

                            }
                        }
                    }

                    // Crear una respuesta JSON
                    var response = new MyResponse
                    {
                        result = "OK",
                        returnObject = appointmentReturnList
                    };

                    return Ok(response);
                }
            }

        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tuotempo/appointments/{app_lid}")]
        public IHttpActionResult GetAppointmentsById(string app_lid)
        {
                var appointmentReturnList = new List<AppointmentResponse>();

     

            // Crea la conexión
            using (var connection = new FbConnection(connectionString))
                {
                    connection.Open();


                    string query = @"
                    SELECT P.*
                    FROM CITAS_TUOTEMPO('0', '0', @oidexplo, null,null) p;";

                    // Crea el comando
                    using (var command = new FbCommand(query, connection))
                    {
                        // Añade el parámetro @Oid al comando
                       
                        command.Parameters.Add("@oidexplo", FbDbType.VarChar).Value = app_lid;

                        // Ejecuta el comando y usa un FbDataReader para leer los resultados
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read()) // Si hay al menos un resultado
                            {
                                var appointmentReturn = CreateAppointmentResponseFromReader(reader);
                                appointmentReturnList.Add(appointmentReturn);

                            }
                        }
                    }

                    // Crear una respuesta JSON
                    var response = new MyResponse
                    {
                        result = "OK",
                        returnObject = appointmentReturnList
                    };

                    return Ok(response);
                }
            }

        }

    }









