
history.pushState(null, null, location.href);
//window.onpopstate = function () {
//    history.go(1);
//};

window.getClientDate = function getClientDate() {
    var d = new Date();
    return d.getTimezoneOffset();
}

function getCountryCode(objectReference, url) {
    var response;
    var current;
    $.getJSON(url, function (data) {
        response = data;
        current = response.country_code;
        objectReference.invokeMethodAsync("SetDefaultCountryCode", response.country_code);
    });
}

function ExcelSaveAs(filename, fileContent) {
    fetch(fileContent)
        .then(res => res.blob()).then(data => {
            const blobUrl = window.URL.createObjectURL(data);
            var anchor = document.createElement('a');
            anchor.href = blobUrl;
            anchor.download = filename;
            anchor.click();
            window.URL.revokeObjectURL(blobUrl);
        }



        )
}

//function isFullScreenWidth() {
//    return window.innerWidth > 1024;
//}

//function showLatestBadgeCount(id, count) {
//    var elementID = "";
//    $("div[id*=" + id + "badge" + "]").each(function (i, el) {
//        elementID = el.id;
//    });
//    var classID = elementID + "Value";
//    document.getElementById(elementID).className = 'text-end';
//    document.getElementById(classID).textContent = count;
//}

function scrollToElementId(elementId) {
    var element = document.getElementById(elementId);
    element.scrollIntoView({ behavior: 'smooth', block: "center" });
}

function scrollOnStartofElementId(elementId) {
    var element = document.getElementById(elementId);
    element.scrollIntoView(true);
}

function scrollToBottom(elementId) {
    var objDiv = document.getElementsByClassName(elementId);
    objDiv.scrollTop = objDiv.scrollHeight;
}

function clearValue(element, isClear = false) {
    if (isClear && element !== null) {
        element.value = "";
    }
}

function setDataValue(myElement, selectedvalue) {
    if (myElement !== null) {
        document.getElementById(myElement.id).value = selectedvalue;
        //myElement.value = selectedvalue;
    }
}

function logValue(place, logValue) {
    console.log(place, logValue);
}

function invokeWebviewMethod(methodname,currentPage) {
    window.location.href = 'https://' +methodname+'.tokenexpired.displayalert?' + currentPage;
}

function getLocalOffset() {
    var d = new Date()
    var localOffset = - d.getTimezoneOffset();
    return localOffset;
}

function printData(html) {
    //var w = window.open();
    //w.document.write(html);
    //w.window.print();
    //w.document.close();
    var newWindow = window.open('', '', 'width=100, height=100'),
        document = newWindow.document.open(),
        pageContent =
            html;
    document.write(pageContent);
    document.close();
    newWindow.moveTo(0, 0);
    newWindow.resizeTo(screen.width, screen.height);
    setTimeout(function () {
        newWindow.print();
        newWindow.close();
    }, 250);
}


$(document).unbind('keydown').bind('keydown', function (event) {
    //if (event.keyCode === 8) {
    //    var doPrevent = true;
    //    var types = ["text", "password", "file", "search", "email", "number", "date", "color", "datetime", "datetime-local", "month", "range", "search", "tel", "time", "url", "week"];
    //    var d = $(event.srcElement || event.target);
    //    var disabled = d.prop("readonly") || d.prop("disabled");
    //    if (!disabled) {
    //        if (d[0].isContentEditable) {
    //            doPrevent = false;
    //        } else if (d.is("input")) {
    //            var type = d.attr("type");
    //            if (type) {
    //                type = type.toLowerCase();
    //            }
    //            if (types.indexOf(type) > -1) {
    //                doPrevent = false;
    //            }
    //        } else if (d.is("textarea")) {
    //            doPrevent = false;
    //        }
    //    }
    //    if (doPrevent) {
    //        event.preventDefault();
    //        return false;
    //    }
    //}
    return true;
});

function preventDefaultOnEnter(decimalPresion, element, ControlType, Value, objectReference, regex, remove = false) {
    //var keyPressEvent = function (e) {
    //    if (e.type !== "paste") {
    //        if (element == null) {

    //            e.currentTarget.value = "";
    //        }
    //        Value = e.currentTarget.value;
    //        if (ControlType == 'VerticalSliderControl' || ControlType == 'HorizontalSliderControl') {
    //            const min = e.target.min
    //            const max = e.target.max
    //            var inputID = element.id.replace('input-text', '')
    //            var result = document.getElementById(inputID + 'sliderValue');
    //            const val = e.currentTarget.value;
    //            result.innerHTML = val;
    //            const progress = (val - min) * 100 / (max - min);
    //            e.target.style.background = `linear-gradient(to right, var(--primary-app-color) ${progress}%, #ccc ${progress}%)`;
    //        }
    //        objectReference.invokeMethodAsync("OnValueChangedAsync", e.currentTarget.value);
    //    }
        return true;
    }

    
    //var pasteEvent = function (e) {

    //    let data = e.clipboardData.getData('Text');
    //    //e.target.value = data;
    //    if (e.type == "paste" && data != "") {

    //        if (regex && regex !== '') {

    //            let myre = new RegExp(regex);
    //            if (!myre.test(data)) {
    //                e.preventDefault();
    //                // e.target.value = "";
    //                e.clipboardData.setData('text/plain', '');
    //                return false;
    //            }
    //            // return true;
    //        }
    //        else {

    //            for (i = 0; i < data.length; i++) {

    //                var code = data.charCodeAt(i);
    //                var key = data[i];
    //                if (ControlType === 'NumericControl') {
    //                    if ((code >= 48 && code < 58) || key === 189) {
    //                        if ((data.includes('-') && key === 189)) {
    //                            e.preventDefault()
    //                            e.clipboardData.setData('text/plain', '');
    //                            return false;
    //                        }
    //                        objectReference.invokeMethodAsync("OnValueChangedAsync", e.target.value);
    //                        return true;
    //                    }
    //                    else {
    //                        e.preventDefault();
    //                        e.clipboardData.setData('text/plain', '');
    //                        return false;
    //                    }
    //                }
    //                else if (ControlType === 'DecimalControl') {
    //                    if (((code >= 48 && code < 58) || key === 190 || key === 189)) {
    //                        if (data.includes('-') && key === 189) {
    //                            e.preventDefault()
    //                            e.clipboardData.setData('text/plain', '');
    //                            return false;
    //                        }
    //                        if ((!data && key === 190) || (data.includes('.') && key === 190)) {
    //                            e.preventDefault()
    //                            e.clipboardData.setData('text/plain', '');
    //                            return false;
    //                        }
    //                        var splitValue = data && data.split('.');
    //                        if (splitValue && splitValue[1] && splitValue[1].length > decimalPresion) {
    //                            e.preventDefault()
    //                            e.clipboardData.setData('text/plain', '');
    //                            // e.target.value = "";
    //                            return false;
    //                        }
    //                        objectReference.invokeMethodAsync("OnValueChangedAsync", e.target.value);
    //                        return true;
    //                    }
    //                    else {
    //                        e.preventDefault();
    //                        e.clipboardData.setData('text/plain', '');
    //                        return false;
    //                    }
    //                }

    //                else if (ControlType === 'PinCodeControl') {
    //                    if (code >= 48 && code <= 57) {
    //                        objectReference.invokeMethodAsync("PincodeKeyPress", data).then(result => {

    //                            if (result) {
    //                                e.preventDefault()
    //                                e.clipboardData.setData('text/plain', '');
    //                                return false;
    //                            }
    //                            objectReference.invokeMethodAsync("OnValueChangedAsync", e.target.value);
    //                            return true;
    //                        });
    //                    }
    //                    else {
    //                        e.preventDefault()
    //                        e.clipboardData.setData('text/plain', '');
    //                        return false;
    //                    }
    //                }
    //            }
    //            // return true;
    //        }
    //    }

    //    objectReference.invokeMethodAsync("OnValueChangedAsync", data);
    //    return true;
    //}
    var preventDefaultOnEnterFunction = function (e) {
    //    let keyCharCode;
    //    let key = e.key;
    //    if (typeof (key) != "undefined") {
    //        keyCharCode = key.charCodeAt(0);
    //    }
    //    if (e.keyCode === 8 || e.keyCode === 9 || e.keyCode === 13 || e.keyCode === 46 || e.keyCode === 18 || e.keyCode === 37 || e.keyCode === 38 || e.keyCode === 39 || e.keyCode === 40) {
    //        return true;
    //    }
    //    if (regex && regex !== '') {
    //        let myRe = new RegExp(regex);
    //        if (!myRe.source(e.key)) {
    //            e.preventDefault()
    //            return false;
    //        }
    //        return true;
    //    }
    //    else {
    //        if (ControlType === 'NumericControl') {

    //            if ((keyCharCode >= 48 && keyCharCode < 58) || e.keyCode === 189) {
    //                if ((e.currentTarget.value.includes('-') && e.keyCode === 189)) {
    //                    e.preventDefault()
    //                    return false;
    //                }
    //                return true;
    //            }
    //            else {
    //                e.preventDefault()
    //                return false;
    //            }
    //        }
    //        else if (ControlType === 'AlphaControl' && (e.keyCode < 65 || e.keyCode > 90)) {
    //            e.preventDefault()
    //            return false;
    //        }
    //        else if (ControlType === 'AlphaNumericControl' && (keyCharCode < 48 || keyCharCode > 57) || (e.keyCode < 65 && e.keyCode > 90)) {
    //            e.preventDefault()
    //            return false;
    //        }
    //        else if (ControlType === 'DecimalControl') {
    //            if (((keyCharCode >= 48 && keyCharCode < 58) || e.keyCode === 190 || e.keyCode === 189)) {
    //                if (e.currentTarget.value.includes('-') && e.keyCode === 189) {
    //                    e.preventDefault()
    //                    e.clipboardData.setData('text/plain', '');
    //                    return false;
    //                }
    //                if ((!e.currentTarget.value && e.keyCode === 190) || (e.currentTarget.value.includes('.') && e.keyCode === 190)) {
    //                    e.preventDefault()
    //                    return false;
    //                }
    //                var splitValue = e.currentTarget.value && e.currentTarget.value.split('.');
    //                if (splitValue && splitValue[1] && splitValue[1].length >= decimalPresion) {
    //                    e.preventDefault()
    //                    return false;
    //                }
    //                return true;
    //            }
    //            else {
    //                e.preventDefault();
    //                return false;
    //            }
    //        }
    //        else if (ControlType === 'PinCodeControl') {
    //            if (keyCharCode >= 48 && keyCharCode <= 57) {
    //                objectReference.invokeMethodAsync("PincodeKeyPress", e.key).then(result => {

    //                    if (result) {
    //                        e.preventDefault()
    //                        return false;
    //                    }
    //                    return true;
    //                });
    //            }
    //            else {
    //                e.preventDefault()
    //                return false;
    //            }
    //        }
    //    }
    //}
    //if (remove) {
    //    element.removeEventListener('keydown', preventDefaultOnEnterFunction, false);

    //}
    //else {
    //    if (ControlType == 'VerticalSliderControl' || ControlType == 'HorizontalSliderControl') {
    //        var inputID = element.id.replace('input-text', '')
    //        var result = document.getElementById(inputID + 'sliderValue');
    //        result.innerHTML = Value
    //    }
    //    element.value = Value;
    //    element.addEventListener('input', keyPressEvent, false);
    //    element.addEventListener('keydown', preventDefaultOnEnterFunction, false);
    //    element.addEventListener('paste', pasteEvent, false);
    //}
        return true;
}