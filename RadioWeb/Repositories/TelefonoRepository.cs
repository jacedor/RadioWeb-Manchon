using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RadioWeb.Repositories
{
    public class TelefonoRepository
    {
        private readonly RadioDBContext _context;
        public TelefonoRepository(RadioDBContext context)
        {
            _context = context;
        }

        public void Update(IEnumerable<TELEFONO> telefonos, int owner)
        {
            foreach (var item in telefonos)
            {
                item.OWNER = owner;
                item.IOR_TIPO = 3664935;
                if (item.OID < 0)
                {
                    _context.Entry(item).State = EntityState.Added;
                }
                else
                {
                    _context.Entry(item).State = EntityState.Modified;
                }
            }
        }
    }
}