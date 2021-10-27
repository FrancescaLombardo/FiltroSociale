using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiltroSociale
{
    class Validazione_Base

    {
        internal bool ValidazioneBase(double valore, double soglia_max, double soglia_min)

        {
            // verifico che non sia NaN
            //Verifico che non sia infinity
            //verifico che sia Maggiore di soglia min
            //Verifica che non sia Exval
            //verifica che non sia -9999,-999, 999. 999
            //verifico sia minore di soglia


            bool passaggio_validazione = true;
            try
            {
                Convert.ToDouble(valore);
            }
            catch (System.Exception e)
            {
                passaggio_validazione = false;
            }

            if (double.IsInfinity(valore)) passaggio_validazione = false;
            if (double.IsNegativeInfinity(valore)) passaggio_validazione = false;
            if (double.IsPositiveInfinity(valore)) passaggio_validazione = false;
            if (double.IsNaN(valore)) passaggio_validazione = false;
            if (valore < soglia_min) passaggio_validazione = false;
            if (valore > soglia_max) passaggio_validazione = false;
            if (valore==MainProgram.ExVal) passaggio_validazione = false;
            if ((valore == -999) ||(valore == -9999)) passaggio_validazione = false;
            if ((valore == 999) || (valore == 9999)) passaggio_validazione = false;

            return passaggio_validazione;
        }
    }
}
