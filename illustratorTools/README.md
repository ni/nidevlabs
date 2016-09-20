## NI Dev Labs : NineGrid Illustrator export scripts v1.00

This repository contains National Instrumments internal script tools to export nineGrid .xml's from Illustrator:

#### 1. What's included:
* Scripts:
    * main script:
        * NI_driverExportXML_NineGrid.jsx
    
    * component scripts:
        * REF_createErrorStringLayer.jsx
        * REF_createPaths.jsx
        * REF_findErrorsInFiles.jsx
        * REF_gradientColors.jsx
        * REF_parsePaths.jsx
        * REF_prePost.jsx
        * REF_readWriteFiles.jsx
        
* Example Illustrator file:
        * EmailCategories.ai
* Markdown file
    * README.md
	
#### 2. Install Script
* Copy the entire "National_Instruments_PUBLIC" folder containing scripts to directory:
	* C:\Program Files\Adobe\Adobe Illustrator CC 2015.3\Presets\en_US\Scripts
* Load Illustrator
* Run script from:
    * File>Scripts>National_Instruments_PUBLIC>NI_driverExportXML_NineGrid.jsx

#### 3. Exporting paths from Illustrator
The NineGrid .xml proprietary format currrently only renders paths/compound paths with a solid or gradient fill color. This means you must run "expand" on text objects to convert them to paths/compound paths
* Paths must be:
    * Path : Compound/Path object type
    * Fill : solid or gradient
* Path will NOT export correctly with:
    * strokes
    * text
    * effects
    * NOT withing the boundries of an artboard

#### 4. Running Script
When running the script, a file destination dialog window will appear, then a series of checks are run to catch any known export issues.
Once the file has successfully completed the checks, files are exported as .svg, then converted to .xml.
If the file has failed the checks a layer is create "NI_ERRORS" with a list of errors that must be fixed before your content will export correctly.

##### Breakdown of export process 
* Select .xml file destination
* Checks for following errrors : Before export we try to catch issues that cause the SVG exported to fail
    * RasterImages
    * Non-Natives
    * Artboard name issues:
        * Illegal charcters
        * Duplicate names
        * TAB in name
        * CARRIAGE RETURN in name
        * DEFAULT "Artboard" name
    * Converts "rectangle" objects to "compound paths" :
        * Primative "rectangle" paths must be converted to "compound paths" so path rotation values are exported correctly.
    * Errors are found : Script creates a layer "NI_Errors", lists all errors found
        * Errors need to be resolved to complete export
* Export NineGrid : If file passes above error checks, the artboards are then exported
    * Writes temporary SVG files
    * Converts data in SVG to nineGrid
    * Removes temporary SVG files
    * Writes .xml files