# RdbParser

### Command-line

RdbSharp.Cli

This application will take a RDB file as input and format it in the specified format.

Example:

```
$ cd src/RdbSharp.Cli
$ dotnet run ../../tests/dumps/multiple_lists_strings.rdb json
RDB Version: 11
{
  "string2": "Hi there!",
  "mylist1": [
    "v1"
  ],
  "mylist3": [
    "v3",
    "v2",
    "v1"
  ],
  "lzf_compressed": "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "string1": "blaa",
  "mylist2": [
    "v2",
    "v1"
  ]
}

