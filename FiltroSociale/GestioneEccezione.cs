using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace FiltroSociale
{
    class CustomExceptionModello : Exception
    {
        public CustomExceptionModello() { }

        public CustomExceptionModello(string name)
            : base(String.Format("Errore gestito dal modello: {0}", name))
        {

        }


    }
    class GestioneEccezione
    {
        // scrive sulle tabelle Diagnostica l'eccezione;
        internal void ScriviEccezione(string testo, DateTime data, int severity, int idprocesso)
        {
            testo = testo.Replace('\'', ' ');
            NpgsqlConnection connessione = new NpgsqlConnection();
            connessione = MainProgram.con2;
            //connessione.ConnectionString = MainProgram.con2.ConnectionString;
            if (connessione.State == System.Data.ConnectionState.Closed)
            {
                connessione.Open();
            }
            string descr = "Log generato dal Filtro Sociale";

            NpgsqlCommand Writer = new NpgsqlCommand("insert into diagnostica.logs( " +
                "idprocesso, " +
                "idtipolog, " +
                "dataora," +
                " messaggio," +
               " descr) " +
                "values (" + idprocesso + " , " + severity + " , " + "'" + data.ToString("yyyyMMdd HH:mm:ss") + "'" + " , ' " + testo + " ',' " + descr + " ' )", connessione);

            Writer.ExecuteNonQuery();

            if (severity >= 5)

            {
                // Termino l'applicazione con il codice -1
                Environment.Exit(-1);
            }
        }
    }
}
