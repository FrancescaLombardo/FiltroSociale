using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Web.UI.DataVisualization.Charting;

namespace FiltroSociale
{
   internal  class Filtro_qrcode
    {
        internal double aff_iniziale = new double(),
                    aff_soglia = new double(),
                    aff_max = new double(),
                    aff_min = new double(),
            DT_max_validazione = new double(),
            Soglia_max = new double(),
            Soglia_min = new double();
        internal bool RivalidareDatiValidati = new bool();

        internal int[,] correlazione_stazioni, correlazione_qrcode;
        internal double[,] coord_qrcode_latlon; //IDSTAZIONE,IDSENSORE,coordx,coordy,lat, lon


        internal void LeggiParametriGlobali()
        {
            if (MainProgram.con2.State == System.Data.ConnectionState.Closed)
            {
                MainProgram.con2.Open();
            }
            NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.qrcode_global_settings", MainProgram.con2);
            NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

            if (!readerpostgre.HasRows)
            {
                Console.WriteLine(" Errore: non trovati dati nella tabella filtri.qrcode_global_settings");
                string exname = " Errore: non trovati dati nella tabella filtri.qrcode_global_settings";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;
                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

            }
            int dummy=0;
            // try catch eventuali null;
            try
            {
                while (readerpostgre.Read())
                {
                    aff_iniziale = Convert.ToDouble(readerpostgre["aff_iniziale"]);
                    aff_soglia = Convert.ToDouble(readerpostgre["aff_soglia"]);
                    aff_max = Convert.ToDouble(readerpostgre["aff_max"]);
                    aff_min = Convert.ToDouble(readerpostgre["aff_min"]);
                    DT_max_validazione = Convert.ToDouble(readerpostgre["dtmax_validazione"]);
                    Soglia_max = Convert.ToDouble(readerpostgre["soglia_max"]);
                    Soglia_min = Convert.ToDouble(readerpostgre["soglia_min"]);
                    dummy = Convert.ToInt32(readerpostgre["rivalidazione"]);
                    if (dummy == 0) RivalidareDatiValidati = false;
                    else RivalidareDatiValidati = true;
                    break;
                }

                readerpostgre.Close();
            }
            catch (System.Exception e)
            {
                string exname = " anomalia lettura dati from filtri.qrcode_global_settings ";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;

                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                ;
            }

            Console.WriteLine("Correttamente letti i parametri globali filtro qrcode ");
        }

        internal void LeggiCorrelazioniQrcode()
        {

            NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.qrcode_correlabili order by id_gruppo_correlazione asc", MainProgram.con2);
            NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

            int dim = 0, conta=0;

            if (!readerpostgre.HasRows)
            {
                Console.WriteLine(" Warning: non trovata alcuna correlazione tra qrcode nella tabella filtri.qrcode_correlabili");
                string exname = " Warning: non trovata alcuna correlazione tra qrcode nella tabella filtri.qrcode_correlabili";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 4;
                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

            }

            // try catch eventuali null;
            try
            {
                while (readerpostgre.Read())
                {
                    conta++;
                }

                readerpostgre.Close();

                dim = conta;
                conta = 0;
                scpostgre.ExecuteReader();

                correlazione_qrcode = new int[dim, 2];

                while (readerpostgre.Read())
                {
                    correlazione_qrcode[conta, 0] = Convert.ToInt32(readerpostgre["id_gruppo_correlazione"]); ;
                    correlazione_qrcode[conta, 1] = Convert.ToInt32(readerpostgre["id_stazione_qrcode"]);
                    conta++;
                }

                readerpostgre.Close();
            }
            catch (System.Exception e)
            {
                string exname = " anomalia lettura dati from filtri.qrcode_correlabili ";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;

                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                ;
            }

            Console.WriteLine("Correttamente letto dalla base dati l'elenco di qrcode correlati tra loro");
        }

        internal void LeggiCorrelazioneStazioni()
        {

            NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.sensori_correlabili order by id_stazione_qrcode asc", MainProgram.con2);
            NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

            int dim = 0, conta = 0;

            if (!readerpostgre.HasRows)
            {
                Console.WriteLine(" Warning: non trovata alcuna correlazione tra qrcode e stazione nella tabella filtri.sensori_correlabili");
                string exname = " Warning: non trovata alcuna correlazione tra qrcode e stazione nella tabella filtri.sensori_correlabili";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 4;
                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

            }

            // try catch eventuali null;
            try
            {
                while (readerpostgre.Read())
                {
                    conta++;
                }

                readerpostgre.Close();

                dim = conta;
                conta = 0;
                scpostgre.ExecuteReader();

                correlazione_stazioni= new int[dim, 2];

                while (readerpostgre.Read())
                {
                    correlazione_stazioni[conta, 0] = Convert.ToInt32(readerpostgre["id_stazione_qrcode"]); ;
                    correlazione_stazioni[conta, 1] = Convert.ToInt32(readerpostgre["id_stazsens"]);
                    conta++;
                }

                readerpostgre.Close();
            }
            catch (System.Exception e)
            {
                string exname = " anomalia lettura dati from filtri.sensori_correlabili ";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;

                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                ;
            }


           
            Console.WriteLine("Correttamente letto dalla base dati l'elenco di stazioni correlate al qrcode");
        }

        internal void LeggiCoordQRCODE()
        {
            NpgsqlCommand scpostgre = new NpgsqlCommand("Select s2.idstazione ,s2.id,coordx ,coordy,lat,lon  from misure.stazioni s  join misure.stazsens s2  on s.id = s2.idstazione where s2.idtipodato =" + MainProgram.tipodato_qrcode, MainProgram.con2);
            NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();


            int dim = 0, conta = 0;

            if (!readerpostgre.HasRows)
            {
                Console.WriteLine(" Errore: non trovate stazioni qrcode nella tabella stazioni e/o stazsens");
                string exname = (" Errore: non trovate stazioni qrcode nella tabella stazioni e/o stazsens");
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;
                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

            }

            try
            {
                while (readerpostgre.Read())
                {
                    conta++;
                }

                readerpostgre.Close();

                dim = conta;
                conta = 0;
                scpostgre.ExecuteReader();

                coord_qrcode_latlon = new double[dim, 6];
  

                while (readerpostgre.Read())
                {
                    coord_qrcode_latlon[conta,0] = Convert.ToInt32(readerpostgre["idstazione"]); ;
                    coord_qrcode_latlon[conta, 1] = Convert.ToInt32(readerpostgre["id"]);
                    coord_qrcode_latlon[conta, 2] = Convert.ToDouble(readerpostgre["coordx"]);
                    coord_qrcode_latlon[conta, 3] = Convert.ToDouble(readerpostgre["coordy"]);
                    coord_qrcode_latlon[conta, 4] = Convert.ToDouble(readerpostgre["lat"]);
                    coord_qrcode_latlon[conta, 5] = Convert.ToDouble(readerpostgre["lon"]);
                    conta++;
                }

                readerpostgre.Close();
            }
            catch (System.Exception e)
            {
                string exname = " anomalia lettura dati stazioni e stazsens ";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;

                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                ;
            }



            Console.WriteLine("Correttamente letto dalla base dati le coordinate delle stazioni qrcode");
        }

        internal double EseguiFiltro1(double valore, DateTime data, double affiniziofiltro, Filtro1 filtro1, int idutente, out bool SaltaFiltriSuccessivi, out int ruoloutente)
        {
            // prendi utente_id per quel dato da web.misurazione_qr_code 
            // query a web.jhi_user_authority con utente_id -> authority_name
            // a seconda di authority_name, incrementa 
            //se utente è DAO salta filtri successivi
            SaltaFiltriSuccessivi = false;
            string tipo_utente = " ";

            try
            {
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from web.jhi_user_authority where user_id= "+idutente, MainProgram.con2);
                NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();
           

            if (!readerpostgre.HasRows)
            {
                Console.WriteLine(" Errore: non trovato categoria di utente avente utente:id="+idutente+ " nella tabella web.jhi_user_authority");
                string exname = " Errore: non trovato categoria di utente avente utente:id=" + idutente + " nella tabella web.jhi_user_authority";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;
                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

            }
            

            while (readerpostgre.Read())
            {
                tipo_utente = Convert.ToString(readerpostgre["authority_name"]);
                break;
            }

            readerpostgre.Close();
            }
            catch (System.Exception e)
            {
                string exname = " Fallita query select distinct *from web.jhi_user_authority where user_id = " + idutente;
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;

                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

            }

            double affid = affiniziofiltro;

            ruoloutente = 3;

            //lista di ruoli presa da web.jhi_authority
           


    switch (tipo_utente)
                {
                case  "ROLE_ADMIN" : ruoloutente=1; break;
                case "ROLE_USER": ruoloutente = 3; break;
                case "ROLE_DECISION_MAKER": ruoloutente = 2; break;
                case "ROLE_RESCUER": ruoloutente = 2; break;
                case "ROLE_POWER_USER": ruoloutente = 2; break;
                case "ROLE_DAO": ruoloutente = 1; break;
                case "ROLE_MANAGER": ruoloutente = 1; break;
                case "ROLE_CONSORTIUM": ruoloutente = 2; break;
                case "ROLE_TEAM_LEADER": ruoloutente = 2; break;
                case "ROLE_PROVINCE_DECISION_MAKER": ruoloutente = 2; break;
                case "ROLE_PROVINCE_RESCUER": ruoloutente = 2; break;
                case "ROLE_ACHAB": ruoloutente = 3; break;
                case "ROLE_TEST": ruoloutente = 3; break;
                case "ROLE_EXPORTWATERML": ruoloutente = 2; break;
                default:ruoloutente = 3;
                    string exname = " Utente : "+tipo_utente+" non codificato, assegnato tipo cittadino";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 4;

                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                    break;
            }


            if (ruoloutente == 1) //caso1 - utente è dao
            {
                affid = affid + filtro1.delta_aff_caso1;
                SaltaFiltriSuccessivi = true;

            }
            else if (ruoloutente == 2) //caso2 - utente è cittadino esperto
            {
 
                affid = affid  + filtro1.delta_aff_caso2;
                SaltaFiltriSuccessivi = false;

            }
            else //caso3 - altro (cittadino normale)
            {
   
                affid = affid + filtro1.delta_aff_caso3;
                SaltaFiltriSuccessivi = false;

            }

            return affid;
        }

        internal double EseguiFiltro2(double valore, DateTime data, double affiniziofiltro, Filtro2 filtro2, int ruoloutente, int id_sensore, int id_stazione, long iddato)
        {
            double affid = affiniziofiltro;
            int ndati = 0;
            //determina  il numero di dati totali (validati e non validati) in compreso in +- dt filtro2

            DateTime OraIniRicerca =data.AddMinutes(-filtro2.dt),
                OraFineRicerca = data.AddMinutes(+filtro2.dt);

            
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from web.misurazione_qr_code where dataora >= '" + OraIniRicerca.ToString(MainProgram.formato_data_lettura_DB) + "' and dataora <= '" + OraFineRicerca.ToString(MainProgram.formato_data_lettura_DB) + "' and sensore_id = " + id_sensore + "and stazione_id=" + id_stazione, MainProgram.con2);
            NpgsqlDataReader readerpostgre; 
          try
                {
                readerpostgre = scpostgre.ExecuteReader();
                while (readerpostgre.Read()) ndati++;
                readerpostgre.Close();


            }

            catch (System.Exception e)
            {
                string exname = " anomalia lettura dati da web.misurazione_qr_code durante applicazione filtro 2 qrcode";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;

                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
              
            }

            double[] dati = new double[ndati];
            long[] iddati = new long[ndati];
            int conta = 0;
 
            readerpostgre = scpostgre.ExecuteReader();
            while (readerpostgre.Read())
            {
                dati[conta] = Convert.ToDouble(readerpostgre["valore"]);
                iddati[conta] = Convert.ToInt64(readerpostgre["id"]);
                conta++ ;
            }
            readerpostgre.Close();

            //somma e calcola numero dati tot
            //applica filtro 2

            int ndatisoglia=0;

            if (ruoloutente == 2) ndatisoglia = filtro2.num_misure_esperto;
            else ndatisoglia = filtro2.num_misure_cittadino;

            //caso1
            if ((ndati < ndatisoglia) || (ndati<=1)) affid = affid + filtro2.delta_aff_caso1; //non raggiunto il target minimo di ridondanza

            else //ho la ridondanza delle misure, ora devo valutare coerenza
            {
                conta = 0;
                double media_tot1 = 0, media_tot2 = 0;
                for (int i=0;i<ndati; i++)
                {
                    media_tot1 = media_tot1 + dati[i];
                    if (iddati[i]!=iddato)
                    {
                        media_tot2 = media_tot2 + dati[i];
                        conta++;
                    }
                }

                media_tot1 = Convert.ToDouble(media_tot1 / (Convert.ToDouble(ndati)));
                media_tot2 = Convert.ToDouble(media_tot2 / (Convert.ToDouble(conta)));

                //caso2
                if ((media_tot2>media_tot1*(1+ (Convert.ToDouble(filtro2.scarto_ammissibile/100))) ||(media_tot2 < media_tot1 * (1 - (Convert.ToDouble(filtro2.scarto_ammissibile / 100)))))) affid = affid + filtro2.delta_aff_caso2; //incremento affidabilità se scarto tra misure nell'intervallo di correlazione è superiore al valore soglia
                //caso3
                else affid = affid + filtro2.delta_aff_caso3; //incremento affidabilità se scarto tra misure nell'intervallo di correlazione è superiore al valore soglia
            }

            return affid;
        }

        internal double EseguiFiltro3(double valore, DateTime data, double affiniziofiltro, Filtro3 filtro3,  int id_sensore, int id_stazione)
        {
            double affid = affiniziofiltro;

            // determina da dati validati in +- dt filtro 3

            DateTime OraIniRicerca = data.AddMinutes(-filtro3.dtmax);
               
            int ndati = 0;


            NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.qrcode_risultati_filtraggio where dataora >= '" + OraIniRicerca.ToString(MainProgram.formato_data_lettura_DB) + "' and dataora < '" + data.ToString(MainProgram.formato_data_lettura_DB) + "' " +  "and idsensore = " + id_sensore + "and idstazione =" + id_stazione, MainProgram.con2);
            NpgsqlDataReader readerpostgre;
            try
            {
                readerpostgre = scpostgre.ExecuteReader();
                while (readerpostgre.Read()) ndati++;
                readerpostgre.Close();


            }

            catch (System.Exception e)
            {
                string exname = " anomalia lettura dati da filtri.qrcode_risultati_filtraggio durante applicazione filtro 3 qrcode";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;

                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

            }

            //caso 1
            if (ndati==0) // non ci sono dati nel dtprecedente (Ossia incremento affidabilità se l'intervento temporale tra l'ultimo dato da validare è maggiore di DTMAX)
            {
                affid = affid + filtro3.delta_aff_caso1;

                return affid;
            }

            double[] dati = new double[ndati];
           
            int conta = 0;

            readerpostgre = scpostgre.ExecuteReader();
            while (readerpostgre.Read())
            {
                dati[conta] = Convert.ToDouble(readerpostgre["valore"]);
              
                conta++;
            }
            readerpostgre.Close();

            double media_DT = 0, DH=0;

            //faccio la media dei dati validati in DT
            for (int i = 0; i < ndati; i++) media_DT = media_DT + dati[i];


            //calcola  incremento
            media_DT = media_DT / ndati;
            DH = Math.Abs(media_DT-valore);

            //caso2
            if (DH>filtro3.dhmax) affid = affid + filtro3.delta_aff_caso2; //incremento affidabilità se la variazine DH tra due misure è maggiore di DHmax
            //caso3
            else affid = affid + filtro3.delta_aff_caso3; //incremento affidabilità se la variazine DH tra due misure è minore di DHmax

            return affid;
        }

        internal double EseguiFiltro4(double valore, DateTime data, double affiniziofiltro, Filtro4 filtro4, int[,] qrcode_correlati, int idstazione)
        {
   
            double affid = affiniziofiltro;
            DateTime OraIniRicerca = data.AddMinutes(-filtro4.dt_correlazione);


            int dim_max, idgruppo_max, current_id = 0,ncorrelazioni=0; ;

            dim_max = Convert.ToInt32(qrcode_correlati.Length/2);
            idgruppo_max = qrcode_correlati[dim_max-1, 0];
            bool[] trovata_stazione = new bool[idgruppo_max];


            for (int i=0; i< dim_max; i++)
            {
              if  (qrcode_correlati[i, 1]==idstazione)
                {
                    current_id = qrcode_correlati[i, 0];
                    trovata_stazione[current_id-1] = true;
                    ncorrelazioni++;
                }
            }

            if (ncorrelazioni==0) //caso1 non ci sono stazioni correlate
            {
                affid = affid + filtro4.delta_aff_caso1;
                return affid;
            }

            //n correlazioni sono i gruppi di correlazione correlati alla mia stazione. Per ciascun gruppo devo individuare ora il num stazioni
            int num_stazioni_correlate = 0;

            for (int i = 0; i < dim_max; i++)
            {
                current_id = qrcode_correlati[i, 0];

                if (trovata_stazione[current_id-1] == true)
                {
                    num_stazioni_correlate++;
                }
            }

            num_stazioni_correlate = num_stazioni_correlate - ncorrelazioni; //perchè in ogni gruppo c'è anche la mia stazione, che non voglio considerare

            int[] elenco_stazioni = new int[num_stazioni_correlate];

            num_stazioni_correlate = 0;

            for (int i = 0; i < dim_max; i++)
            {
                current_id = qrcode_correlati[i, 0];

                if ((trovata_stazione[current_id-1] == true)&&(qrcode_correlati[i, 1] != idstazione))
                {
                    elenco_stazioni[num_stazioni_correlate] = qrcode_correlati[i, 1];
                    num_stazioni_correlate++;
                }
            }

            double[] coeff_correlazione = new double[num_stazioni_correlate];

            //per la mia stazione determino serie temporale in DT correlazione dai dati misurati (LIV QRCODE) + aggiungo alla serie il dato grezzo in analisi

            NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from misure.livqrcode where dataora >= '" + OraIniRicerca.ToString(MainProgram.formato_data_lettura_DB) + "' and dataora < '" + data.ToString(MainProgram.formato_data_lettura_DB) + "' " + "and idstazione =" + idstazione + "order by dataora asc", MainProgram.con2);
            NpgsqlDataReader readerpostgre;

           int ndati = 0;

            try
            {
                readerpostgre = scpostgre.ExecuteReader();
                while (readerpostgre.Read()) ndati++;
                readerpostgre.Close();


            }

            catch (System.Exception e)
            {
            
                string exname = " anomalia lettura dati da misure.livqrcode durante applicazione filtro 4 qrcode";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;

                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

            }
            readerpostgre = scpostgre.ExecuteReader();

            double[] serie_qrcode_da_validare = new double[ndati+1];
            DateTime[] date_serie_qrcode_da_validare = new DateTime[ndati + 1];
            ndati = 0;
            while (readerpostgre.Read())
            {
                serie_qrcode_da_validare[ndati] = Convert.ToDouble(readerpostgre["valore"]);
                date_serie_qrcode_da_validare[ndati] = Convert.ToDateTime(readerpostgre["dataora"]);
                ndati++;
            }


            readerpostgre.Close();
            serie_qrcode_da_validare[ndati] = valore; //aggiungo alla serie già validata la misura da validare
            date_serie_qrcode_da_validare[ndati] = data;

            double Rhomedio = 0;

            for (int i=0; i< num_stazioni_correlate;i ++)
            {
                int idstazionecorrelata = elenco_stazioni[i];
                 scpostgre = new NpgsqlCommand("select distinct * from misure.livqrcode where dataora >= '" + OraIniRicerca.ToString(MainProgram.formato_data_lettura_DB) + "' and dataora < '" + data.ToString(MainProgram.formato_data_lettura_DB) + "' " + "and idstazione =" + idstazionecorrelata + "order by dataora asc", MainProgram.con2);
             

                ndati = 0;

                try
                {
                    readerpostgre = scpostgre.ExecuteReader();
                    while (readerpostgre.Read()) ndati++;
                    readerpostgre.Close();


                }

                catch (System.Exception e)
                {
                    string exname = " anomalia lettura dati da misure.livqrcode per la stazione "+ idstazionecorrelata + "durante applicazione filtro 4 qrcode";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;
                    readerpostgre.Close();
                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

                }
                readerpostgre = scpostgre.ExecuteReader();

                double[] serie_qrcode_correlata = new double[ndati];
                DateTime[] date_serie_qrcode_correlata = new DateTime[ndati];
                ndati = 0;
                while (readerpostgre.Read())
                {
                    serie_qrcode_correlata[ndati] = Convert.ToDouble(readerpostgre["valore"]);
                    date_serie_qrcode_correlata[ndati] = Convert.ToDateTime(readerpostgre["dataora"]);
                    ndati++;
                }
                readerpostgre.Close();
                int num1 = serie_qrcode_correlata.Length, num2 = serie_qrcode_da_validare.Length;


               // verifica che entrambe le serie abbiano almeno 2 elementi se non ritorna a caso 1;
               if ((num1<2)||(num2<2))
                {
                    affid = affid + filtro4.delta_aff_caso1;
                    return affid;
                }

                geometria geo = new geometria();

                double[] serie_qrcode_correlata_aggregata = new double[num2];
                DateTime[] date_serie_qrcode_correlata_aggregata = new DateTime[num2];

                geo.AggregazioneSerieTemporaliAdattato(serie_qrcode_correlata, date_serie_qrcode_correlata,true, out serie_qrcode_correlata_aggregata, out  date_serie_qrcode_correlata_aggregata, Convert.ToDouble(filtro4.dt_correlazione/(60.0)), date_serie_qrcode_da_validare);

       

                // calcolo_coeff_correlazione serie_qrcode_correlata e serie_qrcode_da_validare;
                double Rho;
                var Chart1 = new Chart();
                Chart1.Series.Add("Serie1");
                Chart1.Series.Add("Serie2");
                int nval = 0;

                for (int j=0;j< serie_qrcode_da_validare.Length; j++)
                {
                    if ((serie_qrcode_correlata_aggregata[j] == MainProgram.ExVal) || (serie_qrcode_da_validare[j] == MainProgram.ExVal)) continue;
                  
                    Chart1.Series["Serie1"].Points.AddY(serie_qrcode_correlata_aggregata[j]);
                    Chart1.Series["Serie2"].Points.AddY(serie_qrcode_da_validare[j]);
                    nval++;

                }

                if (nval==0) //impossibile correlare le serie (l'aggregatore ha dato tutti -9999)
                {
                    affid = affid + filtro4.delta_aff_caso1;
                    return affid;
                }
                Rho = Chart1.DataManipulator.Statistics.Correlation("Serie1", "Serie2");
                
                Rhomedio = Rhomedio + Rho;


            }

            //in caso di più stazioni calcolo media coeff correlazione
            Rhomedio = Convert.ToDouble( Rhomedio / Convert.ToDouble(num_stazioni_correlate));


            if (Math.Abs(Rhomedio) < filtro4.rho_min) //caso2
            {
                affid = affid + filtro4.delta_aff_caso2;
                return affid;
            }
            else if ((Math.Abs(Rhomedio) >= filtro4.rho_min) && (Rhomedio < filtro4.rho_max)) //caso3
            {
                affid = affid + filtro4.delta_aff_caso3;
                return affid;
            }
            else if ((Math.Abs(Rhomedio) >= filtro4.rho_max)) //caso 4
            {
                affid = affid + filtro4.delta_aff_caso4;
                return affid;
            }

           
        
            return affid;
        }

        internal double EseguiFiltro5(double valore, DateTime data, double affiniziofiltro, Filtro5 filtro5)
        {
            double affid = affiniziofiltro;
            return affid;
        }

        internal void EseguiFiltro6(double valore, DateTime data, double affiniziofiltro)
        {
           
           
        }

        internal class Filtro1
        {

            internal double delta_aff_caso1,
                            delta_aff_caso2,
                            delta_aff_caso3;

            internal void LeggiParametriFiltro1 ()
            {
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.qrcode_filtro1_settings", MainProgram.con2);
                NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

                if (!readerpostgre.HasRows)
                {
                    Console.WriteLine(" Errore: non trovati dati nella tabella filtri.qrcode_filtro1_settings");
                    string exname = " Errore: non trovati dati nella tabella filtri.qrcode_filtro1_settings";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;
                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

                }

                // try catch eventuali null;
                try
                {
                    while (readerpostgre.Read())
                    {
                        delta_aff_caso1 = Convert.ToDouble(readerpostgre["delta_aff_caso1"]);
                        delta_aff_caso2 = Convert.ToDouble(readerpostgre["delta_aff_caso2"]);
                        delta_aff_caso3 = Convert.ToDouble(readerpostgre["delta_aff_caso3"]);
                        break;
                    }

                    readerpostgre.Close();
                }
                catch (System.Exception e)
                {
                    string exname = " anomalia lettura dati from filtri.qrcode_filtro1_settings ";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;

                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                    ;
                }

                Console.WriteLine("Correttamente letti i parametri  filtro1 qrcode ");

            }





        }
       internal class Filtro2
        {
            internal double delta_aff_caso1,
                            delta_aff_caso2,
                            delta_aff_caso3,
                dt,
                scarto_ammissibile;
            internal int num_misure_esperto,
                        num_misure_cittadino;

            internal void LeggiParametriFiltro2()
            {
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.qrcode_filtro2_settings", MainProgram.con2);
                NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

                if (!readerpostgre.HasRows)
                {
                    Console.WriteLine(" Errore: non trovati dati nella tabella filtri.qrcode_filtro2_settings");
                    string exname = " Errore: non trovati dati nella tabella filtri.qrcode_filtro2_settings";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;
                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

                }

                // try catch eventuali null;
                try
                {
                    while (readerpostgre.Read())
                    {
                        delta_aff_caso1 = Convert.ToDouble(readerpostgre["delta_aff_caso1"]);
                        delta_aff_caso2 = Convert.ToDouble(readerpostgre["delta_aff_caso2"]);
                        delta_aff_caso3 = Convert.ToDouble(readerpostgre["delta_aff_caso3"]);
                        dt = Convert.ToDouble(readerpostgre["dt"]);
                        scarto_ammissibile = Convert.ToDouble(readerpostgre["scarto_ammissibile"]); 
                        num_misure_esperto = Convert.ToInt32(readerpostgre["num_misure_esperto"]); ;
                        num_misure_cittadino= Convert.ToInt32(readerpostgre["num_misure_esperto"]);
                        
                        break;
                    }

                    readerpostgre.Close();
                }
                catch (System.Exception e)
                {
                    string exname = " anomalia lettura dati from filtri.qrcode_filtro2_settings ";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;

                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                    ;
                }

                Console.WriteLine("Correttamente letti i parametri  filtro2 qrcode ");
            }



        }

        internal class Filtro3
        {

            internal double delta_aff_caso1,
                            delta_aff_caso2,
                            delta_aff_caso3,
                            dtmax,
                            dhmax;

            internal void LeggiParametriFiltro3()
            {
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.qrcode_filtro3_settings", MainProgram.con2);
                NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

                if (!readerpostgre.HasRows)
                {
                    Console.WriteLine(" Errore: non trovati dati nella tabella filtri.qrcode_filtro3_settings");
                    string exname = " Errore: non trovati dati nella tabella filtri.qrcode_filtro3_settings";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;
                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

                }

                // try catch eventuali null;
                try
                {
                    while (readerpostgre.Read())
                    {
                        delta_aff_caso1 = Convert.ToDouble(readerpostgre["delta_aff_caso1"]);
                        delta_aff_caso2 = Convert.ToDouble(readerpostgre["delta_aff_caso2"]);
                        delta_aff_caso3 = Convert.ToDouble(readerpostgre["delta_aff_caso3"]);
                        dtmax = Convert.ToDouble(readerpostgre["dtmax"]);
                        dhmax = Convert.ToDouble(readerpostgre["dhmax"]);
                        
                        break;
                    }

                    readerpostgre.Close();
                }
                catch (System.Exception e)
                {
                    string exname = " anomalia lettura dati from filtri.qrcode_filtro3_settings ";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;

                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                    ;
                }

                Console.WriteLine("Correttamente letti i parametri  filtro3 qrcode ");
            }

        }

   

       internal class Filtro4
        {
            internal double delta_aff_caso1,
                            delta_aff_caso2,
                            delta_aff_caso3,
                            dt_correlazione,
                            rho_min,
                            rho_max,
                            delta_aff_caso4;
            internal void LeggiParametriFiltro4()
            {
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.qrcode_filtro4_settings", MainProgram.con2);
                NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

                if (!readerpostgre.HasRows)
                {
                    Console.WriteLine(" Errore: non trovati dati nella tabella filtri.qrcode_filtro4_settings");
                    string exname = " Errore: non trovati dati nella tabella filtri.qrcode_filtro4_settings";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;
                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

                }

                // try catch eventuali null;
                try
                {
                    while (readerpostgre.Read())
                    {
                        delta_aff_caso1 = Convert.ToDouble(readerpostgre["delta_aff_caso1"]);
                        delta_aff_caso2 = Convert.ToDouble(readerpostgre["delta_aff_caso2"]);
                        delta_aff_caso3 = Convert.ToDouble(readerpostgre["delta_aff_caso3"]);
                        delta_aff_caso4 = Convert.ToDouble(readerpostgre["delta_aff_caso4"]);
                        dt_correlazione = Convert.ToDouble(readerpostgre["dt_correlazione"]);
                        rho_min = Convert.ToDouble(readerpostgre["rho_min"]);
                        rho_max =  Convert.ToDouble(readerpostgre["rho_max"]);
                        break;
                    }

                    readerpostgre.Close();
                }
                catch (System.Exception e)
                {
                    string exname = " anomalia lettura dati from filtri.qrcode_filtro4_settings ";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;

                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                    ;
                }

                Console.WriteLine("Correttamente letti i parametri  filtro4 qrcode ");
            }
        }




        internal class Filtro5
        {
            internal double delta_aff_caso1,
                       delta_aff_caso2,
                       delta_aff_caso3,
                       dtmax,
                       dhmax;
            internal void LeggiParametriFiltro5()
            {
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.qrcode_filtro5_settings", MainProgram.con2);
                NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

                if (!readerpostgre.HasRows)
                {
                    Console.WriteLine(" Errore: non trovati dati nella tabella filtri.qrcode_filtro5_settings");
                    string exname = " Errore: non trovati dati nella tabella filtri.qrcode_filtro5_settings";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;
                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

                }

                // try catch eventuali null;
                try
                {
                    while (readerpostgre.Read())
                    {
                        delta_aff_caso1 = Convert.ToDouble(readerpostgre["delta_aff_caso1"]);
                        delta_aff_caso2 = Convert.ToDouble(readerpostgre["delta_aff_caso2"]);
                        delta_aff_caso3 = Convert.ToDouble(readerpostgre["delta_aff_caso3"]);
                        dtmax = Convert.ToDouble(readerpostgre["dtmax"]);
                        dhmax = Convert.ToDouble(readerpostgre["dhmax"]);
                        break;
                    }

                    readerpostgre.Close();
                }
                catch (System.Exception e)
                {
                    string exname = " anomalia lettura dati from filtri.qrcode_filtro5_settings ";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;

                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                    ;
                }

                Console.WriteLine("Correttamente letti i parametri  filtro5 qrcode ");
            }
        }

       

    }
}
