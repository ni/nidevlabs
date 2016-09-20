//	National Instruments : NineGrid export script
//	created : 9/20/2016
//	version: 1.00


#include "REF_findErrorsInFiles.jsx"

/*
    ACCEPTS:           OBJECT_DOCUMENT
    DESCIPTION:     removes previous layer with error text, runs "findErrorsIntFiles" method,  creates layer with error text object
    RETURNS:          TRUE(if errors exist in file, creates layer with error text object), FALSE(if no errors are found in file)
*/
function createErrorStringLayer(doc){
  

	var isErrors;
	var stringOfErrors = "";
	var layerName = "NI_ERRORS" ;	

	removePreviousErrorLayer(doc, layerName); // removes previous layer with error text object
	stringOfErrors = findFileErrors (doc).toString();
  
  // executes if the findFileErrors returns a string of errors
	if(stringOfErrors){
		isErrors = true;
		createLayerErrors(doc, stringOfErrors, layerName);
		alert("WHOOPS!\n\nFound errors, please review \"" + layerName + "\" layer.","WARNING");		
	} else {
		isErrors = false;
	}
	

	return isErrors;
}

/* ----------------------------------------------- METHODS ----------------------------------------------- */

/*
    ACCEPTS:           OBJECT_DOCUMENT(usually the current active doc), STRING(list of errors)
    DESCIPTION:     deletes existing artboard, then creates a new layer if one does not exists, adds errors to textFrame on layer
    RETURNS:          none
*/
function createLayerErrors(doc, errors, layerName){
    var textFrame;
    var textColor = new RGBColor();
          textColor.red = 255;
          textColor.green = 135;
          textColor.blue = 0;

    var errorLayer = createNewLayer(doc, layerName);
    
    textFrame = doc.textFrames.add();
    textFrame.contents = errors;
    textFrame.textRange.fillColor = textColor;
    errorLayer.locked = false;
} // end of createLayerErrors()


/*
    ACCEPTS:           OBJECT_DOCUMENT(usually the current active doc), STRING(list of errors), OBJECT_LAYER(layer to find and remove)
    DESCIPTION:     deletes and existing artboard incase it exists and has an object 
                                   that could be accidently picked up by "empty artboard" error check
    RETURNS:          none
*/
function removePreviousErrorLayer(doc, layerName ){
  for (var g = 0; g < doc.layers.length; g++) {
    if (doc.layers[g].name == layerName) {
      doc.layers[g].locked = false; 
      doc.layers[g].visible = true;
      doc.activeLayer = doc.layers[g];
      doc.activeLayer.remove(g);
    }
  }
} // end of removePreviousErrorLayer()


/*
    ACCEPTS:           OBJECT_DOCUMENT(document to create layer), STRING(layer name to create)
    DESCIPTION:     deletes existing artboard, then creates a new layer if one does not exists
    RETURNS:          none
*/
function createNewLayer(doc, layerName) {
    // finds layer, unlocks, make visible, then deletes the outdated layer
    for (var g = 0; g < doc.layers.length; g++) {
        if (doc.layers[g].name == layerName) {
            doc.layers[g].locked = false; 
            doc.layers[g].visible = true;
            doc.activeLayer = doc.layers[g];
            doc.activeLayer.remove(g);
        }
    }
    var newLayer = doc.layers.add();
    newLayer.name = layerName;
    doc.activeLayer = newLayer;  // sets as active
    return newLayer;
}  // end of createNewLayer()