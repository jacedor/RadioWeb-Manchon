using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RadioWeb.Repositories
{
    public class DireccionRepository
    {
        private readonly RadioDBContext _context;
        public DireccionRepository(RadioDBContext context )
        {
            _context = context;
        }

        public void Update(IEnumerable<DIRECCION> direcciones,int owner)
        {
            foreach (var item in direcciones)
            {
                item.OWNER = owner;
                if (item.OID <= 0)
                {
                    item.IOR_TIPO = 0;
                    if (!String.IsNullOrEmpty(item.DIRECCION1))
                    {
                        _context.Entry(item).State = EntityState.Added;
                    }
                    else
                    {
                        _context.Entry(item).State = EntityState.Deleted;
                    }
                }
                else
                {
                    _context.Entry(item).State = EntityState.Modified;
                }

            }
        }
    }
}