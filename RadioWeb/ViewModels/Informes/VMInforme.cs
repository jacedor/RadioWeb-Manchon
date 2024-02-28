using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.ViewModels.Informes
{
    public class VMInforme
    {
       

        public int OID { get; set; }
        public VMInforme()
        { }


        public VMInforme(int OID)
        {
            RadioDBContext db = new RadioDBContext();
            INFORMES oInforme;
            VALORACIONESINFORME= new List<Informes.VALORACIONITEM> {
                new VALORACIONITEM { ID = 0, DESCRIPCION = "0 - Sin Valor" },
                new VALORACIONITEM { ID = 1, DESCRIPCION = "1 - De acuerdo con la interpretación" },
                new VALORACIONITEM { ID = 2, DESCRIPCION = "2 - Discrepancia leve, diagnóstico dificil o omitió un hallazgo casual" },
                new VALORACIONITEM { ID = 3, DESCRIPCION = "3 - Discrepancia moderada, diagnóstico evidente o equivoco Drcha por Izqu" },
                new VALORACIONITEM { ID = 4, DESCRIPCION = "4 - Discrepancia severa, diagnóstico fácil o se omitió algo importante (ej: Tumor)" }
            } ;

            
            VALORACIONESIMAGEN = new List<Informes.VALORACIONITEM> {
                new VALORACIONITEM { ID = 0, DESCRIPCION = "0 - Sin Valor" },
                new VALORACIONITEM { ID = 1, DESCRIPCION = "1 - Buena Calidad de las imágenes" },
                new VALORACIONITEM { ID = 2, DESCRIPCION = "2 - Regular calidad de las imágenes, pero diagnosticables" },
                new VALORACIONITEM { ID = 3, DESCRIPCION = "3 - Deficiente calidad de las imágenes, dificultan el diagnóstico" },
                new VALORACIONITEM { ID = 4, DESCRIPCION = "4 - Mala calidad de las imágenes, obliga a RECITAR" }
            };

            WebConfigRepositorio oConfig = new WebConfigRepositorio();
        
            //SI SE TRATA DE UN INFORME EXISTENTE
            if (OID > 0)
            {
                oInforme = db.Informes.Single(i => i.OID == OID);
                VALORACION oValoracion = db.Valoracion.Where(v => v.OWNER == OID).FirstOrDefault();
                this.OID = oInforme.OID;
                this.EXPLORACION = ExploracionRepositorio.Obtener(oInforme.OWNER.Value);
                PERSONAL oPersonal = db.Personal.Single(p => p.OID == oInforme.IOR_MEDINFORME);
                PERSONAL oMedicoRevisor = db.Personal
                    .Where(p => p.OID == oInforme.IOR_MEDREVISION)
                    .SingleOrDefault();
                this.MEDICOREVISOR = oMedicoRevisor.NOMBRE;
                this.LOGINMEDICOREVISOR = oMedicoRevisor.LOGIN;
                this.LOGINMEDICOINFORMANTE = oPersonal.LOGIN;
                this.MEDICOINFORMANTE = oPersonal.NOMBRE;
                this.OWNER = oInforme.OWNER;
                this.COD_PAC = oInforme.COD_PAC;
                this.IOR_PAC = oInforme.IOR_PAC;
                this.ALFAS2 = oInforme.ALFAS2;
                this.USERNAME = oInforme.USERNAME;
                this.VALIDACION = oInforme.VALIDACION;
                this.ALFAS = oInforme.ALFAS;
                this.FECHA = oInforme.FECHA;
                this.FECHAREVISION = (oInforme.FECHAREVISION.HasValue ? oInforme.FECHAREVISION.Value.ToShortDateString() : "");
                this.HORAREV = oInforme.HORAREV;
                this.TITULO = oInforme.TITULO;
                this.DURACION = oInforme.DURACION;
                this.HORA = oInforme.HORA;
                this.IOR_MEDINFORME = oInforme.IOR_MEDINFORME;
                this.IOR_MEDREVISION = oInforme.IOR_MEDREVISION;
                this.IOR_TECNICO = oInforme.IOR_TECNICO;
                this.TEXTOHTML = InformesRepositorio.ObtenerHtmlDelInforme(OID);
               
                this.AUDIOSASOCIADOS = ImagenesRepositorio.Obtener(oInforme.OWNER.Value,20);
                if (this.EXPLORACION.APARATO.CANAL == "1")
                {
                    string RutaImportaPrueba = oConfig.ObtenerValor("RUTAIMPORTACIONPRUEBA");
                    if (Directory.Exists(RutaImportaPrueba))
                    {
                        DirectoryInfo oDirectorio = new DirectoryInfo(RutaImportaPrueba);
                        var files = oDirectorio.GetFiles("*" + this.EXPLORACION.PACIENTE.COD_PAC + "_*");
                        foreach (FileInfo item in files)
                        {
                            if (item.CreationTime.ToShortDateString() == this.EXPLORACION.FECHA.Value.ToShortDateString())
                            {
                                //  System.IO.File fInfo = new System.IO.FileInfo(file.FileName);

                              
                                IMAGENES oImagen = new IMAGENES
                                {
                                    IOR_PACIENTE = this.EXPLORACION.IOR_PACIENTE,
                                    IOR_EXPLORACION = this.EXPLORACION.OID,
                                    EXT = item.Extension,
                                    PATH = Utils.Varios.ObtenerCarpetaDocumentosEscaneados(),
                                    OWNER = 21
                                };

                                oImagen.OID = int.Parse(ImagenesRepositorio.Insertar(oImagen));
                                File.Copy(item.FullName, Utils.Varios.ObtenerCarpetaDocumentosEscaneados() + oImagen.NOMBRE  + oImagen.EXT, true);
                                //item.CopyTo(Utils.Varios.ObtenerCarpetaImagen() + oImagen.NOMBRE + item.Extension.ToString().ToLower());
                                try
                                {
                                    File.Delete(item.FullName);
                                }
                                catch (Exception)
                                {

                                   
                                }
                               

                            }
                        }
                    }

                }


                if (this.VALIDACION=="T")
                {
                    //EXPLORACION oExplo = ExploracionRepositorio.Obtener(oInforme.OWNER.Value);
                    this.ENTREGADOSTATUS = (this.EXPLORACION.RECOGIDO == "T" ? "ENTREGADO " + this.EXPLORACION.FECHADERIVACION.Value.ToShortDateString() : "NO ENTREGADO");
                }
                else
                {
                    this.ENTREGADOSTATUS =  "NO ENTREGADO";
                }

                if (oValoracion != null)
                {
                    this.VALORACIONIMAGENES = int.Parse( oValoracion.V_IMAGEN);
                    this.VALORACIONINFORME = int.Parse(oValoracion.V_RADIOLOGO);
                    this.VALORACIONIMAGENESTEXTO = oValoracion.TEXTO_IMAGEN;
                    this.VALORACIONINFORMETEXTO = oValoracion.TEXTO_RADIOLOGO;
                }
            }
            else {
                this.EXPLORACION = ExploracionRepositorio.Obtener(OID);
            }

            try
            {
                this.MODULOGRABARAUDIO = oConfig.ObtenerValor("ModuloAudioInformes").ToString();

                this.URL_PACS = oConfig.ObtenerValor("RUTAPACS" + this.EXPLORACION.DAPARATO.CID).ToString()
                                .Replace("@IOR_PAC", this.EXPLORACION.IOR_PACIENTE.ToString());

                if (this.URL_PACS.Contains("@COD_PAC"))
                {
                    this.URL_PACS = this.URL_PACS.Replace("@COD_PAC", this.EXPLORACION.PACIENTE.COD_PAC);
                }
            }
            catch (Exception)
            {

                this.URL_PACS = "";
            }

           

            //Necesitamos saber si el paciente tiene un movil para notificarle que su informe
            //puede ser recogido
            this.PACIENTE = PacienteRepositorio.Obtener(EXPLORACION.IOR_PACIENTE);

            foreach (TELEFONO item in PACIENTE.TELEFONOS)
            {
                if (item.NUMERO.StartsWith("6"))
                {
                    this.MOVILPACIENTE = item.NUMERO;
                    try
                    {
    this.TEXTOSMS = db.TextosSms.Where(t => t.OID == 8422948).First().TEXTO.Replace(@"\0","");
                    this.TEXTOSMS = this.TEXTOSMS.Substring(0, this.TEXTOSMS.Length - 1);
                    }
                    catch (Exception)
                    {

                        
                    }
                
                }
            }

            if (String.IsNullOrEmpty(this.MOVILPACIENTE))
            {
                this.MOVILPACIENTE = "-1";
            }

        }

        [NotMapped]
        public string ACTION
        {
            get
            {
                return (OID != 0 ? "Edit" : "Create");
            }
        }

        public Nullable<int> OWNER { get; set; }

        public string USERNAME { get; set; }

        public Nullable<System.DateTime> MODIF { get; set; }


        public string BORRADO { get; set; }

        [Required]
        public string TITULO { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]       
        public DateTime FECHA { get; set; }       

        
        [Display(Name = "Fecha Revision")]
        public string FECHAREVISION { get; set; }
        public string URLPREVIA { get; set; }

        public string HORAREV { get; set; }

        [NotMapped]
        public Nullable<System.DateTime> FECHAEXPLORACION { get; set; }

        public string COD_PAC { get; set; }

        public string URL_PACS { get; set; }

        public string ALFAS { get; set; }

        //CUANDO LE ENVIAMOS UN SMS UN PACIENTE INDICANDO QUE YA PUEDE PASAR A RECOGER EL I
        public string ALFAS2 { get; set; }

        public Nullable<double> VISITA { get; set; }

        public string HORAMOD { get; set; }

        public string HORA { get; set; }

        public Nullable<int> IOR_PAC { get; set; }

        [DataType("MEDICO")]
        public int IOR_MEDINFORME { get; set; }

        [DataType("MEDICO")]
        public int IOR_MEDREVISION { get; set; }

        public string PATOLOGICO { get; set; }

        [Display(Name = "Duración")]
        public string DURACION { get; set; }

        [DataType("TECNICO")]
        public Nullable<int> IOR_TECNICO { get; set; }

        public Nullable<int> IOR_MODALIDAD { get; set; }

        public Nullable<int> IOR_SITUACION { get; set; }

        public Nullable<int> IOR_TECNICA { get; set; }

        public string VALIDACION { get; set; }

        [NotMapped]
        public string MEDICOINFORMANTE { get; set; }

        [NotMapped]
        public string LOGINMEDICOINFORMANTE { get; set; }

        [NotMapped]
        public string MODULOGRABARAUDIO { get; set; } 

        [NotMapped]
        public string MEDICOREVISOR { get; set; }

        [NotMapped]
        public string LOGINMEDICOREVISOR { get; set; }

        public PACIENTE PACIENTE { get; set; }
        public EXPLORACION EXPLORACION { get; set; }


        [NotMapped]
        [AllowHtml]
        public string TEXTOHTML { get; set; }

        [NotMapped]
        public string MOVILPACIENTE { get; set; }

        [NotMapped]
        public string ENTREGADOSTATUS { get; set; }

        [NotMapped]
        public string TEXTOSMS { get; set; }

        public List<VALORACIONITEM> VALORACIONESIMAGEN { get; set; }

        
        public List<VALORACIONITEM> VALORACIONESINFORME { get; set; }


       
        [Display(Name = "Valorar Imágenes")]
        public int VALORACIONIMAGENES { get; set; }

        [Display(Name = "Observaciones")]
        public string VALORACIONIMAGENESTEXTO { get; set; }

        [Display(Name = "Valorar Informe")]
        public int VALORACIONINFORME{ get; set; }

        [Display(Name = "Observaciones")]
        public string VALORACIONINFORMETEXTO { get; set; }

        public IEnumerable<P_INFORMES> PLANTILLASINFORMES { get; set; }
        public IEnumerable<IMAGENES> AUDIOSASOCIADOS { get; set; }
    }

    public class VALORACIONITEM
    {
        public int ID { get; set; }
        public String DESCRIPCION { get; set; }
    }
}