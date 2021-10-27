using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace FiltroSociale
{
    class Filtro_smart
    {
        internal double aff_iniziale = new double(),
                  aff_soglia = new double(),
                  aff_max = new double(),
                  aff_min = new double(),
          DT_max_validazione = new double();
        internal bool RivalidareDatiValidati = new bool();

        internal void LeggiParametriGlobali()
        {
            if (MainProgram.con2.State == System.Data.ConnectionState.Closed)
            {
                MainProgram.con2.Open();
            }

            NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.smart_global_settings", MainProgram.con2);
            NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

            if (!readerpostgre.HasRows)
            {
                Console.WriteLine(" Errore: non trovati dati nella tabella filtri.smart_global_settings");
                string exname = " Errore: non trovati dati nella tabella filtri.smart_global_settings";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;
                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);

            }

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
                int dummy = Convert.ToInt32(readerpostgre["rivalidazione"]);
                    if (dummy == 0) RivalidareDatiValidati = false;
                    else RivalidareDatiValidati = true;
                break;
                    
                }
                }
            catch (System.Exception e)
            {
                string exname = " anomalia lettura dati from filtri.smart_global_settings ";
                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 5;

                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                ;
            }
            readerpostgre.Close();
        }

        internal void LeggiCoordPluviometri()
        {
        }

        internal void LeggiClassiPluvio()
        {
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
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from web.jhi_user_authority where user_id= " + idutente, MainProgram.con2);
                NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();


                if (!readerpostgre.HasRows)
                {
                    Console.WriteLine(" Errore: non trovato categoria di utente avente utente:id=" + idutente + " nella tabella web.jhi_user_authority");
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
                case "ROLE_ADMIN": ruoloutente = 1; break;
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
                default:
                    ruoloutente = 3;
                    string exname = " Utente : " + tipo_utente + " non codificato, assegnato tipo cittadino";
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

                affid = affid + filtro1.delta_aff_caso2;
                SaltaFiltriSuccessivi = false;

            }
            else //caso3 - altro (cittadino normale)
            {

                affid = affid + filtro1.delta_aff_caso3;
                SaltaFiltriSuccessivi = false;

            }

            return affid;
        }


        internal double EseguiFiltro2(double valore, DateTime data, double affiniziofiltro, Filtro2 filtro2)
        {
            double affid = affiniziofiltro;
            return affid;
        }

        internal double EseguiFiltro3(double valore, DateTime data, double affiniziofiltro, Filtro3 filtro3)
        {
            double affid = affiniziofiltro;
            return affid;
        }

        internal void EseguiFiltro4(double valore, DateTime data, double affiniziofiltro)
        {
            double affid = affiniziofiltro;
           
        }

        

        internal class Filtro1
        {

            internal double delta_aff_caso1,
                   delta_aff_caso2,
                   delta_aff_caso3;

            internal void LeggiParametriFiltro1()
            {
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.smart_filtro1_settings", MainProgram.con2);
                NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

                if (!readerpostgre.HasRows)
                {
                    Console.WriteLine(" Errore: non trovati dati nella tabella filtri.smart_filtro1_settings");
                    string exname = " Errore: non trovati dati nella tabella filtri.smart_filtro1_settings";
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
                    string exname = " anomalia lettura dati from filtri.smart_filtro1_settings ";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;

                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                    ;
                }

                Console.WriteLine("Correttamente letti i parametri  filtro1 smart ");
            }





        }
        internal class Filtro2
        {
            internal double delta_aff_caso1,
                   delta_aff_caso2,
                   delta_aff_caso3,
                   dt,
                   raggio;
            internal int num_misure_esperto,
                    num_misure_cittadino;

            internal void LeggiParametriFiltro2()
            {
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.smart_filtro2_settings", MainProgram.con2);
                NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

                if (!readerpostgre.HasRows)
                {
                    Console.WriteLine(" Errore: non trovati dati nella tabella filtri.smart_filtro2_settings");
                    string exname = " Errore: non trovati dati nella tabella filtri.smart_filtro2_settings";
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
                        raggio = Convert.ToDouble(readerpostgre["raggio"]);
                        num_misure_esperto = Convert.ToInt32(readerpostgre["num_misure_esperto"]);
                        num_misure_cittadino = Convert.ToInt32(readerpostgre["num_misure_cittadino"]);
                        break;
                    }

                    readerpostgre.Close();
                }
                catch (System.Exception e)
                {
                    string exname = " anomalia lettura dati from filtri.smart_filtro2_settings ";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;

                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                    ;
                }

                Console.WriteLine("Correttamente letti i parametri  filtro2 smart ");
            }



        }

        internal class Filtro3
        {

            internal double delta_aff_caso1,
                            delta_aff_caso2,
                            delta_aff_caso3,
                            dt,
                            raggio;

            internal void LeggiParametriFiltro3()
            {
                NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from filtri.smart_filtro3_settings", MainProgram.con2);
                NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

                if (!readerpostgre.HasRows)
                {
                    Console.WriteLine(" Errore: non trovati dati nella tabella filtri.smart_filtro3_settings");
                    string exname = " Errore: non trovati dati nella tabella filtri.smart_filtro3_settings";
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
                        raggio = Convert.ToDouble(readerpostgre["raggio"]);
                     
                        break;
                    }

                    readerpostgre.Close();
                }
                catch (System.Exception e)
                {
                    string exname = " anomalia lettura dati from filtri.smart_filtro3_settings ";
                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;

                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, MainProgram.IdProcesso);
                    ;
                }

                Console.WriteLine("Correttamente letti i parametri  filtro3 smart ");
            }

        }

        }
    }

