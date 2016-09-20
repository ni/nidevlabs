//	National Instruments : NineGrid export script
//	created : 9/20/2016
//	version: 1.00


/***************************************************************************************
Collection of functions used by export scripts

Currently referenced by:
    ExportNineGridXML_RC.jsx
    ExportIconsXML_RC.jsx
*/

var timeStart;
var timeEnd;
var layersStatus = [];

function main_prePost(destFolder, activeDoc, runFunc) {

  if (runFunc == "pre") {
    preExport(activeDoc, destFolder);
  } 

  if (runFunc == "post") {
    postExport(activeDoc);
  }
}


/***************************************************************************************
    DESC      :   List of functions to run BEFORE any exporting takes palce
    ACCEPTS   : (document object), (layerStatus array)
    RETURNS : NONE
*/
function preExport(activeDoc, destFolder) {
    timeStart = new Date().getUTCSeconds();
    pre_layersUnlockAll(activeDoc, layersStatus);
    compoundPathOnArtboards();
}


/***************************************************************************************
    DESC       :  List of functions to run AFTER all files have been exported
    ACCEPTS   : (layerStatus array)
    RETURNS : NONE
*/
function postExport(activeDoc) {
    pre_returnallLayerLockStatus(activeDoc, layersStatus);
    timeEnd = new Date().getUTCSeconds();
}


/***************************************************************************************
    DESC       :  Computes and prints time difference between two time parameters
    ACCEPTS   : NONE
    RETURNS : NONE
*/
function printTime(timeStart, timeEnd) {
    var total = timeEnd - timeStart;
    $.writeln("Export time for file .xml = ", total, " seconds");
}


/***************************************************************************************
    DESC       :  Selects paths are each artboard, recursively finds pathItems(ONLY), then compounds them
    ACCEPTS   : NONE
    RETURNS : NONE
*/
function compoundPathOnArtboards() {
    // cycles through all artboards, converting pathItems to compoundPathItems
    for (var b = 0; b < app.activeDocument.artboards.length; b++) {

        //  sets active artboard based on index
        //  creates reference variable for all paths on artboard
        //  deselects all paths to avoid compounding all paths
        app.activeDocument.artboards.setActiveArtboardIndex(b);
        app.activeDocument.selectObjectsOnActiveArtboard();
        var artboardPaths = app.activeDocument.selection;
        app.activeDocument.selection = false;

        //  cycle through all paths stored in the reference object
        for (var t = 0; t < artboardPaths.length; t++) {
            var currentPath = artboardPaths[t];
            if(currentPath != "[TextFrame ]"){  // catch any textFrame objects
              findPathInGroup(currentPath);
            }
        } // end of artboard paths
    } // end of artboard
} // end of compoundPathOnArtboards


/***************************************************************************************
    DESC       :  checks if parameter is a path, then call compoundPath()
                      if object is a GroupItem will recursively call function on itself on children in GroupItem
    ACCEPTS   : (PathItem) or (GroupItem)
    RETURNS : NONE
*/
function findPathInGroup(path) {
    if (path.typename == "PathItem" ) {
        compoundPath(path);
    }

    if (path.typename == "GroupItem") {;
        for (var g = 0; g < path.pageItems.length; g++) {
            findPathInGroup(path.pageItems[g]);
        } // end of group pathItems
    }
} // end of findPathInGroup


/***************************************************************************************
    DESC      :   apllies Menu Command "compoundPath" on incoming path
    ACCEPTS   : NONE
    RETURNS : NONE
*/
function compoundPath(pathConvert) {
    var fillColor = pathConvert.fillColor;
    pathConvert.selected = true; //  have to select path to check points.length
    app.executeMenuCommand("compoundPath");
    pathConvert.fillColor = fillColor;
    app.activeDocument.selection = false;
}

/*************************************************************************************** 
    DESC      :   Removes items that cause SVG exporter to fail
    ACCEPTS   : NONE
    RETURNS : NONE
*/
function removeJunk() {

    var allNonNativePaths = app.activeDocument.nonNativeItems;
    var allRasterItems = app.activeDocument.rasterItems;

    layersUnlockShow();

    while (allNonNativePaths.length > 0) {
        for (var c = 0; c < allNonNativePaths.length; c++) {
            allNonNativePaths[c].remove();
        }
    }

    while (allRasterItems.length > 0) {
        for (var c = 0; c < allRasterItems.length; c++) {
            allRasterItems[c].remove();
        }
    }
}


/*************************************************************************************** 
    DESC      :   Unlocks all layers before export, storing the status of each layer so they can be restored
    ACCEPTS   : (Document object) (Array of layer status)
    RETURNS : NONE
*/
function pre_layersUnlockAll(activeDoc) {
    var activeDoc = app.activeDocument;
    // stores lock status for all layers in document in an array
    
    for (var v = 0; v < activeDoc.layers.length; v++) {
        if (activeDoc.layers[v].locked == true) {
            layersStatus[v] = true;
        } else if (activeDoc.layers[v].locked == false) {
            layersStatus[v] = false;
        }
    }
    
    // unlocks all layers
    for (var v = 0; v < activeDoc.layers.length; v++) {
        activeDoc.layers[v].locked = false;
    }
}



/*************************************************************************************** 
    DESC          :  returns all layer visiblity back to there original state
    ACCEPTS   : NONE
    RETURNS : NONE
*/
function pre_returnallLayerLockStatus(activeDoc) {
    var activeDoc = app.activeDocument;

    // returns all layer lock status back to original values
    for (var v = 0; v < activeDoc.layers.length; v++) {
        activeDoc.layers[v].locked = layersStatus[v];
    }
}