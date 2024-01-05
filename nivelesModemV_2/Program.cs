using System;
using System.Collections.Generic;
using SnmpSharpNet;

namespace nivelesModemV_2
{
    internal class Program
    {
        private const int BuckMod = 5; // Ajusta este valor según tus necesidades

        private static List<Oid> OIDsDSrx = new List<Oid>
        {
            new Oid("1.3.6.1.2.1.10.127.1.1.1.1.6")
        };

        private static List<Oid> OIDsDSsnr = new List<Oid>
        {
            new Oid("1.3.6.1.2.1.10.127.1.1.4.1.5")
        };

        private static List<Oid> OIDsDSFreq = new List<Oid>
        {
            new Oid("1.3.6.1.2.1.10.127.1.1.1.1.2")
        };

        private static void Main(string[] args) 
        {
            Console.WriteLine("Cargando niveles...");

            List<string> DSrx = SnmpBulk(OIDsDSrx, BuckMod); // Ejecutando snmpbulk para Rx Downstream
            List<string> DSsnr = SnmpBulk(OIDsDSsnr, BuckMod); // Ejecutando snmpbulk para snr Downstream
            List<string> DSfreq = SnmpBulk(OIDsDSFreq, BuckMod); // Ejecutando snmpbulk para frecuencia Downstream

            Console.WriteLine("*************************************");
            Console.WriteLine("Niveles Downstream");
            Console.WriteLine("*************************************");
            Console.WriteLine("Niveles\tSNR\t\tFreq\n");

            // Procesar la información y extraer los valores específicos
            for (int i = 0; i < Math.Min(DSrx.Count, Math.Min(DSsnr.Count, DSfreq.Count)); i++)
            {
                string[] rxValues = DSrx[i].Split('\t');
                string[] snrValues = DSsnr[i].Split('\t');
                string[] freqValues = DSfreq[i].Split('\t');

                // Convertir la cadena a números y formatearlos como float

                float rxFloat = 0;
                float snrFloat = 0;
                int freqFloat = 0;

                if (float.TryParse(rxValues[0], out rxFloat))
                {
                    rxFloat /= 10;
                }

                if (float.TryParse(snrValues[0], out snrFloat))
                {
                    snrFloat /= 10;
                }

                if (int.TryParse(freqValues[0], out freqFloat))
                {
                    // No necesitas dividir por 10 aquí porque no hay un decimal implícito en el resultado.
                }

                Console.WriteLine($"{rxFloat}\t{snrFloat}\t\t{freqFloat}");
            }

        }

        private static List<string> SnmpBulk(List<Oid> oids, int buckMod)
        {
            List<string> results = new List<string>();

            string targetAddress = "10.212.46.183"; // Reemplaza con la dirección IP del dispositivo SNMP
            string community = "stechs"; // Reemplaza con tu cadena de comunidad SNMP

            // Configurar el objeto de solicitud SNMP
            SimpleSnmp snmp = new SimpleSnmp(targetAddress, community);

            if (snmp.Valid)
            {
                // Crear un objeto Pdu para la solicitud SNMP
                Pdu pdu = new Pdu(PduType.GetBulk);
                pdu.NonRepeaters = 0;
                pdu.MaxRepetitions = buckMod;

                // Agregar los OIDs al Pdu
                foreach (var oid in oids)
                {
                    pdu.VbList.Add(oid);
                }

                // Realizar la solicitud SNMP
                Dictionary<Oid, AsnType> result = snmp.GetBulk(pdu);

                // Verificar si la consulta fue exitosa
                if (result != null && result.Count > 0)
                {
                    foreach (var entry in result)
                    {
                        // Agregar el resultado a la lista
                        results.Add(entry.Value.ToString());
                    }
                }
                else
                {
                    // Manejar el caso de error o resultado vacío según sea necesario
                    results.Add("Error obteniendo los valores para la OID.");
                }
            }
            else
            {
                // Manejar el caso de conexión SNMP no válida
                results.Add("No se pudo establecer una conexión SNMP.");
            }

            return results;
        }

    }
}
