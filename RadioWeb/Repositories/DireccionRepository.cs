using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace RadioWeb.Repositories
{
    public class DireccionRepository
    {
        private readonly RadioDBContext _context;

        public DireccionRepository(RadioDBContext context)
        {
            _context = context;
        }

        public void Update(IEnumerable<DIRECCION> direcciones, int owner)
        {
            foreach (var direccion in direcciones)
            {
                direccion.OWNER = owner;

                // Si la dirección es nueva (OID <= 0)
                if (direccion.OID <= 0)
                {
                    if (!string.IsNullOrWhiteSpace(direccion.DIRECCION1)) // Validar datos no vacíos
                    {
                        direccion.IOR_TIPO = direccion.IOR_TIPO > 0 ? direccion.IOR_TIPO : 0; // Asegurar valor válido
                        _context.Entry(direccion).State = EntityState.Added; // Marcar para inserción
                    }
                }
                else // Si la dirección ya existe
                {
                    if (string.IsNullOrWhiteSpace(direccion.DIRECCION1)) // Eliminar si está vacía
                    {
                        _context.Entry(direccion).State = EntityState.Deleted;
                    }
                    else // Actualizar si tiene datos
                    {
                        _context.Entry(direccion).State = EntityState.Modified;
                    }
                }
            }

            // Guardar cambios después de procesar todas las direcciones
            _context.SaveChanges();
        }

        public void Insert(DIRECCION direccion)
        {
            if (direccion != null && !string.IsNullOrWhiteSpace(direccion.DIRECCION1))
            {
                _context.Entry(direccion).State = EntityState.Added;
                _context.SaveChanges();
            }
        }

        public void Delete(DIRECCION direccion)
        {
            if (direccion != null)
            {
                _context.Entry(direccion).State = EntityState.Deleted;
                _context.SaveChanges();
            }
        }

        public IEnumerable<DIRECCION> GetDireccionesByOwner(int owner)
        {
            return _context.Direcciones .Where(d => d.OWNER == owner).ToList();
        }
    }
}
