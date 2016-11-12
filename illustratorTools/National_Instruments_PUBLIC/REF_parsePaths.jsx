//	National Instruments : NineGrid export script
//	created : 9/20/2016
//	version: 1.00


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function parseCircle(circleSvgElement, artboardName) {
  var cx,
  cy,
  radius,
  color;

  cx = +circleSvgElement.attribute("cx");
  cy = +circleSvgElement.attribute("cy");
  radius = +circleSvgElement.attribute("r");
  color = getColor(circleSvgElement.toXMLString());

  return createEllipsePath(cx, cy, radius, radius, color, artboardName);
}

/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function parseEllipse(ellipseSvgElement, artboardName) {
  var cx,
  cy,
  rx,
  ry,
  color;

  cx = +ellipseSvgElement.attribute("cx");
  cy = +ellipseSvgElement.attribute("cy");
  rx = +ellipseSvgElement.attribute("rx");
  ry = +ellipseSvgElement.attribute("ry");
  color = getColor(ellipseSvgElement.toXMLString());

  return createEllipsePath(cx, cy, rx, ry, color, artboardName);
}

/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function parsePath(pathSvgElement, artboardName, boundingBoxes, currentPathIndex) {
  var data,
  color;

  data = "F1 " + pathSvgElement.attribute("d");
  color = getColor(pathSvgElement.toXMLString());
  if (color.indexOf("E4E4E4") != -1) {
    var t = 0;
  }

  return createPath(data, color, artboardName, boundingBoxes, currentPathIndex);
}

/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function parsePolygon(polygonSvgElement, artboardName, boundingBoxes, currentPathIndex) {
  var data,
  color;

  color = getColor(polygonSvgElement.toXMLString());

  var polygonToPathPattern = /(\s+[0-9\+\-])/g;
  var firstPointPattern = /([0-9\-\+\.]+,[0-9\-\+\.]+)/;
  var firstPointMatch = firstPointPattern.exec(polygonSvgElement.attribute("points"));

  if (firstPointMatch != null) {
    var points = polygonSvgElement.attribute("points").toString().replace(polygonToPathPattern, "L$1").replace(/\s+/g, "");

    data = "F1 M" + points + " L" + firstPointMatch[1] + "z";
  }

  return createPolygonPath(data, color, artboardName, boundingBoxes, currentPathIndex);
}

/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function parsePolyline(polylineSvgElement, artboardName, boundingBoxes, currentPathIndex) {
  var data,
  color;

  color = getColor(polylineSvgElement.toXMLString());

  var polylineToPathPattern = /(\s+[0-9\-\+])/g;

  var points = polylineSvgElement.attribute("points").toString().replace(polylineToPathPattern, " L$1");

  data = "F1 M" + points;

  return createPolylinePath(data, color, artboardName, boundingBoxes, currentPathIndex);
}

/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function parseRect(rectSvgElement, artboardName) {
  var rectX,
  rectY,
  rectWidth,
  rectHeight,
  color;

  rectX = +rectSvgElement.attribute("x");
  rectY = +rectSvgElement.attribute("y");
  rectWidth = +rectSvgElement.attribute("width");
  rectHeight = +rectSvgElement.attribute("height");
  color = getColor(rectSvgElement.toXMLString());

  return createRectPath(rectX, rectY, rectWidth, rectHeight, color, artboardName);
}

/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function parseLine(lineSvgElement, artboardName) {
  var x1,
  x2,
  y1,
  y2,
  color;

  x1 = +lineSvgElement.attribute("x1");
  x2 = +lineSvgElement.attribute("x2");
  y1 = +lineSvgElement.attribute("y1");
  y2 = +lineSvgElement.attribute("y2");
  color = getColor(lineSvgElement.toXMLString());

  return createLinePath(x1, x2, y1, y2, color, artboardName);
}