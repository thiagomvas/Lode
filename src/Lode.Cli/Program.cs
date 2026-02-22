using Lode.Drivers.AccessDb;

var provider = new LinuxMdbToolsProvider();

var file = "/home/thiagomv/Downloads/IBJH 2024 - SOFTEAM.accdb";
var tables = await provider.GetTablesAsync(file);

foreach (var table in tables)
{
    Console.WriteLine(table);
}

var schema = await provider.GetTableSchemaAsync(file, tables.First());

Console.WriteLine(schema.Name);

foreach (var column in schema.Columns)
{
    Console.WriteLine($"{column.Id} {column.Name} {column.Type} {column.Flags}");
}

var count = 0;

await foreach (var row in provider.ExportTableAsync(file, tables.First()))
{
    Console.WriteLine(string.Join(" | ", row.Values.Select(v => v?.ToString() ?? "NULL")));

    count++;

    if (count >= 25)
        break;
}