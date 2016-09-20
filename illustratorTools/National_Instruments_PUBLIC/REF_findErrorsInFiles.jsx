//	National Instruments : NineGrid export script
//	created : 9/20/2016
//	version: 1.00


#target illustrator
app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS;

/*
DESC : checks all files in array for errors, logs errors in a .txt file
PARM : OBJECT_AI[](paths of ai files), STRING(path of search folder)
RTRN : OBJECT_AI[](paths to clean ai files)
*/
function findErrorsInfFileArray(array_AIFilePaths, searchWriteTxtPath){
  var array_cleanAIFilePaths = [];
  var string_fileErrors;
  var array_fileErrorList = [];
  var int_numErrorFiles = 0;
  
  createTxtErrorFile(null, searchWriteTxtPath, null, "create"); // creates a error .txt file
    
  for(var y = 0; y < array_AIFilePaths.length; y++){
    var tempFile = File(array_AIFilePaths[y]);
    var activeDoc = app.open(tempFile);
    array_fileErrorList = findFileErrors(activeDoc);   // returns array all errors in file
    
    // check array of errors
    if(array_fileErrorList.toString()){  // if array contain data, file has errors
      // first value in array is file path, rest of array contains actual errors
      createTxtErrorFile(array_fileErrorList[1], null, null, "write");
      int_numErrorFiles++;
    } else { // if array contains no data, file is clean
      array_cleanAIFilePaths.push(tempFile.fsName);  // store file path of clean file
    } // else
      
    $.writeln("Checking for errors in file : "+ (y + 1) +" / " + array_AIFilePaths.length);
    activeDoc.close(SaveOptions.DONOTSAVECHANGES);
  } // for loop

  // close error .txt file 
  createTxtErrorFile(null, null, int_numErrorFiles, "close");

  return array_cleanAIFilePaths;
}  // end of main

/*
DESC : runs a list of error checks on file that cause the SVG exporter to fail
PARM : OBJECT_DOC(document to check errors)
RTRN : ARRAY_STRING(array of all errors in document)
*/
function findFileErrors ( activeDoc ) {
    var layerStatusLocked = [];
    var layerStatusVisible = [];
    var fileErrorString = "";    
    var fileTxtOutput;
    var array_errorList = [];
    
    var findNonNative = "[NonNativeItem ]";
    var findRasterImage ="[RasterItem ]";
    var isNonNative;
    var isRasterImage;

    layersUnlockAll(activeDoc, layerStatusLocked, layerStatusVisible);  // unlock all layers
    fileErrorString += artBoardNameCheck(activeDoc, fileErrorString);   // check artboard names
    fileErrorString += searchAllPageItemsForObject(findNonNative);          // find non-native objects
    fileErrorString += searchAllPageItemsForObject(findRasterImage);        // find raster images
    fileErrorString += findLockedHiddenContent();                       // find locked or hidden objects
    fileErrorString += findEmptyArtboards(activeDoc);                       // find locked or hidden objects

    // if errors are found, store file name, and errors in one long string
    if (fileErrorString) {  // creates a string of errors for file if errors was return
      var tempFile = File(activeDoc.fullName);
      fileTxtOutput = "FILE : " + tempFile.fsName + "\n";
      fileTxtOutput += fileErrorString + "\n";
      array_errorList[1] = fileTxtOutput;
    }
  
    returnallLayerLockStatus(layerStatusLocked, layerStatusVisible);
    
    return array_errorList;   // first value is "true" if files is clean, else array contains arrays of errors
}  // end of findFileErrors


function findEmptyArtboards(doc){
  var errorTxt = "";

  for (var g = 0; g < doc.artboards.length; g++) {
    
    doc.artboards.setActiveArtboardIndex(g);
    doc.selectObjectsOnActiveArtboard();
    
    var pathCount = doc.selection.length;
    var artboardName = doc.artboards[g].name;
    
    //$.writeln(artboardName + " : path count = " + pathCount);
    
    if (pathCount <= 0){
      errorTxt += "EMPTY ARTBOARD please remove, found in artboard #" + (g + 1)  + " name : " + artboardName + "\n";
    }
  }
  return errorTxt;
}


/*
DESC : checks all items in document for "hidden" or "locked" status
PARM : none
RTRN : STRING(locked or hidden status)
*/
function findLockedHiddenContent(){
  var isLocked = false;
  var isHidden = false;
  var pageItems = app.activeDocument.pageItems;
  var counter = 0;
  var errorTxt = "";
  
  if (pageItems.length > 0) {
      // loops though paths until it finds hidden, locked or complets checking all paths
      do {
          // store file path of file with locked data, set boolean to true
          if (pageItems[counter].locked === true) {
            isLocked = true;
            errorTxt += "LOCKED paths in file. Goto : Object>UnlockAll, and remove deprecated content.\n"
          }
        
          if (pageItems[counter].hidden === true) {
            isHidden = true;
            errorTxt += "HIDDEN paths in file. Goto : Object>ShowAll, and remove deprecated content.\n"
          }
          counter++;
      } while ( !isHidden && !isLocked && counter != pageItems.length)
  }// end of findLockedContent

  // store current file path if there was a hidden or locked path in the current file
  return errorTxt;
}  

/*
DESC : cycles through all layers, setting locked to false, and setting visible to true
PARM : OBJECT_DOCUMENT(document to check), ARRAY(layer locked status for layers), ARRAY(layer visibility status for layers)
RTRN : none
*/
function layersUnlockAll(activeDoc, layerStatusLocked, layerStatusVisible) {
    // stores lock status for all layers in document in an array
    for (var v = 0; v < activeDoc.layers.length; v++) {
        if (activeDoc.layers[v].locked == true) {
            layerStatusLocked[v] = true;
        } else if (activeDoc.layers[v].locked == false) {
            layerStatusLocked[v] = false;
        }
      
        if (activeDoc.layers[v].visible == true) {
            layerStatusVisible[v] = true;
        } else if (activeDoc.layers[v].visible == false) {
            layerStatusVisible[v] = false;
        }      
    }

    // unlocks all layers
    for (var v = 0; v < activeDoc.layers.length; v++) {
        activeDoc.layers[v].locked = false;
        activeDoc.layers[v].visible = true;
    }
}  // end of layersUnlockAll

/*
DESC :  returns file layers back to the users original configuration
PARM :  ARRAY(layers locked status), ARRAY(layers visible status)
RTRN :  none
*/
function returnallLayerLockStatus(layerStatusLocked, layerStatusVisible) {
    var activeDoc = app.activeDocument;

    // returns all layer lock status back to original values
    for (var v = 0; v < activeDoc.layers.length; v++) {
        activeDoc.layers[v].locked = layerStatusLocked[v];
        activeDoc.layers[v].visible = layerStatusVisible[v];
    }
}  // end of returnallLayerLockStatus

/*
DESC : checks the artboard names for erros that cause SVG export to fail
PARM : OBJECT_DOCUMENT(document to check), STRING(list of all errors in file)
RTRN : STRING(concant string of artboard name errors found in document)
*/
function artBoardNameCheck(doc, fileErrorString) {
    var artboards = doc.artboards;
    var warning = '\t** WARNING  **';
    var regExChars = /[<>.?!:\"\/\\|?*]/g; // list of illegal characters to search for in file name
    var regexTabs = /(\t)/g;

    // START of ARTBOARD NAME CHECK
    for (var j = 0; j < artboards.length; j++) {
      
        // check for DUPLICATE ARTBOARD NAMES
        var currentArtboard = artboards[j];
        for (k = 0; k < j; k++) {
            if (artboards[k].name.toUpperCase() == currentArtboard.name.toUpperCase()) { // converts artboard name to caps, checks for duplicates
                fileErrorString += "DUPLICATE artboard name found in artboard #" + (j + 1) + " : " + artboards[j].name + "\n";
            } // end of loop
        } // end of DUPLICATE ARTBOARD NAMES

        // checks for iILLEGAL CHARACTER
        if (artboards[j].name.search(regExChars) > -1) {
            var illegalChar = artboards[j].name.substr(parseInt(artboards[j].name.search(regExChars)), 1); //find the actual illegal character for onscreen alert
            fileErrorString += "ILLEGAL CHARACTER '[<>.?!:\"\/\\|?*' \"" + illegalChar + "\" found in artboard #" + (j + 1)  + " name : " + artboards[j].name + "\n";
        } // end of ILLEGAL CHARACTER

        // checks for illegal CARRIAGE RETURN
        if (artboards[j].name.search(/\r/) != -1) {
            fileErrorString += "CARRIAGE RETURN found in artboard #" + (j + 1) + " name : " + artboards[j].name + "\n";
        } // end of CARRIAGE RETURN

        // checks for illegal TAB
        if (artboards[j].name.search(regexTabs) != -1) {
            fileErrorString += "TAB found in artboard #" + (j + 1) + " name : " + artboards[j].name + "\n";
        } // end of  TAB
      
        // checks for unnamed artboards with "Artboard"
        // had to rename artboards because the default  "Artboards" will export as their artboard numeric value
        if (artboards[j].name.search("Artboard") != -1) {
            fileErrorString += "DEFAULT name found, update  \"Artboard\", in artboard #" + (j + 1) + " name : " + artboards[j].name + "\n";
        } // end of  unnamed ARTBOARD            
      
    } // end of ARTBOARD NAME CHECK 
    return fileErrorString;
} // end of artBoardNameCheck()


/* 
DESC : Searches for a specific object type on all artboards
PARM : OBJECT_TYPE(type of object to search for)
RTRN : STRING(artboard object type was found on, or none)
*/
function searchAllPageItemsForObject( _objectType ){
  var activeDoc = app.activeDocument;
  var foundOnArtboard;
  var foundObjectTypeLoc = "";
  
  for(var g = 0; g < activeDoc.artboards.length; g++){
    app.selection = false;
    activeDoc.artboards.setActiveArtboardIndex(g);
    activeDoc.selectObjectsOnActiveArtboard();
    var itemsOnPage = app.activeDocument.selection;
    app.selection = false;
    
    // search all page items, until object of type is found
    for(var r = 0; r < itemsOnPage.length; r++){
      if(itemsOnPage[r] == _objectType.toString() ){
        //$.writeln("Found " +_objectType+ " on artboard"+ g);
       foundObjectTypeLoc +=  _objectType + " on artboard #" + (g + 1) + ", file contains images or non-path items.\n"; 
      } // if objectType
    } // for itemsOnPage.length
  } // for 
  app.selection = false;
  return foundObjectTypeLoc;
}


/*
DESC : creates/writes/close errors found if files to a .txt file
PARM : STRING (written to file), STRING(location of error file), INT(amount of files), STRING(action to executes
RTRN : NONE
*/
function createTxtErrorFile(fileErrorString, rootFldr, numFiles, fileAction) {

  // various file strings
  var arrowEnd = "<END>\n";
  var header_searchRoot = "\n\n< --- ROOT SEARCH PATH --- >\n";
  var header_AIFiles = "\n\n< --- AI FILE PATHS --- >\n";
  var dateHeader = "< --- CREATED ON  --- >\n" + new Date() + "\n<END>" + "\n";
  
  // creates file
  if(fileAction == "create"){
    // extracts folder location 
    var reg = /[^\\]+$/g; //last folder sting in full path
    var folderLocation = rootFldr.fsName.match(reg).toString().replace(/( +)/, "");
  
    // create a text file to store all the file and artboard data
    file_errorsInFile = File(rootFldr + '/' + 'log_errorFiles_' + folderLocation + '.txt');
    file_errorsInFile.open("w");
    file_errorsInFile.write(dateHeader); // writes date header string to file
    
    file_errorsInFile.write(header_searchRoot + rootFldr +"\n" + arrowEnd); 
    file_errorsInFile.write(header_AIFiles);
  } // end of create action
  
  // write errors to file
  if(fileAction == "write"){
    file_errorsInFile.writeln(fileErrorString);
  }// end of write action

  // close file
  if(fileAction == "close"){  
    file_errorsInFile.write(
        arrowEnd +
        "\n\n< --- PROCESS DATA --- >"
        + "\n" + "Total number of files that failed check = " + numFiles
        + "\n" + arrowEnd);
    file_errorsInFile.close();
  } // end of close action
} // end of createAITxtFile


/* -----------------------------------------------NOT USED ----------------------------------------------------------------------*/

/*  NOT USED BECAUSED IT STOPPED AFTER FINDING ONE OBJECT TYPE,
    I NEED TO FIND ALL ARTBOARD WITH TYPES, NOT JUST A SINGLE INSTANCE
DESC : Searches for a specific object type on all artboards
PARM : OBJECT(type of object to search for)
RTRN : STRING(artboard object type was found on, or none)

function searchArtboardForObject( _objectType ){
  var activeDoc = app.activeDocument;
  var countArtboards = 0 ;  
  var isFound = false;
  var foundOnArtboard;
  
  do{
      app.selection = false;
      activeDoc.artboards.setActiveArtboardIndex(countArtboards);
      activeDoc.selectObjectsOnActiveArtboard();
      var itemsOnPage = app.activeDocument.selection;
      
      // search all page items, until object of type is found
      for(var r = 0; r < itemsOnPage.length; r++){
        
        if(itemsOnPage[r] == _objectType.toString() ){
          isFound = true;
          foundOnArtboard = r;
        } // if objectType
      } // for itemsOnPage.length

  countArtboards++;
  
  // end search if object is found, or reached final artboard
  } while (!isFound && countArtboards != activeDoc.artboards.length);
  
  app.selection = false;
  
  if(isFound){  return  "" + _objectType + " on artboard #" + ( foundOnArtboard +1 ) + ", check all artboards for images or non-path items.\n"; }
  return "";
}
*/