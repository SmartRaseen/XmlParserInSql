using System;

namespace XmlParseInSql
{
    public class Program 
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("XML Data Storing Process Is Starts Now!");
            XmlParser _xmlParser = new XmlParser();
            Console.WriteLine("Query Executed Successfully");
            Console.WriteLine("Data Updated..");
            Console.ReadLine();
        }
    }
}
