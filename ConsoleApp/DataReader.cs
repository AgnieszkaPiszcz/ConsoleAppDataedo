namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using CsvHelper;
    using System.Globalization;

    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;

        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            var lines = File.ReadAllLines(fileToImport);
            ImportedObjects = lines.Skip(1)
                .Select(l => l.Split(';'))
                .Select(l => new ImportedObject(l))
                .ToList();

            // clear and correct imported data
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.Type = importedObject.Type.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
                importedObject.Name = importedObject.Name.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.Schema = importedObject.Schema.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentName = importedObject.ParentName.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentType = importedObject.ParentType.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper(); ;
            }

            // assign number of children
            for (int i = 0; i < ImportedObjects.Count(); i++)
            {
                var parentObj = ImportedObjects.ElementAt(i);
                foreach (var impObj in ImportedObjects)
                {
                    if (impObj.ParentType == parentObj.Type)
                    {
                        if (impObj.ParentName == parentObj.Name)
                        {
                            parentObj.NumberOfChildren++;
                        }
                    }
                }
            }

            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                    // print all database's tables
                    foreach (var table in ImportedObjects)
                    {
                        if (table.ParentType.ToUpper() == database.Type)
                        {
                            if (table.ParentName == database.Name)
                            {
                                Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                                // print all table's columns
                                foreach (var column in ImportedObjects)
                                {
                                    if (column.ParentType.ToUpper() == table.Type)
                                    {
                                        if (column.ParentName == table.Name)
                                        {
                                            Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.ReadLine();
        }
    }

    class ImportedObject : ImportedObjectBaseClass
    {
        public string Schema;

        public string ParentName;
        public string ParentType
        {
            get; set;
        }

        public string DataType { get; set; }
        public string IsNullable { get; set; }

        public double NumberOfChildren;
        public ImportedObject(string[] line)
        {
            this.Type = line.ElementAtOrDefault(0) ?? string.Empty;
            this.Name = line.ElementAtOrDefault(1) ?? string.Empty;
            this.Schema = line.ElementAtOrDefault(2) ?? string.Empty;
            this.ParentName = line.ElementAtOrDefault(3) ?? string.Empty;
            this.ParentType = line.ElementAtOrDefault(4) ?? string.Empty;
            this.DataType = line.ElementAtOrDefault(5) ?? string.Empty;
            this.IsNullable = line.ElementAtOrDefault(6) ?? string.Empty;
        }
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
