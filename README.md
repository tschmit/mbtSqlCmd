# mbtSqlCmd

[![Build status](https://ci.appveyor.com/api/projects/status/32e64n6n1oa70s1w?svg=true)](https://ci.appveyor.com/project/tschmit/mbtsqlcmd)

Command line tool for comparing rows from Sql Server.

"mbtSqlCmd" is a command line tool for field by field row comparison. That's is, for at least two rows from the same query, the tool compares each field and exhibit the fields that are differently valued from one row to another.

The flags are, as much as possible, the same as those of [SqlCmd](https://msdn.microsoft.com/en-us/library/ms162773%28v=sql.120%29.aspx).

Main/Default tool, that is LineComp, supports MySql connections.

Secondary tool SearchFields support MySql connection in an experimental status.

## Usages

### Basic usage  

`mbtSqlCmd.exe -E -S sqlInstante -d databaseName -t tableName -w "field1 = 3 and field4 in ('abc', 'def')"`

Warning: for linux users, if the sqlInstance is based on an instance name you must escape the \ character. That is:

`-S server\\instanceName`

### Complex usage

#### multiple result sets

`mbtSqlCmd.exe -E -S sqlInstante -d databaseName -i file.sql`  
where file.sql comprises any valid slq returning at least one dataset.

#### control considered fields

This can be achieved with the following options:
- incFields: a regular expression for selecting which fields, and only which, must be considered.
- excFields: a regular expression for selecting which fields must be ignored.

Inclusion is evaluated before exclusion.

### More

#### other tool

##### SearchFields

`mbtSqlCmd.exe --tool SearchFields -E -S sqlInstance -d databaseName -w someSqlMask`

to find which field(s) of which table(s) match the sqlMask. The sqltype of the column must be 'castable' as a nvarchar.

#### ultimately

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
