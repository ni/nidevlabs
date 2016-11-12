//	National Instruments : NineGrid export script
//	created : 9/20/2016
//	version: 1.00


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function parseLinearGradient(linearGradientSVGElement, paths) {
  linearGradientSVGElement = new XML(linearGradientSVGElement);

  var colorKey = linearGradientSVGElement.attribute("id");

  var shapeTxt = findShapeForGradient(colorKey, paths);
  var shape = new XML(shapeTxt);

  var gradX1 = +linearGradientSVGElement.attribute("x1");
  var gradX2 = +linearGradientSVGElement.attribute("x2");
  var gradY1 = +linearGradientSVGElement.attribute("y1");
  var gradY2 = +linearGradientSVGElement.attribute("y2");

  var shapeWidth = +shape.attribute("Width");
  var shapeHeight = +shape.attribute("Height");
  var shapeX = +shape.attribute("StartX");
  var shapeY = +shape.attribute("StartY");

  // Convert absolute values to relative values, which is what NineGrid uses
  var startX = (gradX1 - shapeX) / (shapeWidth);
  var endX = (gradX2 - shapeX) / (shapeWidth);
  var startY = (gradY1 - shapeY) / (shapeHeight);
  var endY = (gradY2 - shapeY) / (shapeHeight);

  var stops = linearGradientSVGElement.elements();

  var fullColorXml = "<Color Key=\"" + colorKey + "\" Type=\"Linear\" StartX=\"" + startX + "\" StartY=\"" + startY + "\" EndX=\"" + endX + "\" EndY=\"" + endY + "\"><Stops>";

  for (var i = 0; i < stops.length(); i++) {
    var stop = stops[i];
    var offset = stop.attribute("offset");
    var color = getColor(stop.attribute("style"));
    fullColorXml += "<Stop Color=\"" + color + "\" Offset=\"" + offset + "\" />";
  }

  fullColorXml += "</Stops></Color>";

  return new XML(fullColorXml);
}


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function parseRadialGradient(radialGradientSVGElement, paths) {
  radialGradientSVGElement = new XML(radialGradientSVGElement);

  var matrixPattern = /matrix\(([\d|\.]+)\s+([\d|\.]+)\s+([\d|\.]+)\s+([\d|\.]+)\s+([\d|\.]+)\s+([\d|\.]+)\)/
    var matrixMatch = matrixPattern.exec(radialGradientSVGElement);

  var colorKey = radialGradientSVGElement.attribute("id");

  var shape = new XML(findShapeForGradient(colorKey, paths));

  // Dependent on form of NineGridPath, assuming it has StartX, StartY, Width, Height attributes
  // This is determined in ExportDocToXml method
  var shapeX = +shape.attribute("StartX");
  var shapeY = +shape.attribute("StartY");
  var shapeWidth = +shape.attribute("Width");
  var shapeHeight = +shape.attribute("Height");

  // Assumptions: <radialGradient> element has numeric cx, cy, r or rx/ry attributes
  var gradCX = Number(radialGradientSVGElement.attribute("cx"));
  var gradCY = Number(radialGradientSVGElement.attribute("cy"));
  var gradR = Number(radialGradientSVGElement.attribute("r"));
  var gradRX = Number(radialGradientSVGElement.attribute("rx")) || gradR;
  var gradRY = Number(radialGradientSVGElement.attribute("ry")) || gradR;
  var gradOX = gradCX;
  var gradOY = gradCY;

  // Assumptions: Illustrator exports <radialGradient> into SVG with a matrix() transform attribute
  if (matrixMatch != null) {
    var scaleX = Number(matrixMatch[1]);
    var scaleY = Number(matrixMatch[4]);
    var transX = Number(matrixMatch[5]);
    var transY = Number(matrixMatch[6]);

    gradRX *= scaleX;
    gradRY *= scaleY;
    gradCY = (gradCX * scaleX) + transX;
    gradCY = (gradCY * scaleY) + transY;
    gradOX = gradCX;
    gradOY = gradCY;
  }

  // Convert absolute values to relative values, which is what NineGrid uses
  gradCX = (gradCX - shapeX) / shapeWidth;
  gradCY = (gradCY - shapeY) / shapeHeight;
  gradOX = gradCX;
  gradOY = gradCY;
  gradRX = (gradRX) / shapeWidth;
  gradRY = (gradRY) / shapeHeight;

  var fullColorXml = "<Color Key=\"" + colorKey + "\" Type=\"Radial\" CenterX=\"" + gradCX + "\" CenterY=\"" + gradCY + "\" OriginX=\"" + gradOX + "\" OriginY=\"" + gradOY + "\" RadiusX=\"" + gradRX + "\" RadiusY=\"" + gradRY + "\"><Stops>";

  var stops = radialGradientSVGElement.elements();

  for (var i = 0; i < stops.length(); i++) {
    var stop = stops[i];
    var offset = stop.attribute("offset");
    var color = getColor(stop.attribute("style"));
    fullColorXml += "<Stop Color=\"" + color + "\" Offset=\"" + offset + "\" />";
  }

  fullColorXml += "</Stops></Color>";

  return new XML(fullColorXml);
}


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function findShapeForGradient(colorKey, pathList) {
  for (var t = 0; t < pathList.length; t++) {
    if (pathList[t] != undefined) {
      var p = pathList[t].toXMLString();
      if (pathList[t].toXMLString().indexOf(colorKey) != -1 && pathList[t].localName() != "linearGradient" && pathList[t].localName() != "radialGradient")
        return pathList[t].toXMLString();
    }
  }
  return null;
}


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function getColor(colorString) {
  var fillPattern = /(fill|stop\-color)\s*:\s*#?([a-zA-Z0-9]+)/g;
  var opacityPattern = /opacity\s*:\s*([01]\.?[0-9]*)\s*/;
  var gradientPattern = /fill:url\(#([\w\W]+)\)/;

  var fillMatch = fillPattern.exec(colorString);
  var opacityMatch = opacityPattern.exec(colorString);
  var gradientMatch = gradientPattern.exec(colorString);

  if (gradientMatch) {
    var result = gradientMatch[1];
    return result;
  } else if (fillMatch && opacityMatch) {
    if (fillMatch[2] == "none")
      return "#00FFFFFF";
    else if (fillMatch[2].length == 6) {
      var opacity = +opacityMatch[1] * 255.0;
      return "#" + opacity.toString(16) + fillMatch[2];
    }
  } else if (fillMatch) {
    if (fillMatch[2] == "none")
      return "#00FFFFFF";
    else if (fillMatch[2].length == 6)
      return "#FF" + fillMatch[2];
    else
      return "#" + fillMatch[2];
  } else if (opacityMatch) {
    if (opacityMatch[1] == "none")
      return "#00000000";
    else {
      opacity = +opacityMatch[1] * 255.0;
      return "#" + opacity.toString(16) + "000000";
    }
  }
  return "#FF000000";
}