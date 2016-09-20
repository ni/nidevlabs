//	National Instruments : NineGrid export script
//	created : 9/20/2016
//	version: 1.00


// launch specific version of Illustrator ( version 20, 64bit)
#target illustrator-20.064

// include external scripts
#include "REF_createErrorStringLayer.jsx"
#include "REF_prePost.jsx";
#include "REF_parsePaths.jsx";
#include "REF_createPaths.jsx";
#include "REF_readWriteFiles.jsx";
#include "REF_gradientColors.jsx";

// deactivates the save SVG dialog window
app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS;

main();

/*
    ACCEPTS:           none
    DESCIPTION:     asks user for file export location, checks files for errors, if errors found stops export creates error layer, else exports file
    RETURNS:          none
*/
function main(){
  var saveFilesPath = Folder.selectDialog('Select folder for NineGridXML files.', '~');
  
  if(app.documents.length > 0){
    var openDoc = app.activeDocument;
    
    // check stringOfErrors data, if found creates layer and gives alert, if no errors found export file
    if(saveFilesPath != null && createErrorStringLayer(openDoc) == false){
      exportNineGridXML(saveFilesPath, openDoc);
      alert("Completed NineGridXML export.", "Export NineGrid:");
    }

  } else {
    alert("NO document open Fool!!");
  } // end of is doc open
} // end of main


function exportNineGridXML(destFolder, openFile) {

    if(openFile.typename != "Document"){
      var activeDoc= app.open(openFile);  
    }else{
      var activeDoc =  openFile;
    }
  
    var currentPathIndex = 0;
    var colorList = [];

    try {
        if (activeDoc != undefined && destFolder != undefined ) {

            if (destFolder) {

                if (activeDoc.fullName.exists) {
                    //activeDoc.save(SaveOptions.DONOTSAVECHANGES);
                } else {
                    activeDoc.saveAs(File.saveDialog("Save AI file", ".ai"));
                }

                var activeDocFileName = activeDoc.fullName.fullName;
                
                main_prePost(destFolder, activeDoc, "pre");

                var artboardNames = getArtboardNames(activeDoc);

                var allArtboards = activeDoc.artboards; // created so I can write width and height in writeXmlFile function

                var boundingBoxes = getBoundingBoxesFromDoc(activeDoc);

                // exports all artboards in activeDocument to temp SVG files, deleted below
                var svgFileList = exportDocToSVG(activeDoc, destFolder, boundingBoxes, currentPathIndex, colorList);

                // re-open after svg file replaced ai file      
                app.open(new File(activeDocFileName));

                // all SVG file data  is parse into NineGridPath format, and stored in "nineGridPathList" array
                // all NineGridPath data for all artboards in activeDocument is stored in "nineGridPathList" array
                // removes each of the temp SVG files created from each artboard
                var nineGridPathList = exportSvgToPathList(svgFileList, artboardNames, boundingBoxes);

                writePathsToXml(nineGridPathList, artboardNames, allArtboards, destFolder, colorList);

                main_prePost(destFolder, activeDoc, "post");
            }
        }
    } catch (e) {
        alert(e.message, "Script Alert", true);
    }


    /***************************************************************************************
        DESC :  Takes document object, and generates an array with all of the artboard names.
        ACCEPTS : (Document object)
        RETURNS : (Array) of artboard names with "\n" characters removed (this was a mac issue)
     */
    function getArtboardNames(doc) {

        var artboards = doc.artboards;
        var currentArtboardNames = [];

        for (var j = 0; j < artboards.length; j++) {
            artboards[j].name = artboards[j].name.replace(/\n/g, "");
        }
        //doc.save();

        for (j = 0; j < artboards.length; j++) {
            currentArtboardNames.push(artboards[j].name);
        }

        return currentArtboardNames;
    }

    /***************************************************************************************
        DESC :  parses the document, extracting bounding box information for each artboard
                        data is used to export gradients correctly.
        ACCEPTS : (Document object)
        RETURNS : (Array) with a "artboard name" property
        With a "path" sub array property storing dimensions for each path
        width:
        height:
        xPos:
        yPos:
        zOrder:
     */
    function getBoundingBoxesFromDoc(doc) {

        var artboards = doc.artboards;
        var currentBoundingBoxes = [];

        for (var j = 0; j < artboards.length; j++) {
            var artboardName = artboards[j].name;

            var left = artboards[j].artboardRect[0],
                top = artboards[j].artboardRect[1];

            // adds a property to array for each artboard name, with sub "paths" property
            currentBoundingBoxes[artboardName] = {
                paths: []
            };

            // assumption: artboards indexed in the same way doc indexes them
            doc.artboards.setActiveArtboardIndex(j);

            doc.selectObjectsOnActiveArtboard();
            var artboardPathItems = doc.selection;

            // creates a path variable with width, height, xPos, yPos and zOrder properties
            // stores dimension data for each path on artboard, data is used for gradients
            for (var k = 0; k < artboardPathItems.length; k++) {
                var path = {
                    width: artboardPathItems[k].width,
                    height: artboardPathItems[k].height,
                    xPos: Math.abs(artboardPathItems[k].position[0] - left),
                    yPos: Math.abs(artboardPathItems[k].position[1] - top),
                    zOrder: artboardPathItems[k].absoluteZOrderPosition
                };
                currentBoundingBoxes[artboardName].paths.push(path);
            }

            // sorts all the path objects for an artboard
            currentBoundingBoxes[artboardName].paths.sort(function (a, b) {
                if (a.zOrder < b.zOrder)
                    return -1;
                if (a.zOrder > b.zOrder)
                    return 1;
            });
        }
        return currentBoundingBoxes;
    }

    /***************************************************************************************
        DESC :  Reads each SVG file from incomming array
                      Sends file data to parse all paths into NineGrid format
                      NineGrid path data for document is acculated for all artboards and returned
                      
        ACCEPTS : (ARRAY) all exported artboards SVG files
        (ARRAY) artboard names in current document
        
        RETURNS : (ARRAY) all NineGrid data for active document that was converted from SVG files
     */
    function exportSvgToPathList(svgFiles, boardNames, boundingBoxes, currentPathIndex) {

        var currentNineGridPathList = [];

        for (var j = 0; j < svgFiles.length; j++) {
            var currentSvgFile = svgFiles[j];
            var currentArtboardName = boardNames[j];

            // addes a "artboard" property with a "color" sub property to store gradient data
            colorList[currentArtboardName] = {
                colors: []
            }

            // reads SVG file contents from temp SVG file
            var svgFileContents = readTxtFile(currentSvgFile);

            // creates  "artboard propery with ""nineGridPaths" sub property to store converted SVG to NineGrid paths
            currentNineGridPathList[currentArtboardName] = {
                nineGridPaths: []
            }

            // sends data from an SVG file to parse all the paths into NineGrid format
            //  stores all NineGrid path data for an artboard as a sub property in "currentNineGridPathList" array
            currentNineGridPathList[currentArtboardName].nineGridPaths = parseSvgTxtIntoPaths(svgFileContents, currentArtboardName, boundingBoxes, currentPathIndex);

            // since all SVG path data has been read the converted to NineGrid format, temp SVG file is removed
            currentSvgFile.remove();
        } // end of for loop
        return currentNineGridPathList;
    }


    /***************************************************************************************
        DESC :  converts incoming SVG artboard data into XML object
                         updates the "svg" attributes(x, y, width, height) 
                         then sends the SVG elements to parse the containing paths into NineGrid format
                         
        ACCEPTS : (STRING) full SVG file read from temp SVG file
                                (STRING) current artboard name SVG data being operated on
                                
        RETURNS :
     */
    function parseSvgTxtIntoPaths(svgTxt, artboardName, boundingBoxes, currentPathIndex) {

        // convert raw SVG string into XML object
        var svgXmlObject = new XML(svgTxt);
        currentPathIndex = 0;

        // create multiple variables needed for gradient creation
        var svgX,
            svgY,
            svgWidth,
            svgHeight;

        if (svgXmlObject.localName() == "svg") {
            svgX = svgXmlObject.attribute("x") != undefined ? svgXmlObject.attribute("x").toString().replace("px", "") : 0;
            svgY = svgXmlObject.attribute("y") != undefined ? svgXmlObject.attribute("y").toString().replace("px", "") : 0;

            if (svgXmlObject.attribute("width") != undefined) {
                svgWidth = svgXmlObject.attribute("width").toString().replace("px", "");
                // if viewBox data exists, use values to set "svgWidth" variables
            } else if (svgXmlObject.attribute("viewBox") != undefined) {
                var viewBoxPattern = /([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)/;

                //  method executes a search for a match in a specified string. Returns a result array, or null
                //  returned array has the matched text as the first item
                //  then one item for each capturing parenthesis that matched containing the text that was captured
                var viewBoxMatch = viewBoxPattern.exec(svgXmlObject.attribute("viewBox").toString());

                // sets width property to varibles found in the viewBoxMatch array
                if (viewBoxMatch) {
                    svgWidth = +viewBoxMatch[3] - +viewBoxMatch[1];
                } else {
                    svgWidth = 0;
                }
                // viewBox undefined, set to zero
            } else {
                svgWidth = 0;
            }

            if (svgXmlObject.attribute("height") != undefined) {
                svgHeight = svgXmlObject.attribute("width").toString().replace("px", "");
                // if viewBox data exists, use values to set "svgWidth" variables
            } else if (svgXmlObject.attribute("viewBox") != undefined) {
                var viewBoxPattern = /([0-9]+)\s+([0-9]+)\s+([0-9]+)\s+([0-9]+)/;

                //  method executes a search for a match in a specified string. Returns a result array, or null
                //  returned array has the matched text as the first item
                //  then one item for each capturing parenthesis that matched containing the text that was captured
                var viewBoxMatch = viewBoxPattern.exec(svgXmlObject.attribute("viewBox").toString());

                // sets height property to varibles found in the viewBoxMatch array
                if (viewBoxMatch) {
                    svgHeight = +viewBoxMatch[4] - +viewBoxMatch[2];
                } else {
                    svgHeight = 0;
                }
                // viewBox undefined, set to zero
            } else {
                svgHeight = 0;
            }

        }

        // (XML.elemnts())  sends the XML elements from the converted SVG file text to be converted to NineGrid paths
        // (STRING) current arboard name
        // (BOOLEAN) set if function call was recursivly 
        return parseSvgChildren(svgXmlObject.elements(), artboardName, false, boundingBoxes, currentPathIndex);
    }


    /***************************************************************************************
        DESC :  finds all paths in a artboard from incoming SVG data
                      will find "g" localNames created by groups or layers
                      check visibility of these groups, and recursivly extract children looking for paths
                      once path is found, path is parse into NineGrid by finding it's path of type
                      gradients are found, parsed and stored in the "colorList" array object for active document
                      
        ACCEPTS : (XML.elements())
                             (STRING)  current artboard name
                             (BOOLEAN) if function called recursivly
                             
        RETURNS : (ARRAY) paths converted to NineGrid from SVG file data
     */
    function parseSvgChildren(svgElements, artboardName, passedFromG, boundingBoxes, currentPathIndex) {
        var paths = [];
        
        // cycle through each children elements in the incoming XML.elements from artboard
        // unfortunately, all layers are exported, even invisibile layers
        // need to find only paths in SVG file who were on visible layer and parse those
        for (var j = 0; j < svgElements.length(); j++) {
            var children = svgElements[j];

            // layers are tagged with "g" localName
            // checking if layer visibility was turned off
            if (children.localName() == "g") {

                // check if group children element is not visible, means group layer visibility was set to true
                if (children.attribute("display") != undefined && children.attribute("display").toString() == "none") {
                    continue;
                } else if (children.attribute("style") != undefined && children.attribute("style").toString().indexOf("display:none") != -1) {
                    continue;

                    // groups children element was visible, so recursivly run this.function on children elements inside group
                    // eventually a path of type is found, and path is parsed then returned and concantenated to the "path" variable
                } else {
                    paths = paths.concat(parseSvgChildren(children.elements(), artboardName, true, boundingBoxes, currentPathIndex));
                    if (!passedFromG && children.elements().length() > 0)
                        currentPathIndex++;
                }

                //  children element of path of type was found, compare path localName to case and parse path
            } else {
                switch (children.localName()) {
                case "circle":
                    {
                        paths = paths.concat(parseCircle(children, artboardName));
                        if (!passedFromG)
                            currentPathIndex++;
                        break;
                    }
                case "ellipse":
                    {
                        paths = paths.concat(parseEllipse(children, artboardName));
                        if (!passedFromG)
                            currentPathIndex++;
                        break
                    }
                case "rect":
                    {
                        paths = paths.concat(parseRect(children, artboardName));
                        if (!passedFromG)
                            currentPathIndex++;
                        break;
                    }
                case "polygon":
                    {
                        paths = paths.concat(parsePolygon(children, artboardName, boundingBoxes, currentPathIndex));
                        if (!passedFromG)
                            currentPathIndex++;
                        break;
                    }
                case "polyline":
                    {
                        paths = paths.concat(parsePolyline(children, artboardName, boundingBoxes, currentPathIndex));
                        if (!passedFromG)
                            currentPathIndex++;
                        break;
                    }
                case "path":
                    {
                        paths = paths.concat(parsePath(children, artboardName, boundingBoxes, currentPathIndex));
                        if (!passedFromG)
                            currentPathIndex++;
                        break;
                    }
                case "line":
                    {
                        paths = paths.concat(parseLine(children, artboardName));
                        if (!passedFromG)
                            currentPathIndex++;
                        break;
                    }

                } // end of swtich
            } // end of found "g" if/else
        } // end of cycle through XML.elements() 

        // parse all svgElements from parameters, checking for gradients
        // if gradient is found, parse gradient and add to "colorList" array
        for (var j = 0; j < svgElements.length(); j++) {
            children = svgElements[j];
            var p = svgElements.toXMLString();

            switch (children.localName()) {
            case "linearGradient":
                {
                    colorList[artboardName].colors.push(parseLinearGradient(children, paths));
                    break;
                }
            case "radialGradient":
                {
                    colorList[artboardName].colors.push(parseRadialGradient(children, paths));
                    break;
                }

            }
        }
        // parsed path data is returned from above recursive function call
        return paths;
    }
}