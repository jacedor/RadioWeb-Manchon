using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace RadioWeb.Models.Repos
{
    public class RadioDBContext : DbContext
    {
        public RadioDBContext()
            : base("name=ConexionBD")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingEntitySetNameConvention>();
            // configures one-to-many relationship
            // Configure Student & StudentAddress entity
            modelBuilder.Entity<DIRECCION>()
                .HasRequired(c => c.MUTUA)
                .WithMany(c => c.DIRECCIONES)
                .HasForeignKey(c => c.OWNER);

            modelBuilder.Entity<DIRECCION>()
               .HasRequired(c => c.CENTROEXTERNO)
               .WithMany(c => c.DIRECCIONES)
               .HasForeignKey(c => c.OWNER);

            modelBuilder.Entity<TELEFONO>()
                .HasRequired(c => c.MUTUA)
                .WithMany(c => c.TELEFONOS)
                .HasForeignKey(c => c.OWNER);

            modelBuilder.Entity<TELEFONO>()
               .HasRequired(c => c.CENTROEXTERNO)
               .WithMany(c => c.TELEFONOS)
               .HasForeignKey(c => c.OWNER);


            modelBuilder.Entity<PRECIOS>()
               .HasRequired(c => c.APARATO)
               .WithMany(c => c.PRECIOS)
               .HasForeignKey(c => c.IOR_TIPOEXPLORACION);
            //   modelBuilder.Entity<TEXTOS>()
            //.HasRequired(c => c.MUTUA)
            //.WithRequiredDependent(c => c.TEXTO)
            //.Map(c => c.MapKey("OWNER"));






        }

        public class MyContextInitializer : CreateDatabaseIfNotExists<RadioDBContext>
        {
            protected override void Seed(RadioDBContext context)
            {

                // Add defaults to certain tables in the database
                base.Seed(context);
            }
        }

        public DbSet<Models.AGENDAGEN> AgendaGen { get; set; }
        public DbSet<Models.AYUDA> Ayuda { get; set; }
        public DbSet<Models.APARATOS> Aparatos { get; set; }
        public DbSet<Models.AparatosComplejos> AparatosComplejos { get; set; }
        public DbSet<Models.BOLSA_PRUEBAS> BolsaPruebas { get; set; }
        public DbSet<Models.LISTADIA> ListaDia { get; set; }

        public DbSet<Models.CARTELERA> Carteleras { get; set; }
        public DbSet<Models.CENTROS> Centros { get; set; }
        public DbSet<Models.CENTROSEXTERNOS> CentrosExternos { get; set; }
        public DbSet<Models.COLEGIADOS> Colegiados { get; set; }
        public DbSet<Models.CONSUMIBLES> Consumibles { get; set; }
        public DbSet<Models.CONS_GRUPO> Cons_Grupo { get; set; }
        public DbSet<Models.DAPARATOS> Daparatos { get; set; }
        public DbSet<Models.DIRECCION> Direcciones{ get; set; }
        public DbSet<Models.DESCUENTOS> DESCUENTOS { get; set; }
        public DbSet<Models.EXP_CONSUM> Exp_Consum { get; set; }
        public DbSet<Models.FACTURAS> Facturas { get; set; }
        public DbSet<Models.FORMULARIO> Formulario { get; set; }
        public DbSet<Models.FORMULARIO_PREGUNTA> Formulario_Pregunta { get; set; }
        public DbSet<Models.FORMULARIO_PREGUNTAS_RESPUESTAS> Formulario_Pregunta_Respuestas { get; set; }
        public DbSet<Models.FORMULARIO_RESPUESTAS> Formulario_Respuestas { get; set; }
        public DbSet<Models.FORMULARIO_TIPO_ELEMENTO> Formulario_Tipo_Elemento { get; set; }
        public DbSet<Models.GAPARATOS> Gaparatos { get; set; }
        public DbSet<Models.HISTORIAS> Historias { get; set; }
        public DbSet<Models.INFORMESPDF> InformesPDF { get; set; }
        public DbSet<Models.KIOSKO_DAPARATO_TV> KioskoDaparatoTV { get; set; }
        public DbSet<Models.KIOSKO_TV> KioskoTV { get; set; }
        public DbSet<Models.LINEAS_FACTURAS> Lineas_Facturas { get; set; }
        public DbSet<Models.MUTUAS> Mutuas { get; set; }
        
        public DbSet<Models.NUM_FACTURAS> Num_Facturas { get; set; }
        public DbSet<Models.PERSONAL> Personal { get; set; }
        public DbSet<Models.INFORMES> Informes { get; set; }
        //LO UTILIZAMOS PARA TODO TIPO DE DOCUMENTOS
        public DbSet<Models.IMAGENES> Imagenes { get; set; }

        public DbSet<Models.PAGOS> Pagos { get; set; }

        public DbSet<Models.PRECIOS_CONSUM> PreciosConsum { get; set; }
        public DbSet<Models.PRECIOS> Precios { get; set; }

        public DbSet<Models.P_INFORMES> P_Informes { get; set; }
        public DbSet<Models.PIVOTTABLE> PivotTable{ get; set; }

        //REUTILIZADO DEL OFTALMO, AUNQUE PAREZCA MENTIRA SON LOS TIPOS DE DOCUMENTOS
        //(AUTORIZACION, PETICION...)
        public DbSet<Models.REFRACTOMETROS> Refractometros { get; set; }
        public DbSet<Models.TABLETAS> Tabletas { get; set; }
        public DbSet<Models.TEXTOSLIBRES> TextosLibres { get; set; }

        public DbSet<Models.TEXTOSSMS> TextosSms { get; set; }
        public DbSet<Models.VALORACION> Valoracion { get; set; }
        public DbSet<Models.VID_PREGUNTAS> Vid_Preguntas { get; set; }
        public DbSet<Models.VID_RESPUESTAS> Vid_Respuestas { get; set; }
        public DbSet<Models.VID_DOCUMENTOS> Vid_Documentos { get; set; }
        public DbSet<Models.WEBCONFIG> WebConfig { get; set; }

        public System.Data.Entity.DbSet<RadioWeb.Models.PAGO_PACIENTE> PAGO_PACIENTE { get; set; }
    }
}