using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace BrokerInsightsCodeTest;

public static class MainScript
{
    public static void Main(string[] args)
    {
        // IMPORTANT: CHANGE THESE FILE PATHS TO THE CSV DATA USED FOR THIS CODE TEST
        string filePath1 = "C:\\Users\\seanborg_changingday\\Downloads\\broker1.csv";
        string filePath2 = "C:\\Users\\seanborg_changingday\\Downloads\\broker2.csv";
        
        // IMPORTANT: Change this output path to where you want the data to be outputted to
        string outputFilePath = "C:\\Users\\seanborg_changingday\\Downloads\\output.csv";

        var dataFromInputFile1 = ReadCsv(filePath1);
        var dataFromInputFile2 = ReadCsv(filePath2);

        var mergedData = MergeData(dataFromInputFile1, dataFromInputFile2);

        // Output merged data to a new CSV file
        Console.Clear();
        WriteMergedDataToCsv(mergedData, outputFilePath);
        Console.WriteLine($"Merged data written to {outputFilePath}");
        BasicReporting(outputFilePath);
    }

    static List<dynamic> ReadCsv(string filePath)
    {
        var records = new List<dynamic>();
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            records = csv.GetRecords<dynamic>().ToList();
        }
        return records;
    }

    static List<dynamic> MergeData(List<dynamic> data1, List<dynamic> data2)
    {
        var mergedData = data1.Concat(data2).ToList();

        return mergedData;
    }

    static void WriteMergedDataToCsv(List<dynamic> data, string outputPath)
    {
        using (var writer = new StreamWriter(outputPath))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            csv.WriteRecords(data);
        }
    }

    #region Basic Reporting

    static void BasicReporting(string outputtedDataFilePath)
    {
        // Display total count of policies
        CountPolicies(outputtedDataFilePath);

        // Display total count of Customers
        CountCustomers(outputtedDataFilePath);

        // Display sum of insured amounts
        CalculateInsuredAmount(outputtedDataFilePath);

        // Display average policy duration (in days)
        CalculateAveragePolicy(outputtedDataFilePath);
    }

    static void CountPolicies(string outputtedDataFilePath)
    {
        // Start at -1 to not count the row containing headers
        int count = -1;
        
        using (var reader = new StreamReader(outputtedDataFilePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            while (csv.Read())
            {
                count++;
            }
        }

        Console.WriteLine("Total Policies: " + count);
    }

    static void CountCustomers(string outputtedDataFilePath)
    {
        try
        {
            using (var reader = new StreamReader(outputtedDataFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Reading all records from the CSV file
                IEnumerable<dynamic> records = csv.GetRecords<dynamic>();

                // Counting unique customers based on the Insurer Column
                int uniqueCustomerCount = records.GroupBy(field => field.Insurer).Count();

                Console.WriteLine("Total number of customers: " + uniqueCustomerCount);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    
    static void CalculateInsuredAmount(string outputtedDataFilePath)
    {
        try
        {
            using (var reader = new StreamReader(outputtedDataFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Reading all records from the CSV file
                IEnumerable<dynamic> records = csv.GetRecords<dynamic>();

                // Calculating the total cost
                int totalCost = records.Sum(field => Convert.ToInt32(field.InsuredAmount));

                Console.WriteLine("Sum of insured amounts: " + totalCost);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static List<DateTime> GetStartDates(string outputtedDataFilePath)
    {
        try
        {
            using (var reader = new StreamReader(outputtedDataFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Reading all records from the CSV file
                IEnumerable<dynamic> StartDateRecords = csv.GetRecords<dynamic>();

                List<dynamic> dynamicStartDates = new List<dynamic>();
                
                // Collecting start dates
                dynamicStartDates = StartDateRecords.Select(field => field.StartDate).ToList();

                List<DateTime> startDates = new List<DateTime>();

                for (int i = 0; i < dynamicStartDates.Count; i++)
                {
                    // With more time I could check if the date is invalid rather than hard code this check in
                    if (dynamicStartDates[i] == "Not Known") continue;
                    
                    startDates.Add(DateTime.Parse(dynamicStartDates[i]));

                }

                return startDates;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }

    static List<DateTime> GetEndDates(string outputtedDataFilePath)
    {
        try
        {
            using (var reader = new StreamReader(outputtedDataFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Reading all records from the CSV file
                IEnumerable<dynamic> EndDateRecords = csv.GetRecords<dynamic>();

                List<dynamic> dynamicEndDates = new List<dynamic>();
                
                // Collecting start dates
                dynamicEndDates = EndDateRecords.Select(field => field.EndDate).ToList();

                List<DateTime> endDates = new List<DateTime>();

                for (int i = 0; i < dynamicEndDates.Count; i++)
                {
                    // With more time I could check if the date is invalid rather than hard code this check in
                    if (dynamicEndDates[i] == "Not Known") continue;
                    
                    endDates.Add(DateTime.Parse(dynamicEndDates[i]));

                }

                return endDates;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }
    
    static void CalculateAveragePolicy(string outputtedDataFilePath)
    {
        List<DateTime> startDates = GetStartDates(outputtedDataFilePath);
        List<DateTime> endDates = GetEndDates(outputtedDataFilePath);

        int totalPolicies = 0;
        double policyDurationInDays = 0;
        
        for (int i = 0; i < startDates.Count; i++)
        {
            DateTime startDate = startDates[i];
            DateTime endDate = endDates[i];

            totalPolicies++;

            TimeSpan timeSpan = endDate - startDate;
            policyDurationInDays += timeSpan.Days;
        }

        Console.WriteLine("Policy Average: " + policyDurationInDays / totalPolicies + " days");
    }

    #endregion
}