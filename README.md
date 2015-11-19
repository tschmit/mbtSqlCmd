# mbtSqlCmd
command line tool for comparing rows from Sql Server

"mbtSqlCmd" is a command line tool for field by field row comparison. That's is, for at least two rows from the same query, the tool compares each field and exhibit the fields that are differently valued from one row to another.

The flags are, as much as possible, the same as those of [SqlCmd](https://msdn.microsoft.com/en-us/library/ms162773%28v=sql.120%29.aspx)

## Usages

Basic usage:  
`mbtSqlCmd.exe -E -S sqlInstante -d databaseName -t tableName -w "field1 = 3 and field4 in ('abc', 'def')"`

Complex usage, allowing multiple result sets:  
`mbtSqlCmd.exe -E -S sqlInstante -d databaseName -i file.sql`  
where file.sql comprises any valid slq returning at least one dataset.

You can get more help by:
- launching the app without flag,
- launching the app with -h or --help,
- reading [ComLineOptions.cs](https://github.com/tschmit/mbtSqlCmd/blob/master/mbtSqlCmd/ComLineOptions.cs)

## Various considerations

If the result sets comprises an odd number of rows the tools raises an error.

The returned rows are compared two by two:
- if the result set comprises two rows, the compared rows are obvious;
- if the result set comprises four lines or more, lines N is compared with line N + (Number_of_lines / 2). Due to this, the order of line is important.

The output of the tool displays 1 based numbers of the compared lines in the set.
