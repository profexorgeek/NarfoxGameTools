/*======================================================

 Google Sheets Functions
 -----------------------
 These functions are useful for converting Google Sheets
 data into content that is easily consumable by a game
 engine. In Sheets choose Tools > Script Editor and
 paste in any of the functions below to use them in
 the functions bar.

======================================================*/

// Creates a JSON array from headers and data
function JsonArray(headers, data) {
    var jsonArray = [];

    // EARLY OUT: bad data formatting
    if (headers.length > 1 || data.length < 1 || headers[0].length != data[0].length) {
        return "ERROR: Headers(" + headers[0].length + ") should be a single row and number of data records(" + data[0].length + ") should match number of headers.";
    }

    try {
        // loop through all rows
        for (y = 0; y < data.length; y++) {
            var jsonObj = {};

            // create a record for each row
            for (x = 0; x < headers[0].length; x++) {
                var propName = headers[0][x];
                var propValue = data[y][x];

                jsonObj[propName] = propValue;
            }

            jsonArray.push(jsonObj);
        }

        return JSON.stringify(jsonArray);
    }
    catch (e) {
        return "ERROR: " + e;
    }
}