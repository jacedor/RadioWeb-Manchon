using RadioWeb.Models;
using RadioWeb.Models.Logica;
using RadioWeb.Models.Repos;
using RadioWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;

namespace RadioWeb.Repositories
{
    public class ParametrosUsuarioRepository
    {
        private readonly UsersDBContext _context;
        public ParametrosUsuarioRepository(UsersDBContext context)
        {
            _context = context;
        }


        public void Update(string valor, int idUser, string controlador,string propiedad)
        {
            FiltrosRepository _Filtrosrepository;
            _Filtrosrepository = new FiltrosRepository(_context);
            _Filtrosrepository.Guardar(idUser, controlador, propiedad,valor, "string");
        }

        public void Update(int valor, int idUser, string controlador, string propiedad)
        {
            FiltrosRepository _Filtrosrepository;
            _Filtrosrepository = new FiltrosRepository(_context);
            _Filtrosrepository.Guardar(idUser, controlador, propiedad, valor.ToString());
        }
        public void Update(VWFiltros filtros, int idUser, string controlador)
        {
          
              FiltrosRepository _Filtrosrepository;
            _Filtrosrepository = new FiltrosRepository(_context);
            foreach (PropertyInfo prop in filtros.GetType().GetProperties())
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (prop.GetValue(filtros, null)  !=null)
                {
                    if (type == typeof(DateTime))
                    {
                        _Filtrosrepository.Guardar(idUser, controlador, prop.Name, prop.GetValue(filtros, null).ToString(), "string");

                    }
                    if (type == typeof(Boolean))
                    {                        
                        _Filtrosrepository.Guardar(idUser, controlador, prop.Name,( prop.GetValue(filtros, null).ToString()=="True" ?"T":"F"), "string");
                    }
                    if (type == typeof(string))
                    {
                        _Filtrosrepository.Guardar(idUser, controlador, prop.Name, prop.GetValue(filtros, null).ToString(), "string");
                    }
                    if (type == typeof(int))
                    {
                        _Filtrosrepository.Guardar(idUser, controlador, prop.Name, prop.GetValue(filtros, null).ToString());
                    }
                }
               
            }
          
        }

   
    }


}