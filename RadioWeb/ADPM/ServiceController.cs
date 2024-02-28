using Newtonsoft.Json;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;

namespace RadioWeb.Controllers
{
	//[ServiceAdpm.Filters.BasicAuthorize]
	public class ServiceController : ApiController
	{
		public static string rutaLog = System.Configuration.ConfigurationManager.AppSettings["RutaAPILog"];
        private RadioDBContext db = new RadioDBContext();

		private void adjuntarFichero(EXPLORACION oViewModel, Models.API.Paciente oPaciente, string rutaTemp, string nomArxiuTemp, string extension, int oidPacie)
		{
			//System.IO.FileInfo fInfo = new System.IO.FileInfo(oViewModel.DOCUMENTO.FileName);
			string origen = rutaTemp + "" + nomArxiuTemp + "." + extension;

			if (oidPacie < 1)
			{
				oidPacie = -1;
			}
			IMAGENES oImagen = new IMAGENES
			{
				NOMBRE = nomArxiuTemp + "_" + oViewModel.OID,
				IOR_EXPLORACION = oViewModel.OID,
				IOR_PACIENTE = oidPacie,
				EXT = extension,
				PATH = Utils.Varios.ObtenerCarpetaImagen()
			};

			ImagenesRepositorio.Insertar(oImagen, true);
			oImagen.OID = db.Imagenes.Where(p => p.IOR_EXPLORACION == oViewModel.OID).OrderByDescending(p => p.OID).FirstOrDefault().OID;
			//BOLSA_PRUEBAS oBolsa = db.BolsaPruebas.Single(p => p.OID == oViewModel.OID);
			//oBolsa.IOR_DOCUMENTO = oImagen.OID;
			//db.Entry(oBolsa).State = EntityState.Modified;
			//db.SaveChanges();

			//oViewModel.DOCUMENTO.SaveAs(Utils.Varios.ObtenerCarpetaImagen() + oImagen.OID + fInfo.Extension.ToString().ToLower());
			//Ruta per TEST i PROD
			string desti = Utils.Varios.ObtenerCarpetaImagen() + oImagen.OID + "." + "" + extension;
			//Ruta per local
			//string desti = rutaTemp + "" + nomArxiuTemp + "_" + oViewModel.OID + "." + extension;
			using (var src = System.IO.File.OpenRead(origen))
			using (var dest = System.IO.File.OpenWrite(desti))
			{
				src.CopyTo(dest); //blocks until finished
			}
			System.IO.File.Delete(origen);
		}
		// GET api/service/5
		public HttpResponseMessage Get(string oid)
		{
			string nombreCompleto = PacienteRepositorio.Obtener(int.Parse(oid)).PACIENTE1;
			string nombre = nombreCompleto.Split(',')[1].ToString();
			string apellidos = nombreCompleto.Split(',')[0].ToString();
			var response = new HttpResponseMessage();
			response = Request.CreateResponse(
						   HttpStatusCode.OK, nombre + " " + apellidos);
			return response;

		}

		private HttpResponseMessage VerificacionesManresa(Models.API.Paciente oPaciente)
		{

			var response = new HttpResponseMessage();
			if (oPaciente.Apellidos.Length == 0)
			{

				response = Request.CreateResponse(
						HttpStatusCode.NotAcceptable, "Campo apellidos obligatorio.");
				return response;
			}

			if (oPaciente.Nombre.Length == 0)
			{
				response = Request.CreateResponse(
			 HttpStatusCode.NotAcceptable, "Campo Nombre obligatorio.");

				return response;
			}
			if (oPaciente.Exploracion != null)
			{
				if (oPaciente.Exploracion.TEXTO.Length == 0)
				{
					response = Request.CreateResponse(
				 HttpStatusCode.NotAcceptable, "Indique un texto para la mutua y la exploración.");

					return response;
				}

			}
			try
			{
				DateTime Temp = new DateTime(int.Parse(oPaciente.FechaNacimiento.Substring(6, 4)), int.Parse(oPaciente.FechaNacimiento.Substring(0, 2)), int.Parse(oPaciente.FechaNacimiento.Substring(3, 2)));
				oPaciente.FechaNacimiento = Temp.ToString("MM/dd/yyyy");
			}
			catch
			{

				oPaciente.FechaNacimiento = "01/01/1900";
			}



			if (oPaciente.Sexo.Length == 0)
			{

				return response;
			}



			return response = Request.CreateResponse(HttpStatusCode.OK);
		}

		private void NotifyExitoReserva(Models.API.Paciente oPaciente)
		{

			//CARGAMOS VALORES DE CONFIGURACION DEL SERVER DE CORREO
			XmlDocument doc = new XmlDocument();
			doc.Load(System.Web.HttpContext.Current.Server.MapPath("/XML/Citaonline.xml"));
			string email = doc.GetElementsByTagName("email")[0].InnerText;
			string emailServer = doc.GetElementsByTagName("smtpServer")[0].InnerText;
			int emailServerPort = int.Parse(doc.GetElementsByTagName("smtpPort")[0].InnerText);
			string emailServerUser = doc.GetElementsByTagName("smtpUser")[0].InnerText;
			string emailServerPass = doc.GetElementsByTagName("smtpPass")[0].InnerText;

			List<string> emailCallCenter = doc.GetElementsByTagName("mailCallCenter")[0].InnerText.Split(';').ToList();
			emailCallCenter.Add(oPaciente.Email);
			string mailConfirmacionCita = doc.GetElementsByTagName("mailConfirmacionCita")[0].InnerText;
			mailConfirmacionCita = mailConfirmacionCita.Replace("@PACIENTE@", oPaciente.Nombre.ToUpper() + " " + oPaciente.Apellidos.ToUpper());


			string mailConfirmacionCitasTemplate = doc.GetElementsByTagName("mailConfirmacionCitas")[0].InnerText;
			string mailConfirmacionCitasContent = "";
			foreach (EXPLORACION explo in oPaciente.Exploraciones)
			{
				string Centro = CentrosRepositorio.Obtener((int)DaparatoRepositorio.Obtener((int)explo.IOR_APARATO).CID).NOMBRE;
				mailConfirmacionCitasContent = mailConfirmacionCitasContent + mailConfirmacionCitasTemplate;
				mailConfirmacionCitasContent = mailConfirmacionCitasContent.Replace("@EXPLORACION@", explo.APARATO.DES_FIL + "     ");
				mailConfirmacionCitasContent = mailConfirmacionCitasContent.Replace("@FECHA@", explo.FECHA.Value.ToShortDateString() + "     ");
				mailConfirmacionCitasContent = mailConfirmacionCitasContent.Replace("@HORA@", explo.HORA + "     ");
				mailConfirmacionCitasContent = mailConfirmacionCitasContent.Replace("@CENTRO@", Centro + "     ");
				mailConfirmacionCitasContent = mailConfirmacionCitasContent.Replace("@INDICACIONES@", AparatoRepositorio.ObtenerTextoAparato(int.Parse(explo.IOR_TIPOEXPLORACION.ToString())) + "     ");
				mailConfirmacionCitasContent = mailConfirmacionCitasContent + "<hr/>";
			}

			mailConfirmacionCita = mailConfirmacionCita.Replace("@CITAS@", mailConfirmacionCitasContent);
			mailConfirmacionCita = mailConfirmacionCita.Replace("@oid@", oPaciente.Exploraciones[0].IOR_PACIENTE.ToString());

			Utils.Varios.EnviarMail(mailConfirmacionCita, emailCallCenter, emailServer, emailServerPort, emailServerUser, emailServerPass, email, true);



		}


		private void NotifyPruebaNoCubierta(Models.API.Paciente oPaciente)
		{

			//CARGAMOS VALORES DE CONFIGURACION DEL SERVER DE CORREO
			XmlDocument doc = new XmlDocument();
			doc.Load(System.Web.HttpContext.Current.Server.MapPath("/XML/Citaonline.xml"));
			string emailServer = doc.GetElementsByTagName("smtpServer")[0].InnerText;
			string email = doc.GetElementsByTagName("email")[0].InnerText;
			int emailServerPort = int.Parse(doc.GetElementsByTagName("smtpPort")[0].InnerText);
			string emailServerUser = doc.GetElementsByTagName("smtpUser")[0].InnerText;
			string emailServerPass = doc.GetElementsByTagName("smtpPass")[0].InnerText;

			List<string> emailCallCenter = doc.GetElementsByTagName("mailCallCenter")[0].InnerText.Split(';').ToList();

			string mailPruebaNoCubierta = doc.GetElementsByTagName("mailPruebaNoCubierta")[0].InnerText;
			mailPruebaNoCubierta = mailPruebaNoCubierta.Replace("@PACIENTE@", oPaciente.Nombre.ToUpper() + " " + oPaciente.Apellidos.ToUpper());
			mailPruebaNoCubierta = mailPruebaNoCubierta.Replace("@EXPLORACION@", oPaciente.Exploracion.APARATO.DES_FIL);
			mailPruebaNoCubierta = mailPruebaNoCubierta.Replace("@TELEFONO@", oPaciente.Telefono);
			mailPruebaNoCubierta = mailPruebaNoCubierta.Replace("@MUTUA@", MutuasRepositorio.Obtener(int.Parse(oPaciente.IOR_MUTUA)).NOMBRE);
			mailPruebaNoCubierta = mailPruebaNoCubierta.Replace("@FECHA@", oPaciente.Exploracion.FECHA.ToString());
			mailPruebaNoCubierta = mailPruebaNoCubierta.Replace("@HORA@", oPaciente.Exploracion.HORA);
			Utils.Varios.EnviarMail(mailPruebaNoCubierta, emailCallCenter, emailServer, emailServerPort, emailServerUser, emailServerPass, email, true);



		}

		private void NotifyPruebaEspecialCallCenter(Models.API.Paciente oPaciente)
		{
			//CARGAMOS VALORES DE CONFIGURACION DEL SERVER DE CORREO
			XmlDocument doc = new XmlDocument();
			doc.Load(System.Web.HttpContext.Current.Server.MapPath("/XML/Citaonline.xml"));
			string email = doc.GetElementsByTagName("email")[0].InnerText;
			string emailServer = doc.GetElementsByTagName("smtpServer")[0].InnerText;
			int emailServerPort = int.Parse(doc.GetElementsByTagName("smtpPort")[0].InnerText);
			string emailServerUser = doc.GetElementsByTagName("smtpUser")[0].InnerText;
			string emailServerPass = doc.GetElementsByTagName("smtpPass")[0].InnerText;


			List<string> emailCallCenter = doc.GetElementsByTagName("mailCallCenter")[0].InnerText.Split(';').ToList();

			string feedBackPruebaEspecial = doc.GetElementsByTagName("feedBackPruebaEspecial")[0].InnerText;
			string mailPruebaEspecial = doc.GetElementsByTagName("mailPruebaEspecial")[0].InnerText;


			mailPruebaEspecial = mailPruebaEspecial.Replace("@PACIENTE@", oPaciente.Nombre.ToUpper());
			mailPruebaEspecial = mailPruebaEspecial.Replace("@EXPLORACION@", oPaciente.Exploracion.DIASEMANA);
			mailPruebaEspecial = mailPruebaEspecial.Replace("@TELEFONO@", oPaciente.Telefono);

			Utils.Varios.EnviarMail(mailPruebaEspecial, emailCallCenter, emailServer, emailServerPort, emailServerUser, emailServerPass, email, true);


		}

        
		// POST api/service 
		//Utilizamos este método tanto para dar altas de Manresa como de CitaOnline
		public HttpResponseMessage Post([FromBody]Models.API.Paciente oPaciente)
		{
			try
			{
               
                int oidPaciente;
				switch (oPaciente.Origen.ToUpper())
				{

					case "MANRESA":
						HttpResponseMessage response = VerificacionesManresa(oPaciente);
						if (response.StatusCode == HttpStatusCode.OK)
						{
							 oidPaciente = RadioWeb.Models.Repos.PacienteRepositorio.InsertDesdeApiManresa(oPaciente);
							oPaciente.Exploracion.IOR_PACIENTE = oidPaciente;
							
							ExploracionRepositorio.Insertar(oPaciente.Exploracion);
							response = Request.CreateResponse(
										HttpStatusCode.Created, oPaciente);
							string fic = rutaLog + @"\OK\Manresa_" + DateTime.Now.ToString("yyyyMMddHHmmss") + oPaciente.Nombre + oPaciente.Apellidos + ".log";
							string json = JsonConvert.SerializeObject(oPaciente);
							System.IO.File.WriteAllText(fic, json);

						}


						return response;


					case "CLINICUM":
						oPaciente.IOR_MUTUA = "3820110";
						response = VerificacionesManresa(oPaciente);
						if (response.StatusCode == HttpStatusCode.OK) {

							oidPaciente = RadioWeb.Models.Repos.PacienteRepositorio.InsertDesdeApiCitaOnline(oPaciente);
							response = Request.CreateResponse(
									  HttpStatusCode.Created, oidPaciente);
						}
					  
						 
						 return response;
						

					default:
						response = new HttpResponseMessage();
                       

                        if (oPaciente.Nombre == "PRUEBA ESPECIAL CITAONLINE")
						{
							oPaciente.Oid = -1;
							NotifyPruebaEspecialCallCenter(oPaciente);
							response = Request.CreateResponse(HttpStatusCode.Created, oPaciente);
							return response;
						}


						 oidPaciente = 0;

						int indice = 0;
						foreach (Models.EXPLORACION explo in oPaciente.Exploraciones)
						{
                         
                            // Vamos a mirar si existe precio para esta exploración
                            string precioExploracion = Models.Repos.TarifasRepositorio.ObtenerPrecioExploracion((int)explo.IOR_TIPOEXPLORACION, (int)explo.IOR_ENTIDADPAGADORA);
                          
                            bool pruebaSinTarifa = (precioExploracion.Length == 0 || precioExploracion == null);
							bool pacientePrivado = int.Parse(oPaciente.IOR_MUTUA) == 3820080;
							RadioWeb.Models.APARATOS oAparatoPrueba = AparatoRepositorio.Obtener((int)explo.IOR_TIPOEXPLORACION);
							bool pruebaEspecial = (oAparatoPrueba.SMS == "1");
							string Centro = CentrosRepositorio.Obtener((int)DaparatoRepositorio.Obtener((int)explo.IOR_APARATO).CID).NOMBRE;
							//ASIGNAMOS LA FECHA DE LA EXPLORACION Y EL OID DEL PACIENTE
							//Para evitar problemas de serializacion con las fechas la hemos puesto
							//en un string, el del estadodescripcion
							string fecha = explo.ESTADODESCRIPCION;
							explo.FECHA = DateTime.Parse(fecha);
							if (!pruebaEspecial)
							{
								//Formateamos la fecha de nacimiento del paciente
								oPaciente.FechaNacimiento = oPaciente.FechaNacimiento.Split('/')[1] + "/" + oPaciente.FechaNacimiento.Split('/')[0] + "/" + oPaciente.FechaNacimiento.ToString().Split('/')[2].Substring(0, 4);
								//hacemos un insert or update del paciente y obtenemos el oid del mismo 
								if (oidPaciente == 0)
								{                                    
                                    oidPaciente = RadioWeb.Models.Repos.PacienteRepositorio.InsertDesdeApiCitaOnline(oPaciente);                          
                                }
								if (oPaciente.IdEnvio!=null &&  oPaciente.IdEnvio.Length >0 )
								{
									explo.IDCITAONLINE = "CLI_" + oPaciente.IdEnvio;
								}
								explo.IOR_PACIENTE = oidPaciente;
								if (!pruebaSinTarifa)
								{
									explo.CANTIDAD = float.Parse(precioExploracion);
								}


								Boolean res = ExploracionRepositorio.checkExistingAppointments(explo);

								if (res)
								{
									response = Request.CreateResponse(HttpStatusCode.Conflict);
									return response;
								}

								//INSERTAMOS LA EXPLORACION
								EXPLORACION exploInsertada = ExploracionRepositorio.Obtener(ExploracionRepositorio.Insertar(explo));
								explo.OID = exploInsertada.OID;
								if (oPaciente.DOCUMENTO != null)
								{
									string data = "";
									string retornExtension = "";
									string nomArxiuTemp = "";
									string rutaFinal = "";

									oPaciente.DOCUMENTO = oPaciente.DOCUMENTO.Replace("data:image/pdf;base64,", "");
									oPaciente.DOCUMENTO = oPaciente.DOCUMENTO.Replace("data:image/txt;base64,", "");
									oPaciente.DOCUMENTO = oPaciente.DOCUMENTO.Replace("data:image/png;base64,", "");
									oPaciente.DOCUMENTO = oPaciente.DOCUMENTO.Replace("data:application/pdf;base64,", "");
									oPaciente.DOCUMENTO = oPaciente.DOCUMENTO.Replace("data:application/png;base64,", "");
									oPaciente.DOCUMENTO = oPaciente.DOCUMENTO.Replace("data:application/txt;base64,", "");
									data = oPaciente.DOCUMENTO.Substring(0, 5);

									switch (data.ToUpper())
									{
										case "IVBOR":
											retornExtension = "png";
											break;
										case "/9J/4":
											retornExtension = "jpg";
											break;
										case "AAAAF":
											retornExtension = "mp4";
											break;
										case "JVBER":
											retornExtension = "pdf";
											break;
										case "AAABA":
											retornExtension = "ico";
											break;
										case "UMFYI":
											retornExtension = "rar";
											break;
										case "E1XYD":
											retornExtension = "rtf";
											break;
										case "U1PKC":
											retornExtension = "txt";
											break;
										case "77U/M":
											retornExtension = "srt";
											break;
										default:
											retornExtension = "pdf";
											break;
									}
									nomArxiuTemp = oPaciente.Apellidos + " ," + oPaciente.Nombre;
									//Ruta per local
									//string rutaFinal = @"C:\Users\djimenez\Documents\";
									//Ruta per TEST
									WebConfigRepositorio oConfig = new WebConfigRepositorio();
									 rutaFinal = oConfig.ObtenerValor("RutaDocumentosEscaneadosTemp");
									File.WriteAllBytes(rutaFinal + "" + nomArxiuTemp + "." + retornExtension, Convert.FromBase64String(oPaciente.DOCUMENTO));
									adjuntarFichero(exploInsertada, oPaciente, rutaFinal, nomArxiuTemp, retornExtension, oidPaciente);
								}
								exploInsertada.TEXTO = explo.APARATO.TEXTO;
								exploInsertada.CONTRASTE = Centro.Replace(" ", "").ToUpper();
								try
								{
									string ipAddress = System.Web.HttpContext.Current.Request.UserHostAddress;
									string tipoExploracion = "";
									if (!string.IsNullOrWhiteSpace(exploInsertada.APARATO.COD_FIL) && exploInsertada.APARATO.COD_FIL.Length > 2)
									{
										tipoExploracion = exploInsertada.APARATO.COD_FIL.Substring(3);
									}
									//Insertamos en el LOG de CitaOnline la cita actual
									LOGCITAONLINE oLog = new LOGCITAONLINE
									{
										FECHA = exploInsertada.FECHA.Value.ToString("MM/dd/yyyy"),
										HORA = exploInsertada.HORA,
										TEXTO = ipAddress + " - " + exploInsertada.PACIENTE.PACIENTE1 + " - " + exploInsertada.OID,
										GRUPO = GAparatoRepositorio.Obtener((int)exploInsertada.APARATO.OWNER).COD_GRUP,
										EXPLORACION = tipoExploracion,
										OWNER = oPaciente.IOR_CENTROEXTERNO,
										MODIF = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
										APARATO = exploInsertada.DAPARATO.COD_FIL,
										USERNAME = exploInsertada.USERNAME,
										CID = 1

									};

									LogCitaOnlineRepositorio.Insertar(oLog);

								}
								catch (Exception ex)
								{
									LogException.LogMessageToFile(ex.Message);
									string fic = System.Configuration.ConfigurationManager.AppSettings["RutaAPILog"] + @"\excepciones\" + DateTime.Now.ToString("yyyyMMddHHmmss") + exploInsertada.OID + exploInsertada.PACIENTE.PACIENTE1 + ".log";
									string json = JsonConvert.SerializeObject(oPaciente);
									System.IO.File.WriteAllText(fic, json);

								}
							   
								if (pacientePrivado || pruebaSinTarifa)
								{
									Models.TEXTOS oTexto = new Models.TEXTOS();
									oTexto.TEXTO = exploInsertada.APARATO.DES_FIL + " - CITAONLINE";
									oTexto.OWNER = exploInsertada.OID;
									TextosRepositorio.InsertarOrUpdate(oTexto);
								}
							}
							else
							{
								oPaciente.Oid = -1;
								NotifyPruebaEspecialCallCenter(oPaciente);
							}
							//Si el paciente es de mutua pero no tiene tarifa para esa mutua
							//Aunque se ha dado de alta se notifica al call Center para que lo llamen
							if (!pacientePrivado && pruebaSinTarifa)
							{
								oPaciente.Oid = -2;
								oPaciente.Exploracion = explo;
								NotifyPruebaNoCubierta(oPaciente);
							}
							indice = indice + 1;
						}

						response = Request.CreateResponse(
									HttpStatusCode.Created, oPaciente);
						NotifyExitoReserva(oPaciente);
						return response;
				} 

			}
			catch (Exception ex)
			{
				HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
				message.Content = new StringContent(ex.Message);
				string fic = rutaLog + @"\KO\" + DateTime.Now.ToString("yyyyMMddHHmmss") + oPaciente.Nombre + oPaciente.Apellidos + ".log";
				string json = JsonConvert.SerializeObject(oPaciente) + "************" + ex.Message;
				System.IO.File.WriteAllText(fic, json);

				throw new HttpResponseException(message);

			}
		}

		// PUT api/service/5
		public void Put(int id, [FromBody]string value)
		{
		}

		// DELETE api/service/5
		public HttpResponseMessage Delete(string id)
		{
			var response = new HttpResponseMessage();
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            string RutaWL = oConfig.ObtenerValor("RutaWL");
			string RutaWLAffidea = oConfig.ObtenerValor("RutaWLAffidea");
			ExploracionRepositorio.DeleteDesdeApi( id,RutaWL, RutaWLAffidea);

			response = Request.CreateResponse(
						   HttpStatusCode.OK, "Exploracion Borrada.");
			return response;
		}
	}
}
