# GoogleMapExport2KML

Parses .csv files generated from a google maps export of Saved Places. This fills the gpa of allowing you to import valid KML files from google maps into other mapping applications.

## Usage


GoogleMapExport2KML.exe parse [OPTIONS]

#### Example Commands

```powershell
GoogleMapExport2KML.exe parse -f=C:\downloads\myplaces.csv -f=C:\downloads\myfavoriteplaces.csv
-o=MyCombinedPlaces.kml
```

```powershell
# This will output an estimated time that it will take to complete the conversion
GoogleMapExport2KML.exe parse -f=C:\downloads\myplaces.csv -f=C:\downloads\myfavoriteplaces.csv
-o=MyCombinedPlaces.kml --dryrun
```

```powershell
# Creates multiple output files with 500 placemarks per file. This is useful for mapping
# applications that limit the amount of placemarks that can be imported at 1 time
GoogleMapExport2KML.exe parse -f=C:\downloads\myplaces.csv -f=C:\downloads\myfavoriteplaces.csv
-o=MyCombinedPlaces.kml -c=500
```

```powershell
GoogleMapExport2KML.exe parse -f=C:\downloads\myplaces.csv -f=C:\downloads\myfavoriteplaces.csv
-o=MyCombinedPlaces.kml
```

#### Options

| Short Command | Long Command      | Description                                                                                                    |
| ------------- | ----------------- | -------------------------------------------------------------------------------------------------------------- |
| -h,           | --help            | Prints help information                                                                                        |
| -v,           | --version         | Prints version information                                                                                     |
|               | --noheader        | If true. Does not display the banner on command execute                                                        |
| -f,           | --file            | The csv files to parse                                                                                         |
|               | --includeComments | If true. Adds any comment from the csv column to the description                                               |
| -v,           | --verbose         | If true. Increases the level of the output                                                                     |
| -s,           | --stats           | If true. Outputs all the timing stats                                                                          |
| -o,           | --output          | The output KML file                                                                                            |
| -t,           | --timeout         | The timeout to wait on each lookup for coordinates from Google. Default 10s                                    |
| -p,           | --parallel        | The number of threads used to process Google data locations. Default 4                                         |
|               | --stopOnError     | If true. Stops parsing on any csv row error                                                                    |
|               | --dryrun          | If true. Runs through the files and estimates times to completion                                              |
| -c,           | --chunks          | The number of placements to add per KML file. Files will be named based on number of files needed. Default ALL |

