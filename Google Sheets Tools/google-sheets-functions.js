/*======================================================

 Google Sheets Functions
 -----------------------
 These functions are useful for converting Google Sheets
 data into JSON data that is easily consumable by a game
 engine. The expected workflow is:

1) Design and test game design content in Google Sheets
2) Select a range of data including headers
3) Choose Game Design > Convert to JSON
4) Import the generated JSON into your game!
 
 In Sheets choose Extensions > Apps Script and set up
 scripts for your sheet. This code expects the
 jsonTemplate.html file to exist at the same level
 as your app script

======================================================*/

// Called automatically by sheets when loading a sheet. Will
// add a new menu with an option to convert selected cells
// to JSON
function onOpen() {
    var ui = SpreadsheetApp.getUi();
    ui.createMenu("Game Design")
        .addItem("Convert to JSON", "convertToJson")
        .addToUi();
}

// Called by Sheets when clicking the custom menu
// option created in onOpen. Converts selected
// data to JSON, puts it in side panel, and displays
// the side panel
function convertToJson() {
    var data = SpreadsheetApp.getActiveRange().getValues();
    var headers = data.slice(0, 1);
    var rows = data.slice(1, data.length);
    var json = JsonArray(headers, rows);
    var template = HtmlService.createHtmlOutputFromFile("jsonTemplate.html");
    var htmlText = template.getContent().replace("{json}", json);
    //'<textarea rows="25" style="width:100%;height:100%">'+json+'<textarea>'
    var html = HtmlService.createHtmlOutput(htmlText)
        .setTitle("JSON Data");
    SpreadsheetApp.getUi().showSidebar(html);
}


// Creates a JSON array from an array of headers and
// an array of data. Called by convertToJson but can also
// be called directly in Sheets as a custom function!
// When using as a custom function, the output string
// cannot exceed 50k characters. Passing 'true' as the
// checkError argument will do a check for this.
function JsonArray(headers, data, checkError = false) {
    var jsonArray = [];

    // EARLY OUT: bad data formatting
    if (headers.length > 1 || data.length < 1 || headers[0].length != data[0].length) {
        return "ERROR: Headers(" + headers[0].length + ") should be a single row and number of data records(" + data[0].length + ") should match number of headers.";
    }

    try {
        // loop through all rows
        for (y = 0; y < data.length; y++) {
            var jsonObj = {};
            var emptyCount = 0;

            // create a record for each row
            for (x = 0; x < headers[0].length; x++) {
                var propName = headers[0][x];
                var propValue = data[y][x];
                if (propValue == null || propValue == "") {
                    emptyCount++;
                }
                jsonObj[propName] = propValue;
            }

            // only add if row isn't empty
            if (emptyCount < headers[0].length) {
                jsonArray.push(jsonObj);
            }
        }

        var json = JSON.stringify(jsonArray);
        var len = json.length;

        if (checkError && len > 50000) {
            return "ERROR: Sheets only allows 50k characters and this data is " + len;
        }
        else {
            return json;
        }
    }
    catch (e) {
        return "ERROR: " + e;
    }
}
