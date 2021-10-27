//#define parallelizzazione
#define ESEGUI_FILTRO_QRCODE //commentare per bypass filtro qrcode
#define ESEGUI_FILTRO_SMARTMETEO
#define ESEGUI_FILTRO_SMARTPIOGGIA

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace FiltroSociale
{
    class MainProgram
    {
        public static string formato_data_lettura_DB = "yyyyMMdd HH:mm:ss";
        public static string formato_data_scrittura_DB = "yyyyMMdd HH:mm:ss";
        public static int DEBUG = 1,
            tipodato_qrcode=17,
            IdProcesso=2000,
            ExVal=-9999; //tipodato coincidente con livello qrcode
        public static NpgsqlConnection con2 = new NpgsqlConnection();
      

        static void Main()
        {

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("us-US");
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("it-IT");
            DateTime Inizio_esecuzione = DateTime.Now;

            Console.WriteLine("Inizio filtro sociale: "+ Inizio_esecuzione);

            //Leggi Connession 
            if (DEBUG == 2)  //NO_DEBUG
            {
                Console.WriteLine("Inserisci Connection String: ");   //decommentare se lancio in debug
                con2.ConnectionString = Console.ReadLine(); 
            }
            else  //SI_DEBUG
            {
                con2.ConnectionString = "Server=10.0.0.243; Port=5432; Database=dao; User Id=DaoUsr;Password=dao";
                //con2.ConnectionString = "Server=10.0.0.243; Port=5432; Database=dao; User Id=DaoUsr;Password=dao"; //Macchina di Sviluppo
                //con2.ConnectionString = "Server=10.0.0.246; Port=5432; Database=dao; User Id=DaoUsr;Password=dao"; //Macchina del Piave (Clone Martina)
                // con2.ConnectionString = "Server=10.0.0.219; Port=5432; Database=dao; User Id=DaoUsr;Password=dao"; //Macchina con lo storico (Clone Francesca)
                //con2.ConnectionString = "Server=10.0.0.248; Port=5432; Database=dao; User Id=DaoUsr;Password=dao"; //Macchina Lemene
                //cloud
                //con2.ConnectionString = "Server=10.0.0.243; Port = 5432; Database = dao; User Id = DaoUsr; Password = dao"; //nuovo IP db workstation a Venezia
            }

            con2.ConnectionString = con2.ConnectionString + " ; Timeout=1024; CommandTimeout = 0";

            if (con2.State == System.Data.ConnectionState.Closed)
            {
                con2.Open();
            }

#if ESEGUI_FILTRO_QRCODE
            //------------------------------------ FASE1: Filtraggio livelli QRCODE
            //leggi le tabelle settings per i QRCODE   
            //determina le stazioni di misure correlate spazialmente -> array idrometri correlabili
            //determina i QRCODE correlati spazialmente -> array qrcode correlabili
            Console.WriteLine("Inizio Filtraggio misure QRCODE");

            Filtro_qrcode filtro_qrcode = new Filtro_qrcode();
            filtro_qrcode.LeggiParametriGlobali();
            filtro_qrcode.LeggiCorrelazioneStazioni(); 
            filtro_qrcode.LeggiCorrelazioniQrcode();
            filtro_qrcode.LeggiCoordQRCODE();

            Filtro_qrcode.Filtro1 filtro1 = new Filtro_qrcode.Filtro1();
            filtro1.LeggiParametriFiltro1();

            Filtro_qrcode.Filtro2 filtro2 = new Filtro_qrcode.Filtro2();
            filtro2.LeggiParametriFiltro2();

            Filtro_qrcode.Filtro3 filtro3 = new Filtro_qrcode.Filtro3();
            filtro3.LeggiParametriFiltro3();

            Filtro_qrcode.Filtro4 filtro4 = new Filtro_qrcode.Filtro4();
            filtro4.LeggiParametriFiltro4();

            Filtro_qrcode.Filtro5 filtro5 = new Filtro_qrcode.Filtro5();
            filtro5.LeggiParametriFiltro5();

            double[] misure_qrcode;
            DateTime[] date_misure_qrcode;
            int[,] metadato_misure_qrcode; //utente_id, STAZIONE ID, SENSORE ID

            //Data minore nel passato in cui cercare il dato da validare: date time now - DELTA VALIDAZIONE


           // leggi WEB.misurazione_qr_code con idstazione, valore, tipodato letto da datigrezzo con variabile_id=17
           //salva idweb per ogni dato e utente_id, STAZIONE ID, SENSORE ID
           DateTime IntervalloRicerca = Inizio_esecuzione.AddHours(-(filtro_qrcode.DT_max_validazione));

           NpgsqlCommand scpostgre = new NpgsqlCommand("select distinct * from web.misurazione_qr_code where dataora > '" +IntervalloRicerca.ToString(MainProgram.formato_data_lettura_DB)+"' AND variabile_id =" + tipodato_qrcode, con2);
           NpgsqlDataReader readerpostgre = scpostgre.ExecuteReader();

            if (!readerpostgre.HasRows)
            {
                Console.WriteLine(" non ci sono misure da qrcode da validare nell'intervallo tra " + IntervalloRicerca + " e "+ Inizio_esecuzione);
                string exname = " non ci sono misure da qrcode da validare nell'intervallo tra " + IntervalloRicerca + " e " + Inizio_esecuzione;

                GestioneEccezione ecc = new GestioneEccezione();
                int livello_severita = 4;
                ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, IdProcesso);

            }

            int ndati = 0, conta=0;

            while (readerpostgre.Read())
            {
                ndati++;
            }

            misure_qrcode = new double[ndati];
            date_misure_qrcode = new DateTime[ndati];
            metadato_misure_qrcode = new int[ndati,4]; //colonna 1 utente id, colonna 2 id sensore, colonna 3 id stazione, 4 idweb

            readerpostgre.Close();
            readerpostgre = scpostgre.ExecuteReader();
            while (readerpostgre.Read())
            {
                misure_qrcode[conta] = Convert.ToDouble(readerpostgre["valore"]);
                date_misure_qrcode[conta] = Convert.ToDateTime(readerpostgre["dataora"]);
                metadato_misure_qrcode[conta, 0]=Convert.ToInt32(readerpostgre["utente_id"]);
                metadato_misure_qrcode[conta, 1] = Convert.ToInt32(readerpostgre["sensore_id"]);
                metadato_misure_qrcode[conta, 2] = Convert.ToInt32(readerpostgre["stazione_id"]);
                metadato_misure_qrcode[conta, 3] = Convert.ToInt32(readerpostgre["id"]);
                conta++;
            }
            readerpostgre.Close();

           

#if parallelizzazione
                        Parallel.For(0, ndati, i =>
                        {
                            NpgsqlConnection connessione = new NpgsqlConnection();
                            connessione.ConnectionString = con2.ConnectionString;
                            if (connessione.State == System.Data.ConnectionState.Closed)
                            {
                                connessione.Open();
                            }
#else

            for (int i = 0; i< ndati; i++)
            {

                NpgsqlConnection connessione = MainProgram.con2;
                if (connessione.State == System.Data.ConnectionState.Closed)
                {
                    connessione.Open();
                }
#endif

                // ciclo for/ parallel for con #define per ogni dato letto

                double dato_da_validare = misure_qrcode[i];
                int utente = metadato_misure_qrcode[i, 0],
                idsensore = metadato_misure_qrcode[i, 1],
                idstazione = metadato_misure_qrcode[i, 2];
                long idweb = metadato_misure_qrcode[i, 3];
                int ruolo_utente=0; //1 DAO, 1 Cittadino Esperto, 3 Cittadino. Lo determino al filtro1
                DateTime orario_dato_da_validare = date_misure_qrcode[i];
                double affid0 = filtro_qrcode.aff_iniziale, 
                    affid1 = filtro_qrcode.aff_iniziale, 
                    affid2 = filtro_qrcode.aff_iniziale,
                    affid3 = filtro_qrcode.aff_iniziale, 
                    affid4 = filtro_qrcode.aff_iniziale,
                    affid5 = filtro_qrcode.aff_iniziale, 
                    affidfinale = filtro_qrcode.aff_iniziale;

                bool validazione_ini = true,
                    esegui_filtro1 = true,
                    esegui_filtro2 = true,
                    esegui_filtro3 = true,
                    esegui_filtro4 = true,
                    esegui_filtro5 = true,
                    esegui_filtro6 = true,
                    passaggio_filtro_finale=true;

                Console.WriteLine("FILTRAGGIO QRCODE: sto analizzando dato con  dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = " + idsensore + "and idstazione =" + idstazione);

                // cercare se il dato è già stato validato. QUERY A FILTRI.RISULTATI_FILTRAGGIO_QRCODE CON IDWEB. SE gia stato validato, vai al dato successivo, altrimenti continua il cilco
                scpostgre = new NpgsqlCommand("select distinct * from filtri.qrcode_risultati_filtraggio where dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = "+idsensore+"and idstazione ="+idstazione, con2);
              try
                {
                    readerpostgre = scpostgre.ExecuteReader();
                }
                catch (SystemException e)
                {
                   Console.WriteLine(" Anomaila nella query: select distinct * from filtri.qrcode_risultati_filtraggio where dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = " + idsensore + "and idstazione =" + idstazione );
                    string exname = " Anomaila nella query: select distinct * from filtri.qrcode_risultati_filtraggio where dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = " + idsensore + "and idstazione =" + idstazione;

                    GestioneEccezione ecc = new GestioneEccezione();
                    int livello_severita = 5;
                    ecc.ScriviEccezione(exname, DateTime.Now, livello_severita, IdProcesso);
                }

                if ((readerpostgre.HasRows)&&(!filtro_qrcode.RivalidareDatiValidati)) // se il dato già stato validato, e non lo voglio rivalidare passo al dato successivo
                {
                    Console.WriteLine("FILTRAGGIO QRCODE: Già validato il dato con  dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = " + idsensore + "and idstazione =" + idstazione);
                    readerpostgre.Close();
                    continue;
                }
                else //altrimenti faccio tutta la procedura
                {
                    if (readerpostgre.HasRows) // sto rivalidando un dato già validato. Lo cancello dal db
                        {
                        NpgsqlCommand Delete_Ris = new NpgsqlCommand("delete from filtri.qrcode_risultati_filtraggio where dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = " + idsensore + "and idstazione =" + idstazione, MainProgram.con2);
                        Delete_Ris.ExecuteNonQuery();

                    }

                    readerpostgre.Close();
                    //eventuale procedura di validazione di Carlo

                    Validazione_Base ValidazioneIniziale = new Validazione_Base();

                    validazione_ini = ValidazioneIniziale.ValidazioneBase(dato_da_validare, filtro_qrcode.Soglia_max, filtro_qrcode.Soglia_min);

                    if  (!validazione_ini)
                    {
                        esegui_filtro1 = false;
                        esegui_filtro2 = false;
                        esegui_filtro3 = false;
                        esegui_filtro4 = false;
                        esegui_filtro5 = false;
                        esegui_filtro6 = false;
                        passaggio_filtro_finale = false;

                        affid1 = filtro_qrcode.aff_min;
                        affid2 = filtro_qrcode.aff_min;
                        affid3 = filtro_qrcode.aff_min;
                        affid4 = filtro_qrcode.aff_min;
                        affid5 = filtro_qrcode.aff_min;
                        affidfinale = filtro_qrcode.aff_min;

                        Console.WriteLine("FILTRAGGIO QRCODE: Non ha passato la validazione di base il dato con  dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = " + idsensore + "and idstazione =" + idstazione);
                    }

                   
                    // filtro 1
                    if (esegui_filtro1)
                    {
                        bool saltafiltrisuccessivi = false;
                        affid1 = filtro_qrcode.EseguiFiltro1(dato_da_validare, orario_dato_da_validare, affid0, filtro1, utente, out saltafiltrisuccessivi, out ruolo_utente);
                        Console.WriteLine("FILTRAGGIO QRCODE: Passaggio al filtro 1 con affid  " + affid1 +"per il  dato con  dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = " + idsensore + "and idstazione =" + idstazione);

                        if (saltafiltrisuccessivi) // se l'utente che ha fatto la misura è  DAO, non faccio filtri successivi
                        {
                        esegui_filtro2 = false;
                        esegui_filtro3 = false;
                        esegui_filtro4 = false;
                        esegui_filtro5 = false;
                        esegui_filtro6 = false;
                        passaggio_filtro_finale = true;

                       
                        affid2 = filtro_qrcode.aff_max;
                        affid3 = filtro_qrcode.aff_max;
                        affid4 = filtro_qrcode.aff_max;
                        affid5 = filtro_qrcode.aff_max;
                        affidfinale = filtro_qrcode.aff_max;
                        }
                    }


                    //filtro 2
                    if (esegui_filtro2)
                    {
                        affid2 = filtro_qrcode.EseguiFiltro2(dato_da_validare, orario_dato_da_validare, affid1, filtro2,  ruolo_utente,idsensore, idstazione,idweb);
                        Console.WriteLine("FILTRAGGIO QRCODE: Passaggio al filtro 2 con affid  " + affid2 + "per il  dato con  dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = " + idsensore + "and idstazione =" + idstazione);

                    }
                   

                    //filtro 3
                    if (esegui_filtro3)
                    {
                        affid3 = filtro_qrcode.EseguiFiltro3(dato_da_validare, orario_dato_da_validare, affid2, filtro3, idsensore, idstazione);
                        Console.WriteLine("FILTRAGGIO QRCODE: Passaggio al filtro 3 con affid  " + affid3 + "per il  dato con  dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = " + idsensore + "and idstazione =" + idstazione);


                    }


                    //filtro 4
                    if (esegui_filtro4)
                    {
                        affid4 = filtro_qrcode.EseguiFiltro4(dato_da_validare, orario_dato_da_validare, affid3, filtro4, filtro_qrcode.correlazione_qrcode, idstazione);
                        Console.WriteLine("FILTRAGGIO QRCODE: Passaggio al filtro 3 con affid  " + affid4 + "per il  dato con  dataora = '" + orario_dato_da_validare.ToString(MainProgram.formato_data_lettura_DB) + "' AND iddatoweb =" + idweb + "and idsensore = " + idsensore + "and idstazione =" + idstazione);

                    }


                    //filtro 5
                    if (esegui_filtro5)
                    {

                    }
                    //da array sensori correlati determina le stazioni correlate alla mia asta,
                    //per la/le stazioni correlate determino serie temporale in DT correlazione dai dati misurati (LIVMIS) 
                    // 
                    // applico filtri

                    //filtro 6
                    if (esegui_filtro6)
                    {

                    }
                    // verifico sogli aff minima
                    // se dato supera affid minima salvalo su liv_QRCODE con la giusta affidabilità o neve QRCODE



                    //compila la tabella filtraggio QRCODE
                    // variabili da scrivere
                    //passaggio_filtro_finale;
                    //affid0
                    //affid1 
                    //affid2 
                    //affid3 
                    //affid4 
                    //affid5 
                    //affidfinale = filtro_qrcode.aff_min;
                    //    double dato_da_validare = misure_qrcode[i];
                    //    int utente = metadato_misure_qrcode[i, 0],
                    //idsensore = metadato_misure_qrcode[i, 1],
                    //idstazione = metadato_misure_qrcode[i, 2],
                    //idweb = metadato_misure_qrcode[i, 3];


                    //copia in livmisQRCODE 

                }

                //fine ciclo for

                connessione.Close();

#if parallelizzazione
                           


                        });
#else
            }

#endif
#endif

#if ESEGUI_FILTRO_SMARTPIOGGIA

            //------------------------------------  Filtraggio segnalazioni SMART DI PIOGGIA

            // Data minore nel passato in cui cercare il dato da validare: date time now - DELTA VALIDAZIONE
            // leggi le coordinate e gli id di tutti i pluviometri, eventualmente converti da lat long a XY
            // leggi da valore_misurazione_smart gli id corrispondenti a variabile_id=1 ed i relativi valori_mumerici (il valore di precipitazione rappresentativo della classe) -> salva su array classi di precipitazioni
            // Leggi da web misure pioggia CON DATAORA >= data minore nel passato in cui cercare dato validare e valore_ID pari ad quelli degli array classi di precipitazioni. Salvo valori e coordinate, utente id ecc

            Filtro_smart filtro_smart = new Filtro_smart();
            filtro_smart.LeggiParametriGlobali();
            filtro_smart.LeggiCoordPluviometri();
            filtro_smart.LeggiClassiPluvio();

            Filtro_smart.Filtro1 filtro1s = new Filtro_smart.Filtro1();
            filtro1s.LeggiParametriFiltro1();

            Filtro_smart.Filtro2 filtro2s = new Filtro_smart.Filtro2();
            filtro2s.LeggiParametriFiltro2();

            Filtro_smart.Filtro3 filtro3s = new Filtro_smart.Filtro3();
            filtro3s.LeggiParametriFiltro3();

           

            double[] misure_smart;
            DateTime[] date_misure_smart;
            int[,] metadato_misure_smart;
            double[,] coord_misure_smart;//utente_id, STAZIONE ID, SENSORE ID

            //ciclo for su dati trovati

            //eventuale procedura di validazione di Carlo

            // cercare se il dato è già stato validato. QUERY A FILTRI.RISULTATI_FILTRAGGIO_PIOGGIA CON IDWEB. SE gia stato validato, vai al dato successivo, altrimenti continua il cilco
            // converti le coordinate da lat long a piane
            // filtro 1
            // prendi utente_id per quel dato d
            // query a web.jhi_user_authority con utente_id -> authority_name
            // a seconda di authority_name, incrementa 
            //filtro 2
            // query a Leggi da web misure pioggia CON DATAORA comprese nel Delta dei filtro1 e valore_ID pari ad quelli degli array classi di precipitazioni. Salvo valori e coordinate, utente id ecc
            // CONVERTI COORDINATE
            //VERIFICA SE I DATI SONO NEL RAGGIO DAL MIO DATO IN ANALISi
            //modifica affidabilità
            //FILTRO 3
            //DAlla lista pluviometri verifica quali sono entro il raggio voluto dal mio pluviometro
            // fai query dati dei pluviometri vici da misure.pioggia nell'intervallo considerato
            //applica il filtro
            //filtro 6
            // verifico sogli aff minima
            //compila la tabella filtraggio SMART
            //valutare come inserire nel db il fatto che alcuni dati sono valdati o no (tipo datigrezzi o stodatigrezzi)
#endif

#if ESEGUI_FILTRO_SMARTMETEO
            //------------------------------------  Filtraggio segnalazioni SMART METEO
            // leggi le tabelle settings globali e di singoli filtri IN PARTICOLARE DELTA VALIDAZIONE
            // Data minore nel passato in cui cercare il dato da validare: date time now - DELTA VALIDAZIONE
            // leggi da valore_misurazione_smart gli id corrispondenti a variabile_id=2 ed e la descrizione del meteo associato alla misurazione -> salva su array classi meteo
            // Leggi da web misure pioggia CON DATAORA >= data minore nel passato in cui cercare dato validare e valore_ID pari ad quelli degli array classi meteo. Salvo valori e coordinate,  utente id ecc
            //ciclo for su dati trovati

            //eventuale procedura di validazione di Carlo

            // cercare se il dato è già stato validato. QUERY A FILTRI.RISULTATI_FILTRAGGIO_meteo CON IDWEB. SE gia stato validato, vai al dato successivo, altrimenti continua il cilco
            // converti le coordinate da lat long a piane
            // filtro 1
            // prendi utente_id per quel dato d
            // query a web.jhi_user_authority con utente_id -> authority_name
            // a seconda di authority_name, incrementa 
            //filtro 2
            // query a Leggi da web misure pioggia CON DATAORA comprese nel Delta dei filtro1 e valore_ID pari ad quelli degli array classi di precipitazioni. Salvo valori e coordinate, utente id ecc
            // CONVERTI COORDINATE
            //VERIFICA SE I DATI SONO NEL RAGGIO DAL MIO DATO IN ANALISi
            //filtro 6
            // verifico sogli aff minima
            //compila la tabella filtraggio METEO
            //valutare come inserire nel db il fatto che alcuni dati sono valdati o no (tipo datigrezzi o stodatigrezzi)


            //Valutare se fare i for parallel;

            //try e catch globale e gestione eccezioni
#endif
        }
    }
}
