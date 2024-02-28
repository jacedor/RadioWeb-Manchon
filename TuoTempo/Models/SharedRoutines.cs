using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace TuoTempo.Models
{
    public class SharedRoutines
    {

        private static string RemoveControlCharacters(string input)
        {
            // Eliminar caracteres de control, como \u001f
            return new string(input.Where(c => !char.IsControl(c)).ToArray());
        }
        private static string GetStringFromDataRow(DataRow row, string columnName)
        {

            // Obtener un valor de cadena desde la fila de datos
            if (row.Table.Columns.Contains(columnName))
            {
                if (!row.IsNull(columnName))
                {
                    string encodedString = row[columnName].ToString();
                    string decodedString = Regex.Unescape(encodedString);

                    // Eliminar caracteres de control
                    decodedString = RemoveControlCharacters(decodedString);
                    return decodedString;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }

        }
    }
}