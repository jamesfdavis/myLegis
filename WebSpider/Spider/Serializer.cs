using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace myLegis.Spider
{
    [Serializable]
    public class Sources
    {
        public Uri AbsoluteUri { get; set; }
        public UriType ContentType { get; set; }
        public Sources() { }
    }

    public static class Serializer
    {
        
        public static List<Sources> DeSerialize(String dataDir, String fileName)
        {

            List<Sources> collection = null;
            // Open the file containing the data that you want to deserialize.
            FileStream fs = new FileStream(String.Format(dataDir + "{0}", fileName), FileMode.OpenOrCreate);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                // assign the reference to the local variable.
                collection = (List<Sources>)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
            return collection;

        }

        public static void Searialize(String dataDir, String fileName, List<Sources> collection)
        {

            FileStream fs = new FileStream(String.Format(dataDir + "{0}", fileName), FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, collection);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }

        }
    
    }
}
