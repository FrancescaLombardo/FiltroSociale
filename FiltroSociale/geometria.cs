using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiltroSociale
{
    class geometria
    {
        internal void AggregazioneSerieTemporaliAdattato(double[] SerieNonAggregata, DateTime[] DateSerieNonAggregata, bool interpolazione, out double[] SerieAggregata, out DateTime[] DateSerieAggregata, double DTBuco, DateTime[] Date_di_riferimento)
        {

            //descrizione dei parametri
            // input
            // double[] SerieNonAggregata : generica serie temporale con frequenza del dato variabile e/o irregolare
            // DateTime[] DateSerieNonAggregata : elenco delle date-ore di tutti i valori di SerieNonAggregata
            // DateTime data_iniziale: la prima data da cui si vuole partire con il processo di aggregazione
            //bool interpolazione: se true fa interpolazione lineare (ha senso per livelli e portate), se false fa aggregazione (ha senso per piogge)
            //output
            //double[] SerieAggregata: serie aggregata, presenta un dato ogni DT del macrorun a partire da data_iniziale, compresi enventuali nodata (identificati dal valore MainProgram.ExVal)
            //DateTime[] DateSerieAggregata: elenco delle date-ore di tutti i valori di SerieAggregata

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("us-US");
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("it-IT");

            double  //DT del macrorun in ore
               DT=0,  DTmax = DTBuco;
            


            //Determino il numero teorico di elementi che deve avere la serie aggregata
           
            int NumElementi = Date_di_riferimento.Length;

            //Dimensiono gli array di output e compilo DateSerieAggregata
            SerieAggregata = new double[NumElementi];
            DateSerieAggregata = new DateTime[NumElementi];
            try
            {
                for (int i = 0; i < NumElementi; i++)
                {
                    DateSerieAggregata[i] = Date_di_riferimento[i];
                    SerieAggregata[i] = MainProgram.ExVal;
                }

                int NumElementiSerieMisurata = DateSerieNonAggregata.Length;

                if (interpolazione) // procedura di interpolazione
                {
                    for (int i = 0; i < NumElementi; i++)

                    {
                        DateTime Data = DateSerieAggregata[i];
                        if (i< NumElementi-1) DT = ((DateSerieAggregata[i + 1]).Subtract(Data)).TotalHours;

                        for (int j = 0; j < NumElementiSerieMisurata - 1; j++)
                        {
                            DateTime DataMis1 = DateSerieNonAggregata[j], DataMis2 = DateSerieNonAggregata[j + 1];
                           
                            

                            if ((Data >= DataMis1) && (Data <= DataMis2))
                            {
                                // determino il DT fra due misure successive
                                double DTmis = ((DataMis2).Subtract(DataMis1)).TotalHours,
                                    DT1 = ((Data).Subtract(DataMis1)).TotalHours,
                                    DT2 = ((DataMis2).Subtract(Data)).TotalHours;

                                if (DTmis > DTmax)
                                {
                                    if (DT1 < DT) SerieAggregata[i] = SerieNonAggregata[j];
                                    else if (DT2 < DT) SerieAggregata[i] = SerieNonAggregata[j + 1];
                                    else SerieAggregata[i] = MainProgram.ExVal; // se i due dati misurati sono troppo distanti, allora ammetto che ci sia buco nelle registrazioni e quindi a quell'ora metto nodata
                                }

                                else if ((DTmis <= DTmax) && (DTmis > DT))
                                {

                                    SerieAggregata[i] = SerieNonAggregata[j] + (SerieNonAggregata[j + 1] - SerieNonAggregata[j]) * DT1 / DTmis;


                                }

                                else if (DTmis <= DT)
                                {

                                    SerieAggregata[i] = SerieNonAggregata[j] + (SerieNonAggregata[j + 1] - SerieNonAggregata[j]) * DT1 / DTmis;


                                }

                            }

                            else continue;

                            //DEVO GESTIRE I VALORI INIZIALI E FINALI, PERCHE' POTREI NON RIUSCIRE A TROVARE UNA DATA PRIMA E DOPO
                        }
                        if ((SerieAggregata[i] == MainProgram.ExVal) && (Data < DateSerieNonAggregata[0]))
                        {
                            bool condizione = false;
                            int k = 0;
                            DateTime DataSuccessiva = DateSerieAggregata[k + i + 1];
                            while ((!condizione) && (k + 1 + i < NumElementi))
                            {
                                DataSuccessiva = DateSerieAggregata[k + i + 1];

                                if (DataSuccessiva >= DateSerieNonAggregata[0])
                                {
                                    condizione = true;
                                    double DT1 = ((DateSerieNonAggregata[0]).Subtract(DateSerieAggregata[k + i])).TotalHours;
                                    if (DT1 <= DT) SerieAggregata[k + i] = SerieNonAggregata[0];
                                    else SerieAggregata[k + i] = MainProgram.ExVal; // se i due dati misurati sono troppo distanti, allora ammetto che ci sia buco nelle registrazioni e quindi a quell'ora metto nodata
                                }
                                else k++;

                            }
                        }

                        else if ((SerieAggregata[i] == MainProgram.ExVal) && (Data > DateSerieNonAggregata[NumElementiSerieMisurata - 1]))

                        {
                            bool condizione = false;
                            int k = 0;
                            DateTime DataPrec;
                            while ((!condizione) && (k - 1 - i >= 0))
                            {
                                DataPrec = DateSerieAggregata[k - i - 1];

                                if (DataPrec <= DateSerieNonAggregata[NumElementiSerieMisurata - 1])
                                {
                                    condizione = true;
                                    double DT1 = ((DateSerieAggregata[k + i]).Subtract(DateSerieNonAggregata[NumElementiSerieMisurata - 1])).TotalHours;
                                    if (DT1 <= DT) SerieAggregata[k - i] = SerieNonAggregata[NumElementiSerieMisurata - 1];
                                    else SerieAggregata[k - i] = MainProgram.ExVal; // se i due dati misurati sono troppo distanti, allora ammetto che ci sia buco nelle registrazioni e quindi a quell'ora metto nodata
                                }
                                else k++;

                            }

                        }

                    }

                }

                else // aggregazione
                {
                    int numero_misure = DateSerieNonAggregata.Length;
                    double mis_precedente = 0, mis_ultimo_ts = 0; ;

                    for (int i = 0; i < numero_misure - 1; i++)
                    {
                        double Mis1 = SerieNonAggregata[i], Mis2 = SerieNonAggregata[i + 1];
                        DateTime DataMis1 = DateSerieNonAggregata[i], DataMis2 = DateSerieNonAggregata[i + 1];
                        double DTmis = (DataMis2 - DataMis1).TotalHours; //((DataMis2).Subtract(DataMis1)).TotalHours;
                        int N = 0;
                        double DT1 = 0;
                        DateTime Data = DateSerieAggregata[i];
                        if (i < NumElementi - 1) DT = ((DateSerieAggregata[i + 1]).Subtract(Data)).TotalHours;

                        //trovo il numero N di valori di valori aggregati tra DataMis1 e DataMis2 
                        for (int j = 0; j < NumElementi; j++)
                        {
                            if ((DateSerieAggregata[j] > DataMis1) && (DateSerieAggregata[j] <= DataMis2))
                            {
                                N++;
                            }

                        }

                        if (N == 0) mis_precedente = mis_precedente + Mis2;
                        else if ((N > 0) && (DTmis > DTmax)) //CASO DI BUCO, METTO NODATA
                        {
                            int conta = 0;
                            for (int j = 0; j < NumElementi; j++)
                            {

                                if ((DateSerieAggregata[j] > DataMis1) && (DateSerieAggregata[j] <= DataMis2))
                                {

                                    //  if (conta == 0)
                                    // {

                                    //    SerieAggregata[j] = mis_precedente;
                                    //    mis_precedente = 0;
                                    //  }
                                    //  else
                                    {
                                        SerieAggregata[j] = MainProgram.ExVal;
                                        mis_precedente = 0;
                                    }
                                    conta++;
                                }

                            }

                        }
                        else if ((N > 0) && (DTmis <= DTmax))
                        {
                            int conta = 0;
                            for (int j = 0; j < NumElementi; j++)
                            {

                                if ((DateSerieAggregata[j] > DataMis1) && (DateSerieAggregata[j] <= DataMis2))
                                {

                                    if (conta == 0)
                                    {
                                        DT1 = ((DateSerieAggregata[j]).Subtract(DataMis1)).TotalHours;
                                        SerieAggregata[j] = mis_precedente + Mis2 * DT1 / DTmis;

                                        if (DT > DTmis) mis_precedente = Mis2 * (1 - (DT1) / DTmis);
                                        else mis_precedente = 0;
                                    }
                                    else
                                    {
                                        if (DT <= DTmis)
                                        {
                                            SerieAggregata[j] = Mis2 * DT / DTmis;
                                            mis_precedente = Mis2 * (1 - (DT1 + (N - 1) * DT) / DTmis);
                                        }
                                        else
                                        {
                                            SerieAggregata[j] = Mis2 * (DT - DT1) / DTmis;
                                            mis_precedente = Mis2 * (1 - (DT1 + (N - 1) * (DT - DT1)) / DTmis);
                                        }
                                    }
                                    conta++;



                                }
                            }

                            if (i == numero_misure - 2) mis_ultimo_ts = mis_precedente;
                        }

                        if (i == 0)
                        {
                            DateTime data = DateSerieAggregata[0];
                            int conta = 0;

                            while (data <= DateSerieNonAggregata[0])
                            {
                                SerieAggregata[conta] = MainProgram.ExVal;
                                conta++;
                                if (conta >= SerieAggregata.Length) break;
                                data = DateSerieAggregata[conta];
                            }
                            if (conta > 0) SerieAggregata[conta - 1] = Mis1;

                        }

                        if (i == numero_misure - 2)
                        {
                            // trovo tutte le date serie aggregata >= ultima data misurata e a tutte metto Exval, tranne prima che metto mis_precedente_ultimo ts


                            //DateTime data = DateSerieAggregata[NumElementi-1];
                            //int conta = NumElementi - 1;

                            //while ((data > DateSerieNonAggregata[numero_misure-1])&&(conta>=0))
                            //{
                            //    SerieAggregata[conta] = MainProgram.ExVal;
                            //    conta--;
                            //    if (conta < 0) break;
                            //    data = DateSerieAggregata[conta];
                            //}
                            //if (conta+1<NumElementi) SerieAggregata[conta + 1] = Mis2;


                        }
                    }
                }
            }

            catch (System.Exception e)
            {
                string exname = "Aggregatore serie temporali: Eccezione non codificata: " + e.Message;
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 6;
                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                ;
            }

        }

    }
}
