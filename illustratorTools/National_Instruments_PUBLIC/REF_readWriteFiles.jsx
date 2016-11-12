//	National Instruments : NineGrid export script
//	created : 9/20/2016
//	version: 1.00


/***************************************************************************************
    DESC :  Exports all artbooards into separate SVG files
    first creates empty SVG files, saves file objects to array
    then creates temp duplicate of working file
    exports populated SVG files from temp
    removes the temp file
    returns all artboard data as an array of SVG file objects
    
    ACCEPTS : (Document object)
    
    RETURNS : (Array) of empty .svg File objects based on amount of artboards, files are populated later
 */
function exportDocToSVG(doc, destFolder, boundingBoxes, currentPathIndex, colorList) {

  var newDocName = doc.name.replace(/[.]/g, ""); // created to remove dot, that was causing SVG files read/write to fail
  var artboards = doc.artboards;
  var docFileName = doc.fullName;
  var currentSvgFileList = [];

  // creates an empty SVG file for each artboard, test for read/write before exporting path data, stores SVG file object in an array
  for (var j = 0; j < artboards.length; j++) {
    var currentArtboard = artboards[j];
    // sends docName with appended artboard name, with file extension dot removed, receives an SVG file object for artboard
    var currentSvgFile = getTargetFile(newDocName + "_" + currentArtboard.name, ".svg", destFolder);
    currentSvgFileList.push(currentSvgFile);
  }

  // creates a duplicate temp SVG File to export artboards using cleaned-up document name
  var docSvgFile = new File(destFolder + "//" + newDocName + ".svg");

  // export all artboards from temp SVG File object, then removes temp SVG file
  doc.exportFile(docSvgFile, ExportType.SVG, getOptions(colorList));
  docSvgFile.remove();

  // checks if the above temp SVG file is open and force closes it
  if (artboards.length > 0) {
    for (j = 0; j < app.documents.length; j++) {
      if (app.documents[j].fullName.fullName == docSvgFile.fullName) {
        app.documents[j].close(SaveOptions.DONOTSAVECHANGES);
      }
    }
  }
  return currentSvgFileList;
}


/***************************************************************************************
    DESC :  Creates a .FIle object(.svg or .xml) for each incoming artboard, test to ensure file is accessible for read/write
    
    ACCEPTS : (STRING) active file name, without file extension dot, and added artboard name
    (STRING) file extension for new file
    (FOLDER) folder object location to save file
    
    RETURNS : (FILE) .svg file object is returns
 */
function getTargetFile(docName, ext, destFolder) {

  var newName = "";

  // check if file name has dot character
  // dot characters in SVG file names cause exporting to fail

  // if docName does NOT have a dot, create newName, adding dot and "ext"
  if (docName.indexOf('.') < 0) {
    // and if the "ext" parameters has no dot before creating newName
    if (ext.indexOf('.') < 0) {
      // create newName with docName, removing any dots, add dot and "ext" parameter
      newName = docName.replace(".", "") + '.' + ext;
      // dot exists in "ext" parameter so, remove dots from docName, add "ext" parameter
    } else {
      newName = docName.replace(".", "") + ext;
    }
    // find dot location, create newName from location, then add ext
  } else {
    var dot = docName.lastIndexOf('.');
    newName += docName.substring(0, dot);
    newName += ext;
  }

  // Create the file object to save to
  var targetFile = new File(destFolder + '/' + newName);

  // Preflight access rights
  if (targetFile.open("w")) {
    targetFile.close();
  } else {
    throw new Error('getTargetFile() : Access is denied.');
  }
  return targetFile;
};


/***************************************************************************************
    DESC :  creates a SVG export object needed as a parameter when saving out .svg's
    saveMultipleArtboard propeties is set to "true" in this export obj
    
    ACCEPTS : none
    
    RETURNS : (ExportOptionsSVG)  object used when saving out .svg files
 */
function getOptions(colorList) {
  // Create the required options object
  var options = new ExportOptionsSVG();
  // See ExportOptionsSVG in the JavaScript Reference for available options
  options.saveMultipleArtboards = true;
  options.embedRasterImages = false;
  //options.cssProperties = SVGCSSPropertyLocation.ENTITIES;
  options.DTD = SVGDTDVersion.SVG1_1;
  return options;
};


/*
DESC :        Gets all NineGrid paths from all artboards
                      Writes a .xml file
                      Sends NineGrid paths, XML File and XML text to create full XML object
ACCEPTS : (Array Generic) Stores all the NineGrid paths in current document by the ArtBoards name
                    (Array Artboards) All artboards in current document
                    (Array Strings)  List of all artboard names
RETURNS : NONE
 */
function writePathsToXml(allNineGridPaths, artboardNames, allArtboards, destFolder, colorList) {
  
  // cycle through all artboard names, writing out final XML files
  for (var j = 0; j < artboardNames.length; j++) {
    var paths = allNineGridPaths[artboardNames[j]];

    // send artboard name, file format, file destination to write temp XML file, finalized in the "writeTxtFile" function
    var xmlFile = getTargetFile(artboardNames[j], '.xml', destFolder);
    
    writeXmlFile(xmlFile, allNineGridPaths[artboardNames[j]], colorList[artboardNames[j]].colors, allArtboards[j]);
  }
}


/***************************************************************************************
    DESC :    creates a XML string from an artboard
                    - adds prolog header
                    - adds NineGridDictionary, colorTable tag
                    - adds gradients to colorTable, then closes tag
                    - adds nineGrid paths, but removes variables not needed
                    - adds closing tags for paths, NineGridPathList, NineGridDictionary
                    
                    creates new XML object to format string, then converts back to string
                    - sends xmlTxt string to write function
                    
    ACCEPTS :   (FILE) empty .xml File object, populated in "writeTxtFile"
                          (OBJECT) contains NineGrid array data for this artboard
                          (ARRAY) contains all gradients on current artboard
                          (ARTBOARD object) active artboard, used to find heigh/width data
    RETURNS : NONE
 */
function writeXmlFile(xmlFile, paths, colors, currentArtboard) {
  var xmlTxt = '<?xml version="1.0" encoding="utf-8"?>'; // WARNING : this prolog is lost when string is cast to new XML object
  xmlTxt += '<NineGridDictionary><ColorTable>';
  
  // append all gradients under the "ColorTable" section
  for (var k = 0; k < colors.length; k++) {
    if (colors == undefined)
      continue;
    if (xmlTxt.indexOf(colors[k].toXMLString()) == -1) {
      xmlTxt += colors[k].toXMLString();
    }
  }

  // artBoardRect properties : artboardRect (leftEdge[0], topEdge[1], rightEdge[2], bottomEdge[3])
  var currentArtboardHeight = Math.abs(currentArtboard.artboardRect[1] - currentArtboard.artboardRect[3]);
  var currentArtboardWidth = Math.abs(currentArtboard.artboardRect[0] - currentArtboard.artboardRect[2]);
  xmlTxt += '</ColorTable><Items>' + '<NineGridPathList Height="' + currentArtboardHeight + '" Width="' + currentArtboardWidth + '">' + '<Paths>';

  // create reference to all NineGrid paths for "this" artboard
  paths = paths.nineGridPaths;
  
  // cycle through all NineGrid path indices, appending each path to the "xmlTxt" string
  for (var i = 0; i < paths.length; i++) {
    if (paths[i] == undefined)
      continue;
    // removing attributes from each path because they are not needed, but attributes ARE needed to calculate gradients properly
    delete paths[i]. @ Height;
    delete paths[i]. @ Width;
    delete paths[i]. @ StartX;
    delete paths[i]. @ StartY;
    xmlTxt += paths[i].toXMLString();
  }

  // append all paths to the xml string
  xmlTxt += "</Paths></NineGridPathList></Items></NineGridDictionary>";

  // create new XML object from xml string, this also formats string into proper xml format
  xmlTxt = (new XML(xmlTxt)).toXMLString();

  writeTxtFile(xmlFile, xmlTxt);
};


/***************************************************************************************
    DESC :  tests if previously created file is available for write, then opened and XML objected added
                    appened the XML prolog that was lost when converted  xml string to xml object
                    
    ACCEPTS :   (FILE) temp file created at location specifed by user at runtime
                              (XML) object created from the converted .svg data
                              
    RETURNS : NONE
 */
function writeTxtFile(file, txt) {
  if (file.readonly) {
    throw "Cannot write to file: " + decodeURI(file.absoluteURI);
  }
  file.encoding = "UTF8";
  file.open("w", "TEXT", "????");
  // unicode signature, this is UTF16 but will convert to UTF8 "EF BB BF"
  file.write("\uFEFF");

  //  add prolog that is lost when casting the xmlTxt var to the new XML object
  file.write('<?xml version="1.0" encoding="utf-8"?>\n');

  // then write the XML object to the file
  file.write(txt);
  file.close();
};


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS : (STRING) contents of file
 */
function readTxtFile(file) {
  if (!file.exists) {
    throw "Cannot find file: " + decodeURI(file.absoluteURI);
  }
  file.open("r", "TEXT");
  file.encoding = "UTF8";
  file.lineFeed = "unix";
  var str = file.read();
  file.close();
  return str;
};