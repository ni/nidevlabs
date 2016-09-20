//	National Instruments : NineGrid export script
//	created : 9/20/2016
//	version: 1.00

/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function createLinePath(x1, x2, y1, y2, color, artboardName) {
  var width = Math.abs(x2 - x1);
  var height = Math.abs(y2 - y1);

  var data = "F1 M" + x1 + "," + y1 + " L" + x2 + "," + y2;

  var result = '<NineGridPath StartX="' + Math.min(x1, x2) + '" StartY="' + Math.min(y1, y2);
  result += '" Width="' + width + '" Height="' + height + '" Color="' + color + '" Data="' + data + '" />';

  return new XML(result);
}


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function createEllipsePath(xOrigin, yOrigin, xRadius, yRadius, color, artboardName) {
  var xOffset = xRadius * 0.5522848;
  var yOffset = yRadius * 0.5522848;
  var left = xOrigin - xRadius;
  var top = yOrigin - yRadius;
  var right = xOrigin + xRadius;
  var bottom = yOrigin + yRadius;
  var leftCtrl = xOrigin - xOffset;
  var topCtrl = yOrigin - yOffset;
  var rightCtrl = xOrigin + xOffset;
  var bottomCtrl = yOrigin + yOffset;
  var ellipseWidth = xRadius * 2;
  var ellipseHeight = yRadius * 2;

  var data = "F1 M" + left + "," + yOrigin + " C " + left + "," + topCtrl + " " + leftCtrl + "," + top + " " + xOrigin + "," + top;
  data += " C " + rightCtrl + "," + top + " " + right + "," + topCtrl + " " + right + "," + yOrigin;
  data += " C " + right + "," + bottomCtrl + " " + rightCtrl + "," + bottom + " " + xOrigin + "," + bottom;
  data += " C " + leftCtrl + "," + bottom + " " + left + "," + bottomCtrl + " " + left + "," + yOrigin + "z";

  var result = '<NineGridPath StartX="' + left + '" StartY="' + top;
  result += '" Width="' + ellipseWidth + '" Height="' + ellipseHeight;
  result += '" Color="' + color + '" Data="' + data + '" />';

  return new XML(result);
}


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function createPath(data, color, artboardName, boundingBoxes, currentPathIndex) {
  var startX = boundingBoxes[artboardName].paths[currentPathIndex].xPos;
  var startY = boundingBoxes[artboardName].paths[currentPathIndex].yPos;
  var width = boundingBoxes[artboardName].paths[currentPathIndex].width;
  var height = boundingBoxes[artboardName].paths[currentPathIndex].height;

  var result = '<NineGridPath StartX="' + startX + '" StartY="' + startY;
  result += '" Width="' + width + '" Height="' + height + '" Color="' + color + '" Data="' + data + '" />';

  return new XML(result);
}


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function createPolylinePath(data, color, artboardName, boundingBoxes, currentPathIndex) {
  return createPath(data, color, artboardName, boundingBoxes, currentPathIndex);
}


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function createPolygonPath(data, color, artboardName, boundingBoxes, currentPathIndex) {
  return createPath(data, color, artboardName, boundingBoxes, currentPathIndex);
}


/***************************************************************************************
    DESC :
    ACCEPTS :
    RETURNS :
 */
function createRectPath(rectX, rectY, rectWidth, rectHeight, color, artboardName) {
  var data = "F1 M" + rectX + "," + rectY + " h" + rectWidth + "v" + rectHeight + "h" + "-" + rectWidth + "v" + "-" + rectHeight + "z";

  var result = '<NineGridPath StartX="' + rectX + '" StartY="' + rectY;
  result += '" Width="' + rectWidth + '" Height="' + rectHeight + '" Color="' + color + '" Data="' + data + '" />';

  return new XML(result);
}
